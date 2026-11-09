using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class GameCaseConfiguration : IEntityTypeConfiguration<GameCase>
{
    public void Configure(EntityTypeBuilder<GameCase> builder)
    {
        builder.ToTable("GameCases", table =>
        {
            table.HasCheckConstraint("CK_GameCases_SourceVariant",
                "([Source] = 'AI' AND [CaseVariantId] IS NULL) OR ([Source] = 'Fallback' AND [CaseVariantId] IS NOT NULL)");
            table.HasCheckConstraint("CK_GameCases_PayloadJson", "ISJSON([PayloadJson]) = 1");
        });
        builder.HasKey(x => x.GameId);
        builder.Property(x => x.Source).HasConversion<string>().HasColumnType("varchar(12)");
        builder.Property(x => x.SnapshotHash).HasColumnType("char(64)");
        builder.HasOne<Game>().WithOne().HasForeignKey<GameCase>(x => x.GameId);
        builder.HasOne<CaseVariant>().WithMany().HasForeignKey(x => x.CaseVariantId);
    }
}
