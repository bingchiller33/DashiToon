using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.Series.EventHandlers.SeriesCreated;

public sealed class SeriesCreatedEventHandler : INotificationHandler<SeriesCreatedEvent>
{
    // private readonly ISearchService _searchService;
    //
    // public SeriesCreatedEventHandler(ISearchService searchService)
    // {
    //     _searchService = searchService;
    // }

    //TODO: Refactor Series id to use GUID instead of int. Data implementation constraint seriesId is not initialized at the time of index
    public Task Handle(SeriesCreatedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // await _searchService.IndexSeriesAsync(notification.Series);
    }
}
