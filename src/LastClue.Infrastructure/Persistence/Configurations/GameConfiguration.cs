using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games", table =>
        {
            table.HasCheckConstraint("CK_Games_Status", "[Status] IN ('Starting', 'Active', 'Completed', 'Cancelled')");
            table.HasCheckConstraint("CK_Games_Phase", "[Phase] IN ('Briefing', 'Statements', 'Questioning', 'ClueRelease', 'Accusation')");
            table.HasCheckConstraint("CK_Games_Outcome", "[Outcome] IN ('DetectivesWin', 'CriminalWin')");
            table.HasCheckConstraint("CK_Games_SettingsJson", "ISJSON([SettingsJson]) = 1");
            table.HasCheckConstraint("CK_Games_RunningStatus",
                "([IsRunning] = 1 AND [Status] IN ('Starting', 'Active')) OR ([IsRunning] = 0 AND [Status] IN ('Completed', 'Cancelled'))");
        });
        builder.Property(x => x.Status).HasConversion<string>().HasColumnType("varchar(16)");
        builder.Property(x => x.Phase).HasConversion<string>().HasColumnType("varchar(20)");
        builder.Property(x => x.Outcome).HasConversion<string>().HasColumnType("varchar(24)");
        builder.Property(x => x.CancellationReason).HasMaxLength(200);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.RoomId, x.Number }).IsUnique();
        builder.HasIndex(x => x.RoomId).IsUnique().HasFilter("[IsRunning] = 1");
        builder.HasOne<Room>().WithMany(room => room.Games).HasForeignKey(x => x.RoomId);
        builder.HasOne<SettingsVersion>().WithMany().HasForeignKey(x => x.SettingsVersionId);
    }
}
