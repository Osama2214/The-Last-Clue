using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class GameClueConfiguration : IEntityTypeConfiguration<GameClue>
{
    public void Configure(EntityTypeBuilder<GameClue> builder)
    {
        builder.ToTable("GameClues", table =>
        {
            table.HasCheckConstraint("CK_GameClues_Round", "[Round] BETWEEN 1 AND 3");
            table.HasCheckConstraint("CK_GameClues_Audience", "[Audience] IN ('Public', 'Detectives', 'Character')");
        });
        builder.Property(x => x.ClueKey).HasColumnType("varchar(60)");
        builder.Property(x => x.Audience).HasConversion<string>().HasColumnType("varchar(12)");
        builder.Property(x => x.CharacterKey).HasColumnType("varchar(40)");
        builder.Property(x => x.Body).HasMaxLength(2000);
        builder.HasIndex(x => new { x.GameId, x.ClueKey }).IsUnique();
        builder.HasAlternateKey(x => new { x.GameId, x.Id });
        builder.HasOne<Game>().WithMany(x => x.Clues).HasForeignKey(x => x.GameId);
    }
}
