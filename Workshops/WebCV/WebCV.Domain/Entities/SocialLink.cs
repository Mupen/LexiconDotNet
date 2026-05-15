namespace WebCV.Domain.Entities;

public sealed class SocialLink
{
    private SocialLink()
    {
    }

    public SocialLink(Guid cvProfileId, string label, string url, int sortOrder)
    {
        Id = Guid.NewGuid();
        CvProfileId = cvProfileId;
        Update(label, url, sortOrder);
    }

    public Guid Id { get; private set; }
    public Guid CvProfileId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    public void Update(string label, string url, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException("Url must be an absolute URL.", nameof(url));

        Label = label.Trim();
        Url = url.Trim();
        SortOrder = sortOrder;
    }
}
