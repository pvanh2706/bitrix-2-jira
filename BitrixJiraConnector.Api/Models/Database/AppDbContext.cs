using Microsoft.EntityFrameworkCore;

namespace BitrixJiraConnector.Api.Models.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<BitrixJiraInfo> BitrixJiraInfoes { get; set; }
    public DbSet<ConfigData> ConfigData { get; set; }
    public DbSet<ExceptionLog> ExceptionLog { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<BitrixFieldMapping> BitrixFieldMappings { get; set; }
    public DbSet<DealTypeConfig> DealTypeConfigs { get; set; }
    public DbSet<DealTypeRequiredField> DealTypeRequiredFields { get; set; }
    public DbSet<UserEmailMapping> UserEmailMappings { get; set; }
    public DbSet<PipelineMapping> PipelineMappings { get; set; }
}
