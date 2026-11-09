using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class AdminAuditConfiguration : IEntityTypeConfiguration<AdminAudit>
{
    public void Configure(EntityTypeBuilder<AdminAudit> builder)
    {
        builder.ToTable("AdminAudit", table =>
            table.HasCheckConstraint("CK_AdminAudit_SafeDetailsJson", "ISJSON([SafeDetailsJson]) = 1"));
        builder.Property(x => x.ActorId).HasMaxLength(450);
        builder.Property(x => x.Action).HasColumnType("varchar(60)");
        builder.Property(x => x.EntityType).HasColumnType("varchar(40)");
        builder.Property(x => x.EntityId).HasMaxLength(450);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ActorId);
    }
}
