using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class GenerationJobConfiguration : IEntityTypeConfiguration<GenerationJob>
{
    public void Configure(EntityTypeBuilder<GenerationJob> builder)
    {
        builder.ToTable("GenerationJobs", table =>
            table.HasCheckConstraint("CK_GenerationJobs_Status", "[Status] IN ('Pending', 'Running', 'Succeeded', 'Failed')"));
        builder.Property(x => x.Status).HasConversion<string>().HasColumnType("varchar(16)");
        builder.Property(x => x.LastErrorCode).HasColumnType("varchar(40)");
        builder.HasIndex(x => x.GameId).IsUnique();
        builder.HasIndex(x => x.RequestId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.LeaseUntil });
        builder.HasOne<Game>().WithOne().HasForeignKey<GenerationJob>(x => x.GameId);
    }
}
