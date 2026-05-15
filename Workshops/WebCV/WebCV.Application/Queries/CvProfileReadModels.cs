namespace WebCV.Application.Queries;

public sealed record CvProfileResponse(
    Guid Id,
    string FullName,
    string Title,
    string Summary,
    string Location,
    string Email,
    string Phone,
    IReadOnlyList<SocialLinkResponse> SocialLinks,
    IReadOnlyList<CvSectionResponse> Sections);

public sealed record SocialLinkResponse(string Label, string Url);

public sealed record CvSectionResponse(
    string Heading,
    string Layout,
    IReadOnlyList<CvSectionItemResponse> Items);

public sealed record CvSectionItemResponse(
    string Title,
    string Subtitle,
    string Period,
    string Description,
    IReadOnlyList<string> Tags);
