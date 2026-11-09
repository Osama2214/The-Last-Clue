using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class ScoreEntryConfiguration : IEntityTypeConfiguration<ScoreEntry>
{
    public void Configure(EntityTypeBuilder<ScoreEntry> builder)
    {
        builder.ToTable("ScoreEntries");
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.EventKey).HasColumnType("varchar(80)");
        builder.HasIndex(x => new { x.GameId, x.UserId, x.EventKey }).IsUnique();
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.UserId });
    }
}
