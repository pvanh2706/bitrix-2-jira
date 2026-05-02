using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedCredentials : Migration
    {
        private static readonly string[] _keys = new[]
        {
            "bitrix_user",
            "bitrix_password",
            "bitrix_attach_file_path",
            "bitrix_log_path",
            "jira_user",
            "jira_password",
            "email_sendgrid_api_key",
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemConfigs",
                columns: new[] { "ConfigKey", "ConfigValue", "Description" },
                values: new object[,]
                {
                    { "bitrix_user",             "connector",      "Username đăng nhập Bitrix24 (REST API)" },
                    { "bitrix_password",          "Batdau@2023",    "Password đăng nhập Bitrix24 (REST API)" },
                    { "bitrix_attach_file_path",  @"C:\BitrixClient\AttachFile\", "Thư mục lưu file đính kèm tải từ Bitrix" },
                    { "bitrix_log_path",          @"C:\BitrixClient\Logs\",       "Thư mục lưu log" },
                    { "jira_user",               "connector",      "Username đăng nhập Jira" },
                    { "jira_password",            "Batdau@2024",    "Password đăng nhập Jira" },
                    { "email_sendgrid_api_key",   "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", "SendGrid API Key dùng để gửi email" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var key in _keys)
            {
                migrationBuilder.Sql(
                    $"DELETE FROM SystemConfigs WHERE ConfigKey = '{key}'");
            }
        }
    }
}
