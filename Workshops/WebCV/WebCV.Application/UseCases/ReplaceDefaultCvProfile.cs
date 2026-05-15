using WebCV.Application.Interfaces;
using WebCV.Domain.Entities;

namespace WebCV.Application.UseCases;

public sealed class ReplaceDefaultCvProfile
{
    private readonly ICvProfileRepository _repository;

    public ReplaceDefaultCvProfile(ICvProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<CvProfile> ExecuteAsync(ReplaceCvProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = new CvProfile(
            request.FullName,
            request.Title,
            request.Summary,
            request.Location,
            request.Email,
            request.Phone);

        foreach (var link in request.SocialLinks.OrderBy(link => link.SortOrder))
        {
            profile.AddSocialLink(link.Label, link.Url, link.SortOrder);
        }

        foreach (var sectionRequest in request.Sections.OrderBy(section => section.SortOrder))
        {
            var section = profile.AddSection(sectionRequest.Heading, sectionRequest.Layout, sectionRequest.SortOrder);

            foreach (var item in sectionRequest.Items.OrderBy(item => item.SortOrder))
            {
                section.AddItem(
                    item.Title,
                    item.Subtitle,
                    item.Period,
                    item.Description,
                    string.Join(", ", item.Tags ?? []),
                    item.SortOrder);
            }
        }

        return await _repository.SaveDefaultProfileAsync(profile, cancellationToken);
    }
}

public sealed record ReplaceCvProfileRequest(
    string FullName,
    string Title,
    string Summary,
    string Location,
    string Email,
    string Phone,
    IReadOnlyList<SocialLinkRequest> SocialLinks,
    IReadOnlyList<CvSectionRequest> Sections);

public sealed record SocialLinkRequest(string Label, string Url, int SortOrder);

public sealed record CvSectionRequest(
    string Heading,
    string Layout,
    int SortOrder,
    IReadOnlyList<CvSectionItemRequest> Items);

public sealed record CvSectionItemRequest(
    string Title,
    string Subtitle,
    string Period,
    string Description,
    IReadOnlyList<string>? Tags,
    int SortOrder);
