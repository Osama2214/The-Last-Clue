using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", table =>
        {
            table.HasCheckConstraint("CK_OutboxMessages_Target",
                "([RoomId] IS NOT NULL AND [GameId] IS NULL) OR ([RoomId] IS NULL AND [GameId] IS NOT NULL)");
            table.HasCheckConstraint("CK_OutboxMessages_PayloadJson", "ISJSON([PayloadJson]) = 1");
        });
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.Audience).HasColumnType("varchar(16)");
        builder.Property(x => x.EventType).HasColumnType("varchar(40)");
        builder.HasIndex(x => new { x.DispatchedAt, x.CreatedAt });
        builder.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomId);
        builder.HasOne<Game>().WithMany().HasForeignKey(x => x.GameId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.UserId });
    }
}
