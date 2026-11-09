using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class StatementConfiguration : IEntityTypeConfiguration<Statement>
{
    public void Configure(EntityTypeBuilder<Statement> builder)
    {
        builder.ToTable("Statements", table =>
        {
            table.HasCheckConstraint("CK_Statements_Round", "[Round] BETWEEN 1 AND 3");
            table.HasCheckConstraint("CK_Statements_Text", "LEN(TRIM([Text])) > 0");
        });
        builder.Property(x => x.AuthorId).HasMaxLength(450);
        builder.Property(x => x.Text).HasMaxLength(1000);
        builder.HasIndex(x => new { x.GameId, x.Round, x.AuthorId }).IsUnique();
        builder.HasOne<Game>().WithMany(x => x.Statements).HasForeignKey(x => x.GameId);
        builder.HasOne<GamePlayer>().WithMany().HasForeignKey(x => new { x.GameId, x.AuthorId });
    }
}
