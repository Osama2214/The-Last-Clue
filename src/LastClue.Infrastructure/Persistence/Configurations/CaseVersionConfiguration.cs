using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class CaseVersionConfiguration : IEntityTypeConfiguration<CaseVersion>
{
    public void Configure(EntityTypeBuilder<CaseVersion> builder)
    {
        builder.ToTable("CaseVersions", table =>
            table.HasCheckConstraint("CK_CaseVersions_ReviewStatus", "[ReviewStatus] IN ('Draft', 'Reviewed', 'Published', 'Retired')"));
        builder.Property(x => x.ReviewStatus).HasConversion<string>().HasColumnType("varchar(16)");
        builder.Property(x => x.ReviewedBy).HasMaxLength(450);
        builder.HasIndex(x => new { x.TemplateId, x.VersionNo }).IsUnique();
        builder.HasIndex(x => x.TemplateId).IsUnique().HasFilter("[ReviewStatus] = 'Published'");
        builder.HasOne<CaseTemplate>().WithMany().HasForeignKey(x => x.TemplateId);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ReviewedBy);
    }
}
