using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions", table =>
        {
            table.HasCheckConstraint("CK_Questions_Round", "[Round] BETWEEN 1 AND 3");
            table.HasCheckConstraint("CK_Questions_Slot", "[Slot] IN (1, 2)");
        });
        builder.Property(x => x.DetectiveId).HasMaxLength(450);
        builder.Property(x => x.TargetId).HasMaxLength(450);
        builder.Property(x => x.Text).HasMaxLength(300);
        builder.Property(x => x.AnswerText).HasMaxLength(1000);
        builder.HasIndex(x => new { x.GameId, x.Round, x.DetectiveId, x.Slot }).IsUnique();
        builder.HasOne<Game>().WithMany(x => x.Questions).HasForeignKey(x => x.GameId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.DetectiveId });
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.TargetId });
    }
}
