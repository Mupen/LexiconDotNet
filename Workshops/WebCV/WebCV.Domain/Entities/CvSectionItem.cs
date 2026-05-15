namespace WebCV.Domain.Entities;

public sealed class CvSectionItem
{
    private CvSectionItem()
    {
    }

    public CvSectionItem(
        Guid cvSectionId,
        string title,
        string subtitle,
        string period,
        string description,
        string tags,
        int sortOrder)
    {
        Id = Guid.NewGuid();
        CvSectionId = cvSectionId;
        Update(title, subtitle, period, description, tags, sortOrder);
    }

    public Guid Id { get; private set; }
    public Guid CvSectionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Subtitle { get; private set; } = string.Empty;
    public string Period { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Tags { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    public void Update(
        string title,
        string subtitle,
        string period,
        string description,
        string tags,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Title = title.Trim();
        Subtitle = subtitle.Trim();
        Period = period.Trim();
        Description = description.Trim();
        Tags = tags.Trim();
        SortOrder = sortOrder;
    }
}
