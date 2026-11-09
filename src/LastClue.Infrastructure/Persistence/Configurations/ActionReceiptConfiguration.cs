using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class ActionReceiptConfiguration : IEntityTypeConfiguration<ActionReceipt>
{
    public void Configure(EntityTypeBuilder<ActionReceipt> builder)
    {
        builder.ToTable("ActionReceipts", table =>
            table.HasCheckConstraint("CK_ActionReceipts_ResponseJson", "ISJSON([ResponseJson]) = 1"));
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.PayloadHash).HasColumnType("char(64)");
        builder.HasIndex(x => new { x.UserId, x.RequestId }).IsUnique();
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId);
        builder.HasOne<Game>().WithMany().HasForeignKey(x => x.GameId);
    }
}
