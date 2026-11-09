using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class RankDefinitionConfiguration : IEntityTypeConfiguration<RankDefinition>
{
    public void Configure(EntityTypeBuilder<RankDefinition> builder)
    {
        builder.ToTable("RankDefinitions");
        builder.Property(x => x.Code).HasColumnType("varchar(40)");
        builder.Property(x => x.Name).HasMaxLength(80);
        builder.HasIndex(x => new { x.SettingsVersionId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.SettingsVersionId, x.MinPoints }).IsUnique();
        builder.HasOne<SettingsVersion>().WithMany().HasForeignKey(x => x.SettingsVersionId);
    }
}
