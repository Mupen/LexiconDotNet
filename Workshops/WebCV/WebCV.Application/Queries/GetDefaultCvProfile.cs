using WebCV.Application.Interfaces;
using WebCV.Domain.Entities;

namespace WebCV.Application.Queries;

public sealed class GetDefaultCvProfile
{
    private readonly ICvProfileRepository _repository;

    public GetDefaultCvProfile(ICvProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<CvProfileResponse?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetDefaultProfileAsync(cancellationToken);
        return profile is null ? null : Map(profile);
    }

    private static CvProfileResponse Map(CvProfile profile)
    {
        return new CvProfileResponse(
            profile.Id,
            profile.FullName,
            profile.Title,
            profile.Summary,
            profile.Location,
            profile.Email,
            profile.Phone,
            profile.SocialLinks
                .OrderBy(link => link.SortOrder)
                .Select(link => new SocialLinkResponse(link.Label, link.Url))
                .ToList(),
            profile.Sections
                .OrderBy(section => section.SortOrder)
                .Select(section => new CvSectionResponse(
                    section.Heading,
                    section.Layout,
                    section.Items
                        .OrderBy(item => item.SortOrder)
                        .Select(item => new CvSectionItemResponse(
                            item.Title,
                            item.Subtitle,
                            item.Period,
                            item.Description,
                            SplitTags(item.Tags)))
                        .ToList()))
                .ToList());
    }

    private static IReadOnlyList<string> SplitTags(string tags)
    {
        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
