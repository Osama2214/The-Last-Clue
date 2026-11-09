using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class CaseVariantConfiguration : IEntityTypeConfiguration<CaseVariant>
{
    public void Configure(EntityTypeBuilder<CaseVariant> builder)
    {
        builder.ToTable("CaseVariants", table =>
        {
            table.HasCheckConstraint("CK_CaseVariants_PlayerCount", "[PlayerCount] BETWEEN 4 AND 8");
            table.HasCheckConstraint("CK_CaseVariants_Language", "[Language] IN ('en', 'ar')");
            table.HasCheckConstraint("CK_CaseVariants_PayloadJson", "ISJSON([PayloadJson]) = 1");
        });
        builder.Property(x => x.Language).HasColumnType("char(2)");
        builder.HasIndex(x => new { x.CaseVersionId, x.PlayerCount, x.Language }).IsUnique();
        builder.HasOne<CaseVersion>().WithMany().HasForeignKey(x => x.CaseVersionId);
    }
}
