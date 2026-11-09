using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConnectionConfiguration : IEntityTypeConfiguration<PlayerConnection>
{
    public void Configure(EntityTypeBuilder<PlayerConnection> builder)
    {
        builder.ToTable("PlayerConnections");
        builder.HasKey(x => x.ConnectionId);
        builder.Property(x => x.ConnectionId).HasMaxLength(128);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.UserId });
    }
}
