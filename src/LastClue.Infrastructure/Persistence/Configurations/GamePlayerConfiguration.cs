using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.ToTable("GamePlayers", table =>
            table.HasCheckConstraint("CK_GamePlayers_Role", "[Role] IN ('Detective', 'Criminal', 'Innocent')"));
        builder.HasKey(x => new { x.GameId, x.UserId }).IsClustered(false);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.Role).HasConversion<string>().HasColumnType("varchar(12)");
        builder.Property(x => x.CharacterKey).HasColumnType("varchar(40)");
        builder.HasIndex(x => new { x.GameId, x.CharacterKey }).IsUnique().HasFilter("[CharacterKey] IS NOT NULL");

        builder.HasOne<Game>().WithMany(game => game.Players).HasForeignKey(x => x.GameId);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId);
    }
}
