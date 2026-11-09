using LastClue.Application.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GamePlayer> GamePlayers => Set<GamePlayer>();
    public DbSet<SettingsVersion> SettingsVersions => Set<SettingsVersion>();
    public DbSet<PlayerConnection> PlayerConnections => Set<PlayerConnection>();
    public DbSet<CaseTemplate> CaseTemplates => Set<CaseTemplate>();
    public DbSet<CaseVersion> CaseVersions => Set<CaseVersion>();
    public DbSet<CaseVariant> CaseVariants => Set<CaseVariant>();
    public DbSet<GameCase> GameCases => Set<GameCase>();
    public DbSet<GameClue> GameClues => Set<GameClue>();
    public DbSet<ClueRecipient> ClueRecipients => Set<ClueRecipient>();
    public DbSet<GenerationJob> GenerationJobs => Set<GenerationJob>();
    public DbSet<Statement> Statements => Set<Statement>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AccusationVote> AccusationVotes => Set<AccusationVote>();
    public DbSet<Arrest> Arrests => Set<Arrest>();
    public DbSet<PendingScoreEvent> PendingScoreEvents => Set<PendingScoreEvent>();
    public DbSet<ScoreEntry> ScoreEntrys => Set<ScoreEntry>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>().HavePrecision(3);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>(user =>
        {
            user.Property(x => x.DisplayName).HasMaxLength(80);
            user.Property(x => x.AvatarSeed).HasMaxLength(80);
        });
        builder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId }).IsClustered(false);
        builder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name }).IsClustered(false);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(entity => entity.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
