using Microsoft.EntityFrameworkCore;
using WebCV.Application.Interfaces;
using WebCV.Domain.Entities;
using WebCV.Infrastructure.Persistence;

namespace WebCV.Infrastructure.Repositories;

public sealed class EfCvProfileRepository : ICvProfileRepository
{
    private readonly WebCvDbContext _dbContext;

    public EfCvProfileRepository(WebCvDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CvProfile?> GetDefaultProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CvProfiles
            .Include(profile => profile.SocialLinks)
            .Include(profile => profile.Sections)
                .ThenInclude(section => section.Items)
            .AsSplitQuery()
            .OrderBy(profile => profile.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CvProfile> SaveDefaultProfileAsync(
        CvProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CvProfiles.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CvProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }
}
