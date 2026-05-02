using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedConfigData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConfigData",
                columns: new[] { "KeyConfig", "ValueConfig", "Description" },
                values: new object[,]
                {
                    { "QuetLaiSau",      "5",  "Chu kỳ quét deal định kỳ (phút)" },
                    { "GuiLaiEmailSau",  "50", "Gửi lại email lỗi sau bao nhiêu giờ" },
                    { "SoNgayQuet",      "1",  "Số ngày quét ngược từ hiện tại" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ConfigData WHERE KeyConfig IN ('QuetLaiSau','GuiLaiEmailSau','SoNgayQuet')");
        }
    }
}
