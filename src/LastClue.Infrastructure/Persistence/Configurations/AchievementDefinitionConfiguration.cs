using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class AchievementDefinitionConfiguration : IEntityTypeConfiguration<AchievementDefinition>
{
    public void Configure(EntityTypeBuilder<AchievementDefinition> builder)
    {
        builder.ToTable("AchievementDefinitions", table =>
            table.HasCheckConstraint("CK_AchievementDefinitions_CriteriaJson", "ISJSON([CriteriaJson]) = 1"));
        builder.Property(x => x.Code).HasColumnType("varchar(40)");
        builder.Property(x => x.Name).HasMaxLength(80);
        builder.HasIndex(x => new { x.SettingsVersionId, x.Code }).IsUnique();
        builder.HasOne<SettingsVersion>().WithMany().HasForeignKey(x => x.SettingsVersionId);
    }
}
