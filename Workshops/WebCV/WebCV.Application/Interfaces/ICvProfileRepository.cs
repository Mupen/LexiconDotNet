using WebCV.Domain.Entities;

namespace WebCV.Application.Interfaces;

public interface ICvProfileRepository
{
    Task<CvProfile?> GetDefaultProfileAsync(CancellationToken cancellationToken = default);

    Task<CvProfile> SaveDefaultProfileAsync(
        CvProfile profile,
        CancellationToken cancellationToken = default);
}
