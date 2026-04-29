using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitrixJiraConnector.Models
{
	public class AppDbContext : DbContext
	{
		public DbSet<BitrixJiraInfo> BitrixJiraInfoes { get; set; }
		public DbSet<ConfigData> ConfigData { get; set; }
		public DbSet<ExceptionLog> ExceptionLog { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=ConnectBitrixAndJira.db");
		}
	}
}
