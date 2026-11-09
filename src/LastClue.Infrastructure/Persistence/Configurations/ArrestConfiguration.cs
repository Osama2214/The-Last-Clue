using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class ArrestConfiguration : IEntityTypeConfiguration<Arrest>
{
    public void Configure(EntityTypeBuilder<Arrest> builder)
    {
        builder.ToTable("Arrests", table =>
            table.HasCheckConstraint("CK_Arrests_Round", "[Round] BETWEEN 1 AND 3"));
        builder.HasKey(x => new { x.GameId, x.Round });
        builder.Property(x => x.TargetId).HasMaxLength(450);
        builder.HasIndex(x => new { x.GameId, x.TargetId }).IsUnique();
        builder.HasOne<Game>().WithMany(x => x.Arrests).HasForeignKey(x => x.GameId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.TargetId });
    }
}
