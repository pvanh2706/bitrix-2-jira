using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTableBitrixJiraInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitrixJiraInfoes",
                columns: table => new
                {
                    BitrixDealID = table.Column<int>(name: "Bitrix_DealID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BitrixDealLink = table.Column<string>(name: "Bitrix_DealLink", type: "TEXT", nullable: false),
                    BitrixDateSearch = table.Column<string>(name: "Bitrix_DateSearch", type: "TEXT", nullable: false),
                    IsSendDataToJira = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSendEmail = table.Column<int>(type: "INTEGER", nullable: false),
                    JiraLink = table.Column<string>(name: "Jira_Link", type: "TEXT", nullable: false),
                    HaveError = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorInfo = table.Column<string>(type: "TEXT", nullable: false),
                    DateTimeCreated = table.Column<string>(type: "TEXT", nullable: false),
                    NumberCheckError = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitrixJiraInfoes", x => x.BitrixDealID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitrixJiraInfoes");
        }
    }
}
