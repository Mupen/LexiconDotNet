using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebCV.Domain.Entities;

namespace WebCV.Infrastructure.Persistence;

public static class WebCvSeedData
{
    public static async Task SeedDevelopmentDataAsync(WebCvDbContext dbContext, string seedFilePath)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(seedFilePath);

        if (await dbContext.CvProfiles.AnyAsync())
            return;

        var profile = await LoadProfileFromJsonAsync(seedFilePath);

        await dbContext.CvProfiles.AddAsync(profile);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<CvProfile> LoadProfileFromJsonAsync(string seedFilePath)
    {
        if (!File.Exists(seedFilePath))
            throw new FileNotFoundException("CV seed data file was not found.", seedFilePath);

        await using var stream = File.OpenRead(seedFilePath);
        var seed = await JsonSerializer.DeserializeAsync<SeedCvProfile>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (seed is null)
            throw new InvalidOperationException("CV seed data file is empty or invalid.");

        return seed.ToDomain();
    }

    private sealed record SeedCvProfile(
        string FullName,
        string Title,
        string Summary,
        string Location,
        string Email,
        string Phone,
        IReadOnlyList<SeedSocialLink> SocialLinks,
        IReadOnlyList<SeedSection> Sections)
    {
        public CvProfile ToDomain()
        {
            var profile = new CvProfile(FullName, Title, Summary, Location, Email, Phone);

            foreach (var socialLink in SocialLinks.OrderBy(link => link.SortOrder))
            {
                profile.AddSocialLink(socialLink.Label, socialLink.Url, socialLink.SortOrder);
            }

            foreach (var sectionSeed in Sections.OrderBy(section => section.SortOrder))
            {
                var section = profile.AddSection(sectionSeed.Heading, sectionSeed.Layout, sectionSeed.SortOrder);

                foreach (var item in sectionSeed.Items.OrderBy(item => item.SortOrder))
                {
                    // Tags stay easy to edit as JSON arrays, while the domain stores them as display text.
                    section.AddItem(
                        item.Title,
                        item.Subtitle,
                        item.Period,
                        item.Description,
                        string.Join(", ", item.Tags ?? []),
                        item.SortOrder);
                }
            }

            return profile;
        }
    }

    private sealed record SeedSocialLink(string Label, string Url, int SortOrder);

    private sealed record SeedSection(
        string Heading,
        string Layout,
        int SortOrder,
        IReadOnlyList<SeedSectionItem> Items);

    private sealed record SeedSectionItem(
        string Title,
        string Subtitle,
        string Period,
        string Description,
        IReadOnlyList<string>? Tags,
        int SortOrder);
}
