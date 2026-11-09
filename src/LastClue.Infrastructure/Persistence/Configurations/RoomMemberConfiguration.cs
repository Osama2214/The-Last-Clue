using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> builder)
    {
        builder.ToTable("RoomMembers");
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.HasIndex(x => new { x.RoomId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("[LeftAt] IS NULL");

        builder.HasOne<Room>().WithMany(room => room.Members).HasForeignKey(x => x.RoomId);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId);
    }
}
