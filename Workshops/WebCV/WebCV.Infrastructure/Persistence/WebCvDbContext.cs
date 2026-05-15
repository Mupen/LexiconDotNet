using Microsoft.EntityFrameworkCore;
using WebCV.Domain.Entities;

namespace WebCV.Infrastructure.Persistence;

public sealed class WebCvDbContext : DbContext
{
    public WebCvDbContext(DbContextOptions<WebCvDbContext> options)
        : base(options)
    {
    }

    public DbSet<CvProfile> CvProfiles => Set<CvProfile>();
    public DbSet<CvSection> CvSections => Set<CvSection>();
    public DbSet<CvSectionItem> CvSectionItems => Set<CvSectionItem>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CvProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.FullName).HasMaxLength(160).IsRequired();
            entity.Property(profile => profile.Title).HasMaxLength(160).IsRequired();
            entity.Property(profile => profile.Summary).HasMaxLength(1200).IsRequired();
            entity.Property(profile => profile.Location).HasMaxLength(120).IsRequired();
            entity.Property(profile => profile.Email).HasMaxLength(180).IsRequired();
            entity.Property(profile => profile.Phone).HasMaxLength(80);

            entity.HasMany(profile => profile.Sections)
                .WithOne()
                .HasForeignKey(section => section.CvProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(profile => profile.SocialLinks)
                .WithOne()
                .HasForeignKey(link => link.CvProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(profile => profile.Sections)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Navigation(profile => profile.SocialLinks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CvSection>(entity =>
        {
            entity.HasKey(section => section.Id);
            entity.Property(section => section.Heading).HasMaxLength(120).IsRequired();
            entity.Property(section => section.Layout).HasMaxLength(40).IsRequired();

            entity.HasMany(section => section.Items)
                .WithOne()
                .HasForeignKey(item => item.CvSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(section => section.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CvSectionItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Subtitle).HasMaxLength(180);
            entity.Property(item => item.Period).HasMaxLength(120);
            entity.Property(item => item.Description).HasMaxLength(1200);
            entity.Property(item => item.Tags).HasMaxLength(500);
        });

        modelBuilder.Entity<SocialLink>(entity =>
        {
            entity.HasKey(link => link.Id);
            entity.Property(link => link.Label).HasMaxLength(80).IsRequired();
            entity.Property(link => link.Url).HasMaxLength(500).IsRequired();
        });
    }
}
