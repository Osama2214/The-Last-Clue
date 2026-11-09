using LastClue.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastClue.Infrastructure.Persistence.Configurations;

internal sealed class CaseTemplateConfiguration : IEntityTypeConfiguration<CaseTemplate>
{
    public void Configure(EntityTypeBuilder<CaseTemplate> builder)
    {
        builder.ToTable("CaseTemplates");
        builder.Property(x => x.CaseKey).HasColumnType("varchar(60)");
        builder.Property(x => x.Title).HasMaxLength(120);
        builder.HasIndex(x => x.CaseKey).IsUnique();
    }
}
