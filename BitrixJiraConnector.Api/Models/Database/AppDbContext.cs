using Microsoft.EntityFrameworkCore;

namespace BitrixJiraConnector.Api.Models.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BitrixJiraInfo> BitrixJiraInfoes { get; set; }
    public DbSet<ConfigData> ConfigData { get; set; }
    public DbSet<ExceptionLog> ExceptionLog { get; set; }
}
