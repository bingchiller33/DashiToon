using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Application.Users.Commands.CheckinUser;
using DashiToon.Api.Application.Users.Commands.CompleteMission;
using DashiToon.Api.Application.Users.Commands.DowngradeSubscriptionTier;
using DashiToon.Api.Application.Users.Commands.ProcessUpgradeSubscriptionTier;
using DashiToon.Api.Application.Users.Commands.UnsubscribeSeries;
using DashiToon.Api.Application.Users.Commands.UpgradeSubscriptionTier;
using DashiToon.Api.Application.Users.Queries.GetFollowedSeries;
using DashiToon.Api.Application.Users.Queries.GetKanaTotals;
using DashiToon.Api.Application.Users.Queries.GetKanaTransactions;
using DashiToon.Api.Application.Users.Queries.GetMissions;
using DashiToon.Api.Application.Users.Queries.GetPaymentHistory;
using DashiToon.Api.Application.Users.Queries.GetSeriesFollowDetail;
using DashiToon.Api.Application.Users.Queries.GetSeriesFollowStatus;
using DashiToon.Api.Application.Users.Queries.GetSeriesSubscription;
using DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;
using DashiToon.Api.Application.Users.Queries.GetUserMetadatas;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Namotion.Reflection;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace DashiToon.Api.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SignInWithGoogle, "signin-google")
            .MapGet(GoogleResponse, "callback")
            .MapPost(SetUsernameAndLogin, "set-username-and-login")
            .MapIdentityApi();

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetUserInfo, "info")
            .MapGet(GetUserMetadata, "metadata")
            .MapGet(GetKanaTotals, "kanas")
            .MapPost(Logout, "logout")
            .MapPost(Checkin, "checkin");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetKanaTransactions, "kana-transactions")
            .MapGet(GetMissions, "missions")
            .MapPut(CompleteMission, "missions/{id}");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetSubscribedSeries, "subscriptions")
            .MapGet(GetSeriesSubscription, "subscriptions/series/{id}")
            .MapPut(CancelSubscription, "subscriptions/{id}/cancel")
            .MapPut(UpgradeSubscription, "subscriptions/{id}/upgrade")
            .MapPost(ProcessUpgradeSubscription, "subscriptions/{orderId}/upgrade/capture")
            .MapPut(DowngradeSubscription, "subscriptions/{id}/downgrade");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetPaymentHistory, "payments");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(IsFollowedSeries, "followed-series/{id}")
            .MapGet(GetSeriesFollowDetail, "followed-series2/{id}")
            .MapGet(GetFollowedSeries, "followed-series");
    }

    public IResult SignInWithGoogle(SignInManager<ApplicationUser> signInManager, string? returnUrl = null)
    {
        string redirectUrl = $"api/Users/callback?returnUrl={returnUrl}";
        AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            redirectUrl);

        return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
    }

    public async Task<IResult> GoogleResponse(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        string? returnUrl = null)
    {
        returnUrl ??= "/";

        AuthenticateResult result = await httpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        if (!result.Succeeded)
        {
            return Results.BadRequest("Google authentication failed.");
        }

        List<Claim>? claims = result.Principal.Identities
            .FirstOrDefault()?
            .Claims
            .ToList();

        string? email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Results.BadRequest("Google authentication did not return an email address.");
        }

        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            string token = GenerateSerUsernameToken(email, configuration);

            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            query["token"] = token;
            query["returnUrl"] = returnUrl;

            UriBuilder setUsernameUri = new(new Uri(configuration["FrontEndHost"]!))
            {
                Path = "set-username", Query = query.ToString()
            };

            return Results.Redirect(setUsernameUri.Uri.ToString());
        }

        await signInManager.SignInAsync(user, false);

        UriBuilder uriBuilder = new(new Uri(new Uri(configuration["FrontEndHost"]!), returnUrl));

        return Results.Redirect(uriBuilder.Uri.ToString());
    }

    private string GenerateSerUsernameToken(string email, IConfiguration configuration)
    {
        JsonWebTokenHandler tokenHandler = new();
        byte[] key = Encoding.UTF8.GetBytes(configuration["Authentication:Jwt:Secret"]!);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim("TokenPurpose", "SetUsername")
            ]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.CreateToken(tokenDescriptor);
    }

    public async Task<Results<RedirectHttpResult, BadRequest<IEnumerable<IdentityError>>>> SetUsernameAndLogin(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        string token,
        SetUsernameRequest request)
    {
        ClaimsIdentity? claimsIdentity = await ValidateSetUsernameToken(token, configuration);

        if (claimsIdentity == null || !claimsIdentity.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            List<IdentityError> errors = new()
            {
                new IdentityError
                {
                    Code = "InvalidToken", Description = "The token is invalid or expired. Please login again."
                }
            };

            return TypedResults.BadRequest(errors.AsEnumerable());
        }

        string email = claimsIdentity.Claims.First(x => x.Type == ClaimTypes.Email).Value;

        // Check if the user already exists
        ApplicationUser? user = await userManager.FindByEmailAsync(email);

        if (user != null)
        {
            List<IdentityError> errors = new()
            {
                new IdentityError { Code = "UserAlreadyExists", Description = "User already exists." }
            };

            return TypedResults.BadRequest(
                errors.AsEnumerable()
            );
        }

        // Create a new user
        user = new ApplicationUser { UserName = request.Username, Email = email, EmailConfirmed = true };

        IdentityResult createUserResult = await userManager.CreateAsync(user);

        if (!createUserResult.Succeeded)
        {
            return TypedResults.BadRequest(createUserResult.Errors);
        }

        await signInManager.SignInAsync(user, false);

        UriBuilder uriBuilder = new(new Uri(new Uri(configuration["FrontEndHost"]!), request.ReturnUrl ?? "/"));

        return TypedResults.Redirect(uriBuilder.Uri.ToString());
    }

    private async Task<ClaimsIdentity?> ValidateSetUsernameToken(string token, IConfiguration configuration)
    {
        JsonWebTokenHandler tokenHandler = new();
        byte[] key = Encoding.UTF8.GetBytes(configuration["Authentication:Jwt:Secret"]!);

        try
        {
            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            TokenValidationResult? validationResult =
                await tokenHandler.ValidateTokenAsync(token, validationParameters);

            return validationResult.IsValid ? validationResult.ClaimsIdentity : null;
        }
        catch
        {
            return null;
        }
    }


    public async Task<IResult> GetUserInfo(ClaimsPrincipal claimsPrincipal, UserManager<ApplicationUser> userManager)
    {
        if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
        {
            return Results.NotFound();
        }

        return Results.Ok(new UserInfoVm(
            user.Id,
            user.UserName,
            user.Email,
            await userManager.GetRolesAsync(user)));
    }

    public async Task<UserMetadata> GetUserMetadata(ISender sender)
    {
        return await sender.Send(new GetUserMetadataQuery());
    }

    public async Task<KanaTotalsVm> GetKanaTotals(ISender sender)
    {
        return await sender.Send(new GetKanaTotalsQuery());
    }

    public async Task<IResult> Logout(
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        await signInManager.SignOutAsync();

        Uri uri = new(configuration["FrontEndHost"]!);

        return Results.Redirect(uri.ToString());
    }

    public async Task<IResult> Checkin(ISender sender)
    {
        await sender.Send(new CheckinUserCommand());

        return Results.NoContent();
    }

    public async Task<PaginatedList<KanaTransactionVm>> GetKanaTransactions(
        ISender sender,
        string type,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetKanaTransactionsQuery(type, pageNumber, pageSize));
    }

    public async Task<List<UserSubscriptionVm>> GetSubscribedSeries(ISender sender)
    {
        return await sender.Send(new GetSubscriptionsQuery());
    }

    public async Task<List<MissionVm>> GetMissions(ISender sender)
    {
        return await sender.Send(new GetMissionsQuery());
    }

    public async Task<IResult> CompleteMission(ISender sender, Guid id)
    {
        await sender.Send(new CompleteMissionCommand(id));

        return Results.NoContent();
    }

    public async Task<PaginatedList<PaymentVm>> GetPaymentHistory(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetPaymentHistoryQuery(pageNumber, pageSize));
    }

    public async Task<SeriesSubscriptionVm?> GetSeriesSubscription(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesSubscriptionQuery(id));
    }

    public async Task<IResult> CancelSubscription(ISender sender, Guid id)
    {
        await sender.Send(new UnsubscribeSeriesCommand(id));

        return Results.NoContent();
    }

    public async Task<IResult> UpgradeSubscription(ISender sender, Guid id, UpgradeSubscriptionTiersCommand command)
    {
        OrderResult result = await sender.Send(command);

        return Results.Json(data: result.Data, statusCode: result.StatusCode);
    }

    public async Task<IResult> ProcessUpgradeSubscription(ISender sender, string orderId)
    {
        OrderResult result = await sender.Send(new ProcessUpgradeSubscriptionTiersCommand(orderId));

        return Results.Json(data: result.Data, statusCode: result.StatusCode);
    }

    public async Task<IResult> DowngradeSubscription(ISender sender, Guid id, DowngradeSubscriptionTierCommand command)
    {
        await sender.Send(command);

        return Results.NoContent();
    }

    public async Task<bool> IsFollowedSeries(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesFollowStatusQuery(id));
    }

    public async Task<PaginatedList<FollowedSeriesVm>> GetFollowedSeries(
        ISender sender,
        bool? hasRead,
        string sortBy,
        string sortOrder,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetFollowedSeriesQuery(hasRead, sortBy, sortOrder, pageNumber, pageSize));
    }

    public async Task<SeriesFollowDetailVm?> GetSeriesFollowDetail(ISender sender, int id)
    {
        return await sender.Send(new GetSeriesFollowDetailQuery(id));
    }
}

public sealed record UserInfoVm(
    string UserId,
    string? Username,
    string? Email,
    IList<string> Roles);

public sealed record SetUsernameRequest(
    string Username,
    string? ReturnUrl
);

public static class IdentityApiEndpointRouteBuilderExtensions
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    /// <summary>
    ///     Add endpoints for registering, logging in, and logging out using ASP.NET Core Identity.
    /// </summary>
    /// <typeparam name="ApplicationUser">
    ///     The type describing the user. This should match the generic parameter in
    ///     <see cref="UserManager{ApplicationUser}" />.
    /// </typeparam>
    /// <param name="endpoints">
    ///     The <see cref="IEndpointRouteBuilder" /> to add the identity endpoints to.
    ///     Call <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)" /> to add a prefix to all
    ///     the endpoints.
    /// </param>
    /// <returns>An <see cref="IEndpointConventionBuilder" /> to further customize the added endpoints.</returns>
    public static IEndpointConventionBuilder MapIdentityApi(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        TimeProvider timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions =
            endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        IEmailSender<ApplicationUser> emailSender =
            endpoints.ServiceProvider.GetRequiredService<IEmailSender<ApplicationUser>>();
        LinkGenerator linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

        // We'll figure out a unique endpoint name based on the final route pattern during endpoint generation.
        string? confirmEmailEndpointName = null;

        RouteGroupBuilder routeGroup = endpoints.MapGroup("");

        // NOTE: We cannot inject UserManager<ApplicationUser> directly because the ApplicationUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        routeGroup.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] RegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapIdentityApi)} requires a user store with email support.");
            }

            IUserStore<ApplicationUser> userStore = sp.GetRequiredService<IUserStore<ApplicationUser>>();
            IUserEmailStore<ApplicationUser> emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            string email = registration.Email;

            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
            }

            ApplicationUser user = new();
            await emailStore.SetUserNameAsync(user, registration.Username, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            IdentityResult result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await SendConfirmationEmailAsync(user, userManager, context, email);
            return TypedResults.Ok();
        });

        routeGroup.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
        ([FromBody] LoginRequest login,
            [FromQuery] bool? useCookies,
            [FromQuery] bool? useSessionCookies,
            [FromServices] IServiceProvider sp) =>
        {
            SignInManager<ApplicationUser> signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();

            bool useCookieScheme = useCookies == true || useSessionCookies == true;
            bool isPersistent = useCookies == true && useSessionCookies != true;
            signInManager.AuthenticationScheme =
                useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            ApplicationUser? user = await signInManager.UserManager.FindByEmailAsync(login.Email);

            SignInResult result = await signInManager.PasswordSignInAsync(user?.UserName ?? string.Empty,
                login.Password,
                isPersistent,
                true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent,
                        isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
        });

        routeGroup.MapPost("/refresh",
            async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>
                ([FromBody] RefreshRequest refreshRequest, [FromServices] IServiceProvider sp) =>
            {
                SignInManager<ApplicationUser> signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
                ISecureDataFormat<AuthenticationTicket> refreshTokenProtector =
                    bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
                AuthenticationTicket? refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

                // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
                if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                    timeProvider.GetUtcNow() >= expiresUtc ||
                    await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not ApplicationUser user)

                {
                    return TypedResults.Challenge();
                }

                ClaimsPrincipal newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
            });

        routeGroup.MapGet("/confirmEmail", async Task<Results<RedirectHttpResult, UnauthorizedHttpResult>>
            ([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail,
                [FromServices] IServiceProvider sp) =>
            {
                UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();

                if (await userManager.FindByIdAsync(userId) is not { } user)
                {
                    // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
                    return TypedResults.Unauthorized();
                }

                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                }
                catch (FormatException)
                {
                    return TypedResults.Unauthorized();
                }

                IdentityResult result;

                if (string.IsNullOrEmpty(changedEmail))
                {
                    result = await userManager.ConfirmEmailAsync(user, code);
                }
                else
                {
                    result = await userManager.ChangeEmailAsync(user, changedEmail, code);
                }

                if (!result.Succeeded)
                {
                    return TypedResults.Unauthorized();
                }

                return TypedResults.Redirect(new UriBuilder(new Uri(new Uri(configuration["FrontEndHost"]!), "login"))
                    .Uri.ToString());
            })
            .Add(endpointBuilder =>
            {
                string? finalPattern = ((RouteEndpointBuilder)endpointBuilder).RoutePattern.RawText;
                confirmEmailEndpointName = $"{nameof(MapIdentityApi)}-{finalPattern}";
                endpointBuilder.Metadata.Add(new EndpointNameMetadata(confirmEmailEndpointName));
            });

        routeGroup.MapPost("/resendConfirmationEmail", async Task<Ok>
        ([FromBody] ResendConfirmationEmailRequest resendRequest, HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
            {
                return TypedResults.Ok();
            }

            await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email);
            return TypedResults.Ok();
        });

        routeGroup.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ForgotPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            ApplicationUser? user = await userManager.FindByEmailAsync(resetRequest.Email);

            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();

            if (user is not null && await userManager.IsEmailConfirmedAsync(user))
            {
                await SendResetPasswordAsync(user, userManager, resetRequest.Email, configuration["FrontEndHost"]!);
            }

            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return TypedResults.Ok();
        });

        routeGroup.MapPost("/resetPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ResetPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByEmailAsync(resetRequest.Email);

            if (user is null || !await userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                // returned a 400 for an invalid code given a valid user email.
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
            }

            IdentityResult result;
            try
            {
                string code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
        });

        RouteGroupBuilder accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

        accountGroup.MapPost("/2fa", async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>>
        (ClaimsPrincipal claimsPrincipal, [FromBody] TwoFactorRequest tfaRequest,
            [FromServices] IServiceProvider sp) =>
        {
            SignInManager<ApplicationUser> signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            UserManager<ApplicationUser> userManager = signInManager.UserManager;
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            if (tfaRequest.Enable == true)
            {
                if (tfaRequest.ResetSharedKey)
                {
                    return CreateValidationProblem("CannotResetSharedKeyAndEnable",
                        "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
                }

                if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
                {
                    return CreateValidationProblem("RequiresTwoFactor",
                        "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
                }

                if (!await userManager.VerifyTwoFactorTokenAsync(user,
                        userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
                {
                    return CreateValidationProblem("InvalidTwoFactorCode",
                        "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
                }

                await userManager.SetTwoFactorEnabledAsync(user, true);
            }
            else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
            {
                await userManager.SetTwoFactorEnabledAsync(user, false);
            }

            if (tfaRequest.ResetSharedKey)
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
            }

            string[]? recoveryCodes = null;
            if (tfaRequest.ResetRecoveryCodes ||
                (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
            {
                IEnumerable<string>? recoveryCodesEnumerable =
                    await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                recoveryCodes = recoveryCodesEnumerable?.ToArray();
            }

            if (tfaRequest.ForgetMachine)
            {
                await signInManager.ForgetTwoFactorClientAsync();
            }

            string? key = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                key = await userManager.GetAuthenticatorKeyAsync(user);

                if (string.IsNullOrEmpty(key))
                {
                    throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
                }
            }

            return TypedResults.Ok(new TwoFactorResponse
            {
                SharedKey = key,
                RecoveryCodes = recoveryCodes,
                RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
                IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user)
            });
        });

        accountGroup.MapGet("/info", async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
            (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            IImageStore imageStore = sp.GetRequiredService<IImageStore>();

            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(await CreateInfoResponseAsync(user, imageStore));
        });

        accountGroup.MapPost("/info", async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>>
        (ClaimsPrincipal claimsPrincipal, [FromBody] InfoRequest infoRequest, HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            IImageStore imageStore = sp.GetRequiredService<IImageStore>();

            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !_emailAddressAttribute.IsValid(infoRequest.NewEmail))
            {
                return CreateValidationProblem(
                    IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
            }

            if (!string.IsNullOrEmpty(infoRequest.NewPassword))
            {
                if (string.IsNullOrEmpty(infoRequest.OldPassword))
                {
                    return CreateValidationProblem("OldPasswordRequired",
                        "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
                }

                IdentityResult changePasswordResult = await userManager.ChangePasswordAsync(
                    user,
                    infoRequest.OldPassword,
                    infoRequest.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return CreateValidationProblem(changePasswordResult);
                }
            }

            if (!string.IsNullOrEmpty(infoRequest.NewEmail))
            {
                string? email = await userManager.GetEmailAsync(user);

                if (email != infoRequest.NewEmail)
                {
                    await SendConfirmationEmailAsync(user, userManager, context, infoRequest.NewEmail, true);
                }
            }

            return TypedResults.Ok(await CreateInfoResponseAsync(user, imageStore));
        });

        async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager,
            HttpContext context,
            string email, bool isChange = false)
        {
            if (confirmEmailEndpointName is null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            string code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string userId = await userManager.GetUserIdAsync(user);
            RouteValueDictionary routeValues = new() { ["userId"] = userId, ["code"] = code };

            if (isChange)
            {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }

            string confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
                                     ?? throw new NotSupportedException(
                                         $"Could not find endpoint named '{confirmEmailEndpointName}'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        }

        async Task SendResetPasswordAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, string email,
            string host)
        {
            string code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            Dictionary<string, string?> routeValues = new() { ["email"] = email, ["resetCode"] = code };

            string resetPasswordUrl = QueryHelpers.AddQueryString(
                new Uri(new Uri(host), "reset-password").ToString(),
                routeValues);

            await emailSender.SendPasswordResetLinkAsync(user, email, HtmlEncoder.Default.Encode(resetPasswordUrl));
        }

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]> { { errorCode, [errorDescription] } });
    }

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        Dictionary<string, string[]> errorDictionary = new(1);

        foreach (IdentityError error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out string[]? descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync(ApplicationUser user, IImageStore imageStore)
    {
        return new InfoResponse
        {
            Avatar = string.IsNullOrEmpty(user.Avatar)
                ? await imageStore.GetUrl("avatars/default.png", DateTime.UtcNow.AddMinutes(2))
                : await imageStore.GetUrl($"avatars/{user.Avatar}", DateTime.UtcNow.AddMinutes(2)),
            UserName = user.UserName ?? throw new NotSupportedException("User must have a username"),
            Email = user.Email ?? throw new NotSupportedException("User must have an email address."),
            IsEmailConfirmed = user.EmailConfirmed
        };
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention)
        {
            InnerAsConventionBuilder.Add(convention);
        }

        public void Finally(Action<EndpointBuilder> finallyConvention)
        {
            InnerAsConventionBuilder.Finally(finallyConvention);
        }
    }
}

public sealed class RegisterRequest
{
    public required string Username { get; set; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public sealed class InfoRequest
{
    public string? NewEmail { get; init; }
    public string? NewPassword { get; init; }
    public string? OldPassword { get; init; }
}

public sealed class InfoResponse
{
    public required string? Avatar { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required bool IsEmailConfirmed { get; init; }
}
