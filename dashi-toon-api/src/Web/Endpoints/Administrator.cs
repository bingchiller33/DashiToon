using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPack;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPackStatus;
using DashiToon.Api.Application.Administrator.KanaGoldPacks.Queries.GetKanaGoldPacks;
using DashiToon.Api.Application.Administrator.Missions.Commands.CreateMission;
using DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMission;
using DashiToon.Api.Application.Administrator.Missions.Commands.UpdateMissionStatus;
using DashiToon.Api.Application.Administrator.Missions.Queries.GetMissions;
using DashiToon.Api.Application.Administrator.Series.ExportSeries;
using DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateCommissionRate;
using DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateKanaExchangeRate;
using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetCommissionRate;
using DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetKanaExchangeRate;
using DashiToon.Api.Application.Administrator.Users.Commands.AssignRole;
using DashiToon.Api.Application.Administrator.Users.Models;
using DashiToon.Api.Application.Administrator.Users.Queries.GetRoles;
using DashiToon.Api.Application.Administrator.Users.Queries.GetUsers;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using KanaGoldPackVm = DashiToon.Api.Application.Administrator.KanaGoldPacks.Models.KanaGoldPackVm;
using MissionVm = DashiToon.Api.Application.Administrator.Missions.Models.MissionVm;

namespace DashiToon.Api.Web.Endpoints;

public class Administrator : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetKanaGoldPacksAdmin, "kana-packs")
            .MapPost(CreateKanaGoldPackAdmin, "kana-packs")
            .MapPut(UpdateKanaGoldPackAdmin, "kana-packs/{id}")
            .MapPut(UpdateKanaGoldPackStatusAdmin, "kana-packs/{id}/status");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetMissionsAdmin, "missions")
            .MapPost(CreateMissionAdmin, "missions")
            .MapPut(UpdateMissionAdmin, "missions/{id}")
            .MapPut(UpdateMissionStatusAdmin, "missions/{id}/status");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetCommissionRate, "commission-rates")
            .MapPut(UpdateCommissionRate, "commission-rates")
            .MapGet(GetKanaExchangeRate, "kana-exchange-rates")
            .MapPut(UpdateKanaExchangeRate, "kana-exchange-rates");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetUsers, "users")
            .MapPost(AssignRole, "users/{id}/assign-role")
            .MapGet(GetRoles, "roles");

        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(ExportSeries, "export-series");
    }

    public async Task<List<KanaGoldPackVm>> GetKanaGoldPacksAdmin(ISender sender)
    {
        return await sender.Send(new GetKanaGoldPackQuery());
    }

    public async Task<KanaGoldPackVm> CreateKanaGoldPackAdmin(
        ISender sender,
        CreateKanaGoldPackCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateKanaGoldPackAdmin(
        ISender sender,
        Guid id,
        UpdateKanaGoldPackCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> UpdateKanaGoldPackStatusAdmin(
        ISender sender,
        Guid id,
        UpdateKanaGoldPackStatusCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<List<MissionVm>> GetMissionsAdmin(ISender sender)
    {
        return await sender.Send(new GetMissionsQuery());
    }

    public async Task<MissionVm> CreateMissionAdmin(
        ISender sender,
        CreateMissionCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateMissionAdmin(
        ISender sender,
        Guid id,
        UpdateMissionCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<IResult> UpdateMissionStatusAdmin(
        ISender sender,
        Guid id,
        UpdateMissionStatusCommand command)
    {
        if (id != command.Id)
        {
            return Results.BadRequest();
        }

        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<CommissionRateVm> GetCommissionRate(ISender sender, CommissionType type)
    {
        return await sender.Send(new GetCommissionRateQuery(type));
    }

    public async Task<IResult> UpdateCommissionRate(
        ISender sender,
        UpdateCommissionRateCommand command)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<KanaExchangeRateVm> GetKanaExchangeRate(ISender sender)
    {
        return await sender.Send(new GetKanaExchangeRateQuery());
    }


    public async Task<IResult> UpdateKanaExchangeRate(ISender sender, UpdateKanaExchangeRateCommand command)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    public async Task<PaginatedList<UserVm>> GetUsers(
        ISender sender,
        string? userId,
        string? userName,
        string? role,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetUsersQuery(userId, userName, role, pageNumber, pageSize));
    }

    public async Task<Results<Ok<Result>, BadRequest>> AssignRole(
        ISender sender,
        string id,
        AssignRoleCommand command)
    {
        if (id != command.UserId)
        {
            return TypedResults.BadRequest();
        }

        Result? result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<List<string>> GetRoles(ISender sender)
    {
        return await sender.Send(new GetRolesQuery());
    }

    public async Task<IResult> ExportSeries(ISender sender)
    {
        return Results.File(
            await sender.Send(new ExportSeriesCommand()),
            fileDownloadName: "ExportedSeries.json",
            lastModified: DateTimeOffset.UtcNow
        );
    }
}
