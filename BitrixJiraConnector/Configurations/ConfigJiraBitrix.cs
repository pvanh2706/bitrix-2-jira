using BitrixJiraConnector.Models;
using System.Configuration;

namespace BitrixJiraConnector.Configurations
{
	class ConfigJiraBitrix
	{
		public static string bitrixUrlDeal = "https://work.ezcloudhotel.com/crm/deal/details";
		public static string bitrixAPI = "https://worktest.ezcloudhotel.com/rest/861/mgna8op38m1k5vcx";
		public static string bitrixUrl = "https://worktest.ezcloudhotel.com";
		public static string bitrixURI = "https://worktest.ezcloudhotel.com/auth/index.php?login=yes";
		public static string bitrixUser = "connector";
		public static string bitrixPass = "Batdau@2023";
		public static string bitrixPHPSESSID = "o2c5d8f7bir6c1qpncon4327pg";
		public static string bitrixBITRIX_SM_SALE_UID = "0";


		public static string jiraUrl = "https://jira.ezcloudhotel.com";
		public static string jiraUser = "connector";
		public static string jiraPass = "Batdau@2024";

		public static List<ConfigData> configDatas = new List<ConfigData>();
		//public static string folderBitrixClient = ConfigurationManager.AppSettings["folderBitrixClient"];
		public static string folderAttachFile = "C:\\BitrixClient\\AttachFile\\";
		public static string folderLog = "C:\\BitrixClient\\Logs\\";
		//public static string folderLog = ConfigurationManager.AppSettings["folderLog"];

		public static List<int> dealIDProcessed = new List<int>();
		// public static List<int> dealCheckResendEmail = new List<int>();

		public const string IssueProject = "TRIENKHAI";
		public const int HotelStatusCheck = 3;
		public const string IssueType = "10000";
		public const string IssuePriority = "10102";
		// Cấu hình kết nối DB
		public const string connectionString = "Data Source=ConnectBitrixAndJira.db";
		// public const string beginDateSearch = DateTime.Now.ToString("yyyy/MM/dd");
		// Số phút quét Bitrix để lấy won deal
		public static int quetSauPhut = 5;
		// Số ngày lấy deal từ sau ngày hiện tại
		public static int soNgayQuet = 1;
		public static int guiLaiEmailSau = 50;
		public static bool haveReloadConfig = true;
		// Lấy thông tin cột ID ở trang Administration => Issues chọn tab Issue Type
		public const string IdIssueType_Epic = "10000";
		public const string IdIssueType_HDNgungHuyDichVu = "11600";
		public const string IdIssueType_ServiceRequest = "10301";
		// Lấy thông tin cột key ở trang Administration => Projects chọn tab Projects
		public const string IssueProject_TrienKhai = "TRIENKHAI";
		public const string IssueProject_ES = "ES";
		// Thông tin loại Deal
		public const string LoaiDeal_TrienKhaiMoi = "3095";
		public const string LoaiDeal_TrienKhaiBoSung = "3096";
		public const string LoaiDeal_NgungHuyDichVu = "3097";
		public const string LoaiDeal_ChuyenDoiDichVu = "3098";
		public const string LoaiDeal_CapKey = "3099";
		public const string LoaiDeal_HoTroKhac = "3100";
		// Thông tin gửi Email
		public const string MailInfo_RenewalManagerEmail = "sale3.hn@ezcloud.vn";
		public const string MailInfo_SalesManagerEmail = "huyenlt@ezcloud.vn";
		public const string MailInfo_AdminBitrixEmail = "huyenlt@ezcloud.vn";
		public const string MailInfo_AdminJiraEmail = "dung.nguyen@ezcloud.vn";

		public const string MailInfo_FromAddress = "ezfolio.notification@gmail.com";
		public const string MailInfo_password = "rrodawlgonlotsrf";
		public const string MailInfo_ToAddress = "anh.pham@ezcloud.vn";
		public const string MailInfo_Subject_Create_Iss_Success = "Tạo issues thành công từ Bitrix";
		public const string MailInfo_Subject_Create_Iss_Error = "Tạo issues từ Bitrix bị lỗi";
		public const string MailInfo_Body = "This is a test email sent from C#.";
		// Thông tin gửi Email SendGrid
		public const string SendGrid_FromAddress = "bitrix-jira.noreply@gmail.com";
		public const string SendGrid_FromName = "Bitrix Jira Connector";
		public const string SendGrid_API_Key = "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";
		// Danh sách trường bắt buộc
		public static List<string> fieldRequire_TrienKhaiMoi = new List<string> { "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID", "CATEGORY_ID", "UF_CRM_1708697569", "SOURCE_ID", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34",  "UF_CRM_5B3F32B1068C7", "UF_CRM_6054492BF24D8", "CONTACT_ID", "UF_CRM_1708697773", "UF_CRM_1708698013", "UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1614068621890", "UF_CRM_1616130516", "UF_CRM_1616130611" };
		public static List<string> fieldRequire_TrienKhaiBoSung = new List<string> { "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID", "UF_CRM_1708697569", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34", "UF_CRM_5B3F32B1068C7", "UF_CRM_6054492BF24D8", "CONTACT_ID", "UF_CRM_1708697773", 
			"UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1614068621890", "UF_CRM_1616130516", "UF_CRM_1616130611" };
		public static List<string> fieldRequire_NgungHuyDichVu = new List<string> { "UF_CRM_1713881390", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID", "CATEGORY_ID", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34", "CONTACT_ID", "UF_CRM_1570243307", "UF_CRM_1570243354", "UF_CRM_1708699318", "UF_CRM_1715997025" };
		public static List<string> fieldRequire_ChuyenDoiDichVu = new List<string> { "UF_CRM_1713881390", "UF_CRM_5D98084D1476E", "UF_CRM_5B3F32B1118E0", "ASSIGNED_BY_ID", "CATEGORY_ID", "UF_CRM_1708697569", "COMPANY_ID", "ID", "UF_CRM_5D984E2CA62C9", "UF_CRM_5D984E30EEE34", "UF_CRM_5B3F32B1068C7", "UF_CRM_6054492BF24D8", "CONTACT_ID", "UF_CRM_1613788692724", "UF_CRM_1616123465", "UF_CRM_1616130516", "UF_CRM_1614068621890" };
		public static List<string> fieldRequire_CapKey = new List<string> { "UF_CRM_1713881390", "ASSIGNED_BY_ID", "TITLE", "UF_CRM_5B3F32B1118E0", "UF_CRM_1600922068", "UF_CRM_1600922103", "UF_CRM_1715997550", "CONTACT_ID", "UF_CRM_1613788692724" };
		public static List<string> fieldRequire_HoTroKhac = new List<string> { "UF_CRM_1713881390", "ASSIGNED_BY_ID", "TITLE", "UF_CRM_1531008138", "CONTACT_ID", "UF_CRM_1613788692724" };

		/// <summary>
		/// Key trả ra tử api map với tên trên giao diện
		/// </summary>
		public static Dictionary<string, string> keyValueField_Bitrix = new Dictionary<string, string>
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
			// { "UF_CRM_5D984E38B7502", "Địa chỉ Khách sạn: Tỉnh/ Thành phố" }, // Tạm thời bỏ do cấu hình bị thay đổi thành loại khác
			// { "UF_CRM_5D984E34ACFB6", "Địa chỉ Khách sạn: Quận/ Huyện" }, // Bỏ do không còn Quận/Huyện nữa
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
}
