using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BitrixJiraConnector
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Install-Package Microsoft.EntityFrameworkCore.Sqlite -Version 7.0.0
			// Install-Package Microsoft.EntityFrameworkCore.Tools -Version 7.0.0
			// Install-Package Microsoft.EntityFrameworkCore.Design -Version 7.0.0
			// NuGet\Install-Package Atlassian.SDK -Version 13.0.0
			// NuGet\Install-Package Newtonsoft.Json -Version 13.0.3


			//Add-Migration InitialCreate AddFieldTableBitrixJiraInfo
			//Update-Database


			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			//ApplicationConfiguration.Initialize();
			//Application.Run(new FormMain());

			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormMain());
		}
	}
}