using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Notifications.Queries.GetUserNotifications;

public sealed record NotificationVm(
    Guid Id,
    string Title,
    string Content,
    bool IsRead,
    string Timestamp,
    int? ChapterId,
    int? VolumeId,
    int? SeriesId,
    SeriesType? Type);
