using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class SettingsVersionConfiguration : IEntityTypeConfiguration<SettingsVersion>
{
    public void Configure(EntityTypeBuilder<SettingsVersion> builder)
    {
        builder.ToTable("SettingsVersions", table =>
            table.HasCheckConstraint("CK_SettingsVersions_ValuesJson", "ISJSON([ValuesJson]) = 1"));
        builder.Property(x => x.ChangedBy).HasMaxLength(450);
        builder.HasIndex(x => x.VersionNo).IsUnique();
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ChangedBy);
    }
}
