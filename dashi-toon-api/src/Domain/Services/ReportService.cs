namespace DashiToon.Api.Domain.Services;

public class ReportService
{
    public static void MuteUser(IDomainUser user, int mutedDurationInDays)
    {
        if (mutedDurationInDays <= 0)
        {
            throw new ArgumentException("Muted duration must be greater than zero");
        }

        DateTimeOffset muteUntil = user.MuteUntil?.AddDays(mutedDurationInDays) ??
                                   DateTimeOffset.UtcNow.AddDays(mutedDurationInDays);

        user.Mute(muteUntil);
    }

    public static void RestrictUser(IDomainUser user, int restrictedDurationInDays)
    {
        if (restrictedDurationInDays <= 0)
        {
            throw new ArgumentException("Restricted duration must be greater than zero");
        }

        DateTimeOffset restrictUntil = user.RestrictPublishUntil?.AddDays(restrictedDurationInDays) ??
                                       DateTimeOffset.UtcNow.AddDays(restrictedDurationInDays);

        user.RestrictPublish(restrictUntil);
    }

    public static bool IsUserAllowedToCommentOrReview(IDomainUser user)
    {
        return user.MuteUntil is null ||
               (user.MuteUntil is not null && user.MuteUntil < DateTimeOffset.UtcNow);
    }

    public static bool IsUserAllowedToPublish(IDomainUser user)
    {
        return user.RestrictPublishUntil is null ||
               (user.RestrictPublishUntil is not null && user.RestrictPublishUntil < DateTimeOffset.UtcNow);
    }
}
