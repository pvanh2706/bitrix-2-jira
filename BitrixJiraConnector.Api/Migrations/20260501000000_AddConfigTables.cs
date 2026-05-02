using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitrixJiraConnector.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----------------------------------------------------------------
            // SystemConfig
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfigKey = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigValue = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemConfigs",
                columns: new[] { "ConfigKey", "ConfigValue", "Description" },
                values: new object[,]
                {
                    { "deal_scan_cutoff_minutes",    "3",                                        "Số phút lùi về quá khứ khi lọc deal theo DATE_MODIFY" },
                    { "deal_recent_update_minutes",  "3",                                        "Nếu deal vừa được sửa trong vòng N phút thì bỏ qua (chờ ổn định)" },
                    { "deal_stage_process",          "S,F",                                      "Danh sách STAGE_SEMANTIC_ID cần xử lý, phân cách bằng dấu phẩy" },
                    { "email_subject_success",       "Tạo issues thành công từ Bitrix",          "Tiêu đề email khi tạo Jira issue thành công" },
                    { "email_subject_error",         "Tạo issues từ Bitrix bị lỗi",              "Tiêu đề email khi tạo Jira issue thất bại" },
                    { "email_domain_strip",          "@ezcloud.vn",                              "Phần domain bị cắt bỏ khi chuyển email thành Jira username mặc định" },
                    { "email_retry_hour",            "9",                                        "Giờ trong ngày để gửi lại email lỗi (0-23)" },
                    { "max_attachment_bytes",        "10485760",                                 "Kích thước tối đa file đính kèm tính bằng byte (mặc định 10 MB)" },
                    { "default_search_days",         "30",                                       "Số ngày lùi mặc định khi tìm kiếm deal trên giao diện" },
                    // Bitrix URLs
                    { "bitrix_api_url",              "https://worktest.ezcloudhotel.com/rest/861/mgna8op38m1k5vcx", "URL REST API của Bitrix24" },
                    { "bitrix_base_url",             "https://worktest.ezcloudhotel.com",        "Base URL của Bitrix24" },
                    { "bitrix_deal_detail_url",      "https://work.ezcloudhotel.com/crm/deal/details", "URL xem chi tiết deal trên Bitrix24" },
                    // Jira URL
                    { "jira_url",                    "https://jira.ezcloudhotel.com",            "URL của Jira" },
                    // Email addresses
                    { "email_from_address",          "bitrix-jira.noreply@gmail.com",            "Địa chỉ email gửi đi" },
                    { "email_from_name",             "Bitrix Jira Connector",                    "Tên hiển thị của địa chỉ email gửi đi" },
                    { "email_fallback_to",           "anh.pham@ezcloud.vn",                      "Email nhận thông báo dự phòng khi không xác định được người nhận" },
                    { "email_renewal_manager",       "sale3.hn@ezcloud.vn",                      "Email Renewal Manager nhận thông báo" },
                    { "email_sales_manager",         "huyenlt@ezcloud.vn",                       "Email Sales Manager nhận thông báo" },
                    { "email_admin_bitrix",          "huyenlt@ezcloud.vn",                       "Email Admin Bitrix nhận thông báo lỗi" },
                    { "email_admin_jira",            "dung.nguyen@ezcloud.vn",                   "Email Admin Jira nhận thông báo lỗi" },
                    // Scanning
                    { "scanning_interval_minutes",   "5",                                        "Chu kỳ quét deal định kỳ (phút)" },
                    { "scanning_lookback_days",      "1",                                        "Số ngày lùi khi quét deal lần đầu hoặc sau khi restart" },
                });

            // ----------------------------------------------------------------
            // BitrixFieldMapping
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "BitrixFieldMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FieldKey = table.Column<string>(type: "TEXT", nullable: false),
                    FieldLabel = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitrixFieldMappings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BitrixFieldMappings",
                columns: new[] { "FieldKey", "FieldLabel" },
                values: new object[,]
                {
                    { "UF_CRM_1713881390",       "Loại Deal" },
                    { "UF_CRM_5D98084D1476E",    "Nhu cầu sản phẩm" },
                    { "UF_CRM_5B3F32B1118E0",    "Tên khách sạn" },
                    { "ASSIGNED_BY_ID",           "Responsible" },
                    { "CATEGORY_ID",              "Pipeline" },
                    { "UF_CRM_1708697569",        "Thị trường" },
                    { "UF_CRM_1616123841",        "Loại hình khách sạn [Lead]" },
                    { "SOURCE_ID",                "Source" },
                    { "COMPANY_ID",               "Company Id" },
                    { "url",                      "Link CRM" },
                    { "ID",                       "Deal Id" },
                    { "UF_CRM_5D984E2CA62C9",    "Địa chỉ Khách sạn: Số nhà, Đường" },
                    { "UF_CRM_5D984E30EEE34",    "Địa chỉ Khách sạn: Phường/ Xã" },
                    { "UF_CRM_5B3F32B1068C7",    "Số phòng" },
                    { "UF_CRM_6054492BF24D8",     "Số giường" },
                    { "CONTACT_ID",               "Contact" },
                    { "UF_CRM_1708697773",        "Chính sách giá" },
                    { "UF_CRM_1708698013",        "Máy chủ chạy phần mềm" },
                    { "UF_CRM_1715996452",        "Dùng thử hay dùng thật" },
                    { "UF_CRM_1613788692724",     "Hợp đồng & Phụ lục" },
                    { "UF_CRM_1616123465",        "Trạng thái thanh toán lần 1" },
                    { "UF_CRM_1614068621890",     "Nội dung chuyển khoản (Thanh toán lần 1)" },
                    { "UF_CRM_1616130516",        "Phương thức triển khai" },
                    { "UF_CRM_1616130611",        "Thời điểm triển khai" },
                    { "UF_CRM_1616130767",        "Tài khoản dùng thử PMS (nếu có)" },
                    { "UF_CRM_1616130823",        "Nếu triển khai BE, có tích hợp ezFolio/Cloud không? (Có/ Không)" },
                    { "UF_CRM_1616130900",        "Thông tin triển khai web" },
                    { "UF_CRM_1531008138",        "Ghi chú" },
                    { "UF_CRM_1613727861184",     "Mã khách sạn" },
                    { "UF_CRM_1708698698",        "Yêu cầu thêm" },
                    { "UF_CRM_1570243307",        "Lý do lost" },
                    { "UF_CRM_1570243354",        "Ghi chú cho lý do lost" },
                    { "UF_CRM_1708699318",        "Loại yêu cầu huỷ" },
                    { "UF_CRM_1715997025",        "Thời điểm ngừng/ hủy" },
                    { "TITLE",                    "Tên Deal" },
                    { "UF_CRM_1600922068",        "Ngày bắt đầu tính GHBT" },
                    { "UF_CRM_1600922103",        "Ngày kết thúc hạn GHBT" },
                    { "UF_CRM_1715997550",        "Tình huống cần cấp key" },
                    { "THONTINLIENHE_TENKHACH",         "Tên khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
                    { "THONTINLIENHE_CHUCVUKHACHHANG",  "Chức vụ khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
                    { "THONTINLIENHE_SDT",              "Số điện thoại khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
                    { "THONTINLIENHE_EMAIL",            "Email khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
                });

            // ----------------------------------------------------------------
            // DealTypeConfig
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "DealTypeConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    DealTypeName = table.Column<string>(type: "TEXT", nullable: false),
                    JiraProjectKey = table.Column<string>(type: "TEXT", nullable: false),
                    JiraIssueTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    ShouldCreateIssue = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealTypeConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DealTypeConfigs",
                columns: new[] { "DealTypeId", "DealTypeName", "JiraProjectKey", "JiraIssueTypeId", "ShouldCreateIssue" },
                values: new object[,]
                {
                    { "3095", "Triển Khai Mới",        "TRIENKHAI", "10000", 1 },
                    { "3096", "Triển Khai Bổ Sung",    "TRIENKHAI", "10000", 1 },
                    { "3097", "Ngừng/ Hủy Dịch Vụ",   "ES",        "11600", 1 },
                    { "3098", "Chuyển Đổi Dịch Vụ",   "TRIENKHAI", "10000", 1 },
                    { "3099", "Cấp Key",               "ES",        "10301", 1 },
                    { "3100", "Hỗ Trợ Khác",           "ES",        "10301", 1 },
                });

            // ----------------------------------------------------------------
            // DealTypeRequiredField
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "DealTypeRequiredFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    FieldKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealTypeRequiredFields", x => x.Id);
                });

            // TrienKhaiMoi (3095)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3095", "UF_CRM_1713881390" },
                    { "3095", "UF_CRM_5D98084D1476E" },
                    { "3095", "UF_CRM_5B3F32B1118E0" },
                    { "3095", "ASSIGNED_BY_ID" },
                    { "3095", "CATEGORY_ID" },
                    { "3095", "UF_CRM_1708697569" },
                    { "3095", "SOURCE_ID" },
                    { "3095", "COMPANY_ID" },
                    { "3095", "ID" },
                    { "3095", "UF_CRM_5D984E2CA62C9" },
                    { "3095", "UF_CRM_5D984E30EEE34" },
                    { "3095", "UF_CRM_5B3F32B1068C7" },
                    { "3095", "UF_CRM_6054492BF24D8" },
                    { "3095", "CONTACT_ID" },
                    { "3095", "UF_CRM_1708697773" },
                    { "3095", "UF_CRM_1708698013" },
                    { "3095", "UF_CRM_1613788692724" },
                    { "3095", "UF_CRM_1616123465" },
                    { "3095", "UF_CRM_1614068621890" },
                    { "3095", "UF_CRM_1616130516" },
                    { "3095", "UF_CRM_1616130611" },
                });

            // TrienKhaiBoSung (3096)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3096", "UF_CRM_1713881390" },
                    { "3096", "UF_CRM_5D98084D1476E" },
                    { "3096", "UF_CRM_5B3F32B1118E0" },
                    { "3096", "ASSIGNED_BY_ID" },
                    { "3096", "UF_CRM_1708697569" },
                    { "3096", "COMPANY_ID" },
                    { "3096", "ID" },
                    { "3096", "UF_CRM_5D984E2CA62C9" },
                    { "3096", "UF_CRM_5D984E30EEE34" },
                    { "3096", "UF_CRM_5B3F32B1068C7" },
                    { "3096", "UF_CRM_6054492BF24D8" },
                    { "3096", "CONTACT_ID" },
                    { "3096", "UF_CRM_1708697773" },
                    { "3096", "UF_CRM_1613788692724" },
                    { "3096", "UF_CRM_1616123465" },
                    { "3096", "UF_CRM_1614068621890" },
                    { "3096", "UF_CRM_1616130516" },
                    { "3096", "UF_CRM_1616130611" },
                });

            // NgungHuyDichVu (3097)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3097", "UF_CRM_1713881390" },
                    { "3097", "UF_CRM_5B3F32B1118E0" },
                    { "3097", "ASSIGNED_BY_ID" },
                    { "3097", "CATEGORY_ID" },
                    { "3097", "COMPANY_ID" },
                    { "3097", "ID" },
                    { "3097", "UF_CRM_5D984E2CA62C9" },
                    { "3097", "UF_CRM_5D984E30EEE34" },
                    { "3097", "CONTACT_ID" },
                    { "3097", "UF_CRM_1570243307" },
                    { "3097", "UF_CRM_1570243354" },
                    { "3097", "UF_CRM_1708699318" },
                    { "3097", "UF_CRM_1715997025" },
                });

            // ChuyenDoiDichVu (3098)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3098", "UF_CRM_1713881390" },
                    { "3098", "UF_CRM_5D98084D1476E" },
                    { "3098", "UF_CRM_5B3F32B1118E0" },
                    { "3098", "ASSIGNED_BY_ID" },
                    { "3098", "CATEGORY_ID" },
                    { "3098", "UF_CRM_1708697569" },
                    { "3098", "COMPANY_ID" },
                    { "3098", "ID" },
                    { "3098", "UF_CRM_5D984E2CA62C9" },
                    { "3098", "UF_CRM_5D984E30EEE34" },
                    { "3098", "UF_CRM_5B3F32B1068C7" },
                    { "3098", "UF_CRM_6054492BF24D8" },
                    { "3098", "CONTACT_ID" },
                    { "3098", "UF_CRM_1613788692724" },
                    { "3098", "UF_CRM_1616123465" },
                    { "3098", "UF_CRM_1616130516" },
                    { "3098", "UF_CRM_1614068621890" },
                });

            // CapKey (3099)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3099", "UF_CRM_1713881390" },
                    { "3099", "ASSIGNED_BY_ID" },
                    { "3099", "TITLE" },
                    { "3099", "UF_CRM_5B3F32B1118E0" },
                    { "3099", "UF_CRM_1600922068" },
                    { "3099", "UF_CRM_1600922103" },
                    { "3099", "UF_CRM_1715997550" },
                    { "3099", "CONTACT_ID" },
                    { "3099", "UF_CRM_1613788692724" },
                });

            // HoTroKhac (3100)
            migrationBuilder.InsertData(
                table: "DealTypeRequiredFields",
                columns: new[] { "DealTypeId", "FieldKey" },
                values: new object[,]
                {
                    { "3100", "UF_CRM_1713881390" },
                    { "3100", "ASSIGNED_BY_ID" },
                    { "3100", "TITLE" },
                    { "3100", "UF_CRM_1531008138" },
                    { "3100", "CONTACT_ID" },
                    { "3100", "UF_CRM_1613788692724" },
                });

            // ----------------------------------------------------------------
            // UserEmailMapping
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "UserEmailMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    JiraUsername = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmailMappings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserEmailMappings",
                columns: new[] { "Email", "JiraUsername" },
                values: new object[,]
                {
                    { "dung.nguyen@ezcloud.vn",  "dungnn" },
                    { "duong.nguyen@ezcloud.vn", "duongnh" },
                });

            // ----------------------------------------------------------------
            // PipelineMapping
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "PipelineMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    PipelineName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineMappings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PipelineMappings",
                columns: new[] { "CategoryId", "PipelineName" },
                values: new object[,]
                {
                    { "0", "Sales" },
                    { "3", "Renewal" },
                    { "4", "Cross-Sales" },
                    { "6", "Project Sale" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SystemConfigs");
            migrationBuilder.DropTable(name: "BitrixFieldMappings");
            migrationBuilder.DropTable(name: "DealTypeConfigs");
            migrationBuilder.DropTable(name: "DealTypeRequiredFields");
            migrationBuilder.DropTable(name: "UserEmailMappings");
            migrationBuilder.DropTable(name: "PipelineMappings");
        }
    }
}
