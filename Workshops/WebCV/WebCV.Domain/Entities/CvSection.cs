namespace WebCV.Domain.Entities;

public sealed class CvSection
{
    private readonly List<CvSectionItem> _items = [];

    private CvSection()
    {
    }

    public CvSection(Guid cvProfileId, string heading, string layout, int sortOrder)
    {
        Id = Guid.NewGuid();
        CvProfileId = cvProfileId;
        Update(heading, layout, sortOrder);
    }

    public Guid Id { get; private set; }
    public Guid CvProfileId { get; private set; }
    public string Heading { get; private set; } = string.Empty;
    public string Layout { get; private set; } = "timeline";
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<CvSectionItem> Items => _items;

    public void Update(string heading, string layout, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(heading))
            throw new ArgumentException("Heading cannot be empty.", nameof(heading));

        Heading = heading.Trim();
        Layout = string.IsNullOrWhiteSpace(layout) ? "timeline" : layout.Trim();
        SortOrder = sortOrder;
    }

    public CvSectionItem AddItem(
        string title,
        string subtitle,
        string period,
        string description,
        string tags,
        int sortOrder)
    {
        var item = new CvSectionItem(Id, title, subtitle, period, description, tags, sortOrder);
        _items.Add(item);
        return item;
    }
}
