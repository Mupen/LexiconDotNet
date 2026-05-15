namespace WebCV.Domain.Entities;

public sealed class CvProfile
{
    private readonly List<CvSection> _sections = [];
    private readonly List<SocialLink> _socialLinks = [];

    private CvProfile()
    {
    }

    public CvProfile(
        string fullName,
        string title,
        string summary,
        string location,
        string email,
        string phone)
    {
        Id = Guid.NewGuid();
        UpdateHeader(fullName, title, summary, location, email, phone);
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public IReadOnlyCollection<CvSection> Sections => _sections;
    public IReadOnlyCollection<SocialLink> SocialLinks => _socialLinks;

    public void UpdateHeader(
        string fullName,
        string title,
        string summary,
        string location,
        string email,
        string phone)
    {
        FullName = RequireText(fullName, nameof(fullName));
        Title = RequireText(title, nameof(title));
        Summary = RequireText(summary, nameof(summary));
        Location = RequireText(location, nameof(location));
        Email = RequireText(email, nameof(email));
        Phone = phone.Trim();
    }

    public CvSection AddSection(string heading, string layout, int sortOrder)
    {
        var section = new CvSection(Id, heading, layout, sortOrder);
        _sections.Add(section);
        return section;
    }

    public SocialLink AddSocialLink(string label, string url, int sortOrder)
    {
        var socialLink = new SocialLink(Id, label, url, sortOrder);
        _socialLinks.Add(socialLink);
        return socialLink;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", parameterName);

        return value.Trim();
    }
}
