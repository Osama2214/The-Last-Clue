using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class AccusationVoteConfiguration : IEntityTypeConfiguration<AccusationVote>
{
    public void Configure(EntityTypeBuilder<AccusationVote> builder)
    {
        builder.ToTable("AccusationVotes", table =>
            table.HasCheckConstraint("CK_AccusationVotes_Round", "[Round] BETWEEN 1 AND 3"));
        builder.HasKey(x => new { x.GameId, x.Round, x.DetectiveId }).IsClustered(false);
        builder.Property(x => x.DetectiveId).HasMaxLength(450);
        builder.Property(x => x.TargetId).HasMaxLength(450);
        builder.HasOne<Game>().WithMany(x => x.Votes).HasForeignKey(x => x.GameId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.DetectiveId });
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.TargetId });
    }
}
