using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldTableBitrixJiraInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DateTimeSendMailFist",
                table: "BitrixJiraInfoes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DateTimeSendMailSecond",
                table: "BitrixJiraInfoes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DateTimeSendMailThird",
                table: "BitrixJiraInfoes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastChangeData",
                table: "BitrixJiraInfoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTimeSendMailFist",
                table: "BitrixJiraInfoes");

            migrationBuilder.DropColumn(
                name: "DateTimeSendMailSecond",
                table: "BitrixJiraInfoes");

            migrationBuilder.DropColumn(
                name: "DateTimeSendMailThird",
                table: "BitrixJiraInfoes");

            migrationBuilder.DropColumn(
                name: "LastChangeData",
                table: "BitrixJiraInfoes");
        }
    }
}
