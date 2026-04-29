namespace BitrixJiraConnector.Api.Configurations;

public static class ConfigJiraBitrix
{
    // Issue type IDs (from Jira Administration > Issues > Issue Types)
    public const string IdIssueType_Epic = "10000";
    public const string IdIssueType_HDNgungHuyDichVu = "11600";
    public const string IdIssueType_ServiceRequest = "10301";

    // Project keys (from Jira Administration > Projects)
    public const string IssueProject_TrienKhai = "TRIENKHAI";
    public const string IssueProject_ES = "ES";

    // Deal type IDs from Bitrix
    public const string LoaiDeal_TrienKhaiMoi = "3095";
    public const string LoaiDeal_TrienKhaiBoSung = "3096";
    public const string LoaiDeal_NgungHuyDichVu = "3097";
    public const string LoaiDeal_ChuyenDoiDichVu = "3098";
    public const string LoaiDeal_CapKey = "3099";
    public const string LoaiDeal_HoTroKhac = "3100";

    // Email subjects
    public const string MailInfo_Subject_Create_Iss_Success = "Tạo issues thành công từ Bitrix";
    public const string MailInfo_Subject_Create_Iss_Error = "Tạo issues từ Bitrix bị lỗi";

    // Required fields per deal type
    public static readonly IReadOnlyList<string> FieldRequire_TrienKhaiMoi = new[]
    {
        "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID",
        "CATEGORY_ID", "UF_CRM_1708697569", "SOURCE_ID", "COMPANY_ID", "ID",
        "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34", "UF_CRM_5B3F32B1068C7",
        "UF_CRM_6054492BF24D8", "CONTACT_ID", "UF_CRM_1708697773", "UF_CRM_1708698013",
        "UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1614068621890",
        "UF_CRM_1616130516", "UF_CRM_1616130611"
    };

    public static readonly IReadOnlyList<string> FieldRequire_TrienKhaiBoSung = new[]
    {
        "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID",
        "UF_CRM_1708697569", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34",
        "UF_CRM_5B3F32B1068C7", "UF_CRM_6054492BF24D8", "CONTACT_ID", "UF_CRM_1708697773",
        "UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1614068621890",
        "UF_CRM_1616130516", "UF_CRM_1616130611"
    };

    public static readonly IReadOnlyList<string> FieldRequire_NgungHuyDichVu = new[]
    {
        "UF_CRM_1713881390", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID", "CATEGORY_ID",
        "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34", "CONTACT_ID",
        "UF_CRM_1570243307", "UF_CRM_1570243354", "UF_CRM_1708699318", "UF_CRM_1715997025"
    };

    public static readonly IReadOnlyList<string> FieldRequire_ChuyenDoiDichVu = new[]
    {
        "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID",
        "CATEGORY_ID", "UF_CRM_1708697569", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9",
        "UF_CRM_5D984E30EEE34", "UF_CRM_5B3F32B1068C7", "UF_CRM_6054492BF24D8", "CONTACT_ID",
        "UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1616130516", "UF_CRM_1614068621890"
    };

    public static readonly IReadOnlyList<string> FieldRequire_CapKey = new[]
    {
        "UF_CRM_1713881390", "ASSIGNED_BY_ID", "TITLE", "UF_CRM_5B3F32B1118E0",
        "UF_CRM_1600922068", "UF_CRM_1600922103", "UF_CRM_1715997550", "CONTACT_ID",
        "UF_CRM_1613788692724"
    };

    public static readonly IReadOnlyList<string> FieldRequire_HoTroKhac = new[]
    {
        "UF_CRM_1713881390", "ASSIGNED_BY_ID", "TITLE", "UF_CRM_1531008138",
        "CONTACT_ID", "UF_CRM_1613788692724"
    };

    // Maps Bitrix API field key → display name (for error messages)
    public static readonly IReadOnlyDictionary<string, string> KeyValueField_Bitrix = new Dictionary<string, string>
    {
        { "UF_CRM_1713881390", "Loại Deal" },
        { "UF_CRM_5D98084D1476E", "Nhu cầu sản phẩm" },
        { "UF_CRM_5B3F32B1118E0", "Tên khách sạn" },
        { "ASSIGNED_BY_ID", "Responsible" },
        { "CATEGORY_ID", "Pipeline" },
        { "UF_CRM_1708697569", "Thị trường" },
        { "UF_CRM_1616123841", "Loại hình khách sạn [Lead]" },
        { "SOURCE_ID", "Source" },
        { "COMPANY_ID", "Company Id" },
        { "url", "Link CRM" },
        { "ID", "Deal Id" },
        { "UF_CRM_5D984E2CA62C9", "Địa chỉ Khách sạn: Số nhà, Đường" },
        { "UF_CRM_5D984E30EEE34", "Địa chỉ Khách sạn: Phường/ Xã" },
        { "UF_CRM_5B3F32B1068C7", "Số phòng" },
        { "UF_CRM_6054492BF24D8", "Số giường" },
        { "CONTACT_ID", "Contact" },
        { "UF_CRM_1708697773", "Chính sách giá" },
        { "UF_CRM_1708698013", "Máy chủ chạy phần mềm" },
        { "UF_CRM_1715996452", "Dùng thử hay dùng thật" },
        { "UF_CRM_1613788692724", "Hợp đồng & Phụ lục" },
        { "UF_CRM_1616123465", "Trạng thái thanh toán lần 1" },
        { "UF_CRM_1614068621890", "Nội dung chuyển khoản (Thanh toán lần 1)" },
        { "UF_CRM_1616130516", "Phương thức triển khai" },
        { "UF_CRM_1616130611", "Thời điểm triển khai" },
        { "UF_CRM_1616130767", "Tài khoản dùng thử PMS (nếu có)" },
        { "UF_CRM_1616130823", "Nếu triển khai BE, có tích hợp ezFolio/Cloud không? (Có/ Không)" },
        { "UF_CRM_1616130900", "Thông tin triển khai web" },
        { "UF_CRM_1531008138", "Ghi chú" },
        { "UF_CRM_1613727861184", "Mã khách sạn" },
        { "UF_CRM_1708698698", "Yêu cầu thêm" },
        { "UF_CRM_1570243307", "Lý do lost" },
        { "UF_CRM_1570243354", "Ghi chú cho lý do lost" },
        { "UF_CRM_1708699318", "Loại yêu cầu huỷ" },
        { "UF_CRM_1715997025", "Thời điểm ngừng/ hủy" },
        { "TITLE", "Tên Deal" },
        { "UF_CRM_1600922068", "Ngày bắt đầu tính GHBT" },
        { "UF_CRM_1600922103", "Ngày kết thúc hạn GHBT" },
        { "UF_CRM_1715997550", "Tình huống cần cấp key" },
        { "THONTINLIENHE_TENKHACH", "Tên khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
        { "THONTINLIENHE_CHUCVUKHACHHANG", "Chức vụ khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
        { "THONTINLIENHE_SDT", "Số điện thoại khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
        { "THONTINLIENHE_EMAIL", "Email khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
    };
}
