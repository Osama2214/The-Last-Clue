using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class PlayerAchievementConfiguration : IEntityTypeConfiguration<PlayerAchievement>
{
    public void Configure(EntityTypeBuilder<PlayerAchievement> builder)
    {
        builder.ToTable("PlayerAchievements");
        builder.HasKey(x => new { x.UserId, x.AchievementCode }).IsClustered(false);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.AchievementCode).HasColumnType("varchar(40)");
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId);
        builder.HasOne<AchievementDefinition>().WithMany().HasForeignKey(x => x.DefinitionId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.UserId });
    }
}
