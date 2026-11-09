using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class ClueRecipientConfiguration : IEntityTypeConfiguration<ClueRecipient>
{
    public void Configure(EntityTypeBuilder<ClueRecipient> builder)
    {
        builder.ToTable("ClueRecipients");
        builder.HasKey(x => new { x.GameId, x.ClueId, x.UserId }).IsClustered(false);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.HasOne<GameClue>().WithMany().HasForeignKey(x => new { x.GameId, x.ClueId })
            .HasPrincipalKey(x => new { x.GameId, x.Id });
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.UserId });
    }
}
