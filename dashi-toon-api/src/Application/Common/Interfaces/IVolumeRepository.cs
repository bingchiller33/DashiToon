using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IVolumeRepository
{
    Task<IEnumerable<Volume>> FindVolumesBySeriesId(int seriesId);
}
