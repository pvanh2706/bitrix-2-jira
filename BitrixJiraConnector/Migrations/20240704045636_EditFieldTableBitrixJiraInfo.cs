using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Migrations
{
    /// <inheritdoc />
    public partial class EditFieldTableBitrixJiraInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeSendMailFist",
                table: "BitrixJiraInfoes",
                newName: "DateTimeSendMailFirst");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeSendMailFirst",
                table: "BitrixJiraInfoes",
                newName: "DateTimeSendMailFist");
        }
    }
}
