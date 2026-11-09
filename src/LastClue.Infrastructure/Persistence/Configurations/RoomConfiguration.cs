using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms", table =>
        {
            table.HasCheckConstraint("CK_Rooms_Language", "[Language] IN ('en', 'ar')");
            table.HasCheckConstraint("CK_Rooms_Status", "[Status] IN ('Lobby', 'Starting', 'Active', 'Closed')");
        });
        builder.Property(x => x.Code).HasColumnType("varchar(12)");
        builder.Property(x => x.Language).HasColumnType("char(2)");
        builder.Property(x => x.Status).HasConversion<string>().HasColumnType("varchar(16)");
        builder.Property(x => x.HostUserId).HasMaxLength(450);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.HostUserId);
    }
}
