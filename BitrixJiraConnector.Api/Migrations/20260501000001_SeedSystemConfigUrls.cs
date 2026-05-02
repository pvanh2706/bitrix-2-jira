using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemConfigUrls : Migration
    {
        private static readonly string[] _keys = new[]
        {
            "bitrix_api_url",
            "bitrix_base_url",
            "bitrix_deal_detail_url",
            "jira_url",
            "email_from_address",
            "email_from_name",
            "email_fallback_to",
            "email_renewal_manager",
            "email_sales_manager",
            "email_admin_bitrix",
            "email_admin_jira",
            "scanning_interval_minutes",
            "scanning_lookback_days",
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemConfigs",
                columns: new[] { "ConfigKey", "ConfigValue", "Description" },
                values: new object[,]
                {
                    { "bitrix_api_url",            "https://worktest.ezcloudhotel.com/rest/861/mgna8op38m1k5vcx", "URL REST API của Bitrix24" },
                    { "bitrix_base_url",            "https://worktest.ezcloudhotel.com",        "Base URL của Bitrix24" },
                    { "bitrix_deal_detail_url",     "https://work.ezcloudhotel.com/crm/deal/details", "URL xem chi tiết deal trên Bitrix24" },
                    { "jira_url",                   "https://jira.ezcloudhotel.com",            "URL của Jira" },
                    { "email_from_address",         "bitrix-jira.noreply@gmail.com",            "Địa chỉ email gửi đi" },
                    { "email_from_name",            "Bitrix Jira Connector",                    "Tên hiển thị của địa chỉ email gửi đi" },
                    { "email_fallback_to",          "anh.pham@ezcloud.vn",                      "Email nhận thông báo dự phòng khi không xác định được người nhận" },
                    { "email_renewal_manager",      "sale3.hn@ezcloud.vn",                      "Email Renewal Manager nhận thông báo" },
                    { "email_sales_manager",        "huyenlt@ezcloud.vn",                       "Email Sales Manager nhận thông báo" },
                    { "email_admin_bitrix",         "huyenlt@ezcloud.vn",                       "Email Admin Bitrix nhận thông báo lỗi" },
                    { "email_admin_jira",           "dung.nguyen@ezcloud.vn",                   "Email Admin Jira nhận thông báo lỗi" },
                    { "scanning_interval_minutes",  "5",                                        "Chu kỳ quét deal định kỳ (phút)" },
                    { "scanning_lookback_days",     "1",                                        "Số ngày lùi khi quét deal lần đầu hoặc sau khi restart" },
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
