using System.Text;
using Atlassian.Jira;
using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Models.Bitrix;
using BitrixJiraConnector.Api.Models.Database;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BitrixJiraConnector.Api.Services;

public class JiraService : IJiraService
{
    private readonly JiraSettings _jiraSettings;
    private readonly IDbService _dbService;
    private readonly IEmailService _emailService;
    private readonly ILogger<JiraService> _logger;

    public JiraService(
        IOptions<JiraSettings> jiraSettings,
        IDbService dbService,
        IEmailService emailService,
        ILogger<JiraService> logger)
    {
        _jiraSettings = jiraSettings.Value;
        _dbService = dbService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Issue?> CreateIssueAsync(BitrixDataDeal deal)
    {
        try
        {
            return deal.LoaiDeal switch
            {
                ConfigJiraBitrix.LoaiDeal_TrienKhaiMoi => await CreateTrienKhaiMoiAsync(deal),
                ConfigJiraBitrix.LoaiDeal_TrienKhaiBoSung => await CreateTrienKhaiBoSungAsync(deal),
                ConfigJiraBitrix.LoaiDeal_NgungHuyDichVu => await CreateNgungHuyDichVuAsync(deal),
                ConfigJiraBitrix.LoaiDeal_ChuyenDoiDichVu => await CreateChuyenDoiDichVuAsync(deal),
                ConfigJiraBitrix.LoaiDeal_CapKey => await CreateCapKeyAsync(deal),
                ConfigJiraBitrix.LoaiDeal_HoTroKhac => await CreateHoTroKhacAsync(deal),
                _ => null
            };
        }
        catch (Exception ex)
        {
            await SaveExceptionLogAsync(ex, deal.DealID);
            throw;
        }
    }

    private async Task<Issue?> CreateTrienKhaiMoiAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai);
        issue.Type = ConfigJiraBitrix.IdIssueType_Epic;
        issue.Summary = $"Triển Khai - {deal.NhuCauSanPham} - {deal.TenKhachSan}";
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        issue["Người yêu cầu"] = deal.Responsible_FirstName + " " + deal.Responsible_LastName;
        issue["Team bán hàng"] = GetTeamByCategoryId(deal.Pipeline);
        issue["Market"] = deal.ThiTruong;
        issue["Phân khúc KH"] = deal.LoaiHinhKhachSan;
        issue["Khách hàng mới hay cũ"] = deal.Source == "9" ? "Cũ" : "Mới";
        issue["Mã khách hàng (company ID)"] = deal.CompanyId;
        issue["Link CRM"] = deal.LinkCRM;
        issue["Mã hợp đồng (deal ID)"] = deal.DealID;
        issue["Epic Name"] = deal.TenKhachSan;
        issue["Địa điểm khách sạn"] = BuildDiaChiKhachSan(deal);
        issue["Tỉnh/Thành phố"] = deal.DC_Khach_TinhThanhPho;
        issue["Quận/Huyện"] = deal.DC_Khach_QuanHuyen;
        issue["Phường/Xã"] = deal.DC_Khach_PhuongXa;
        issue["Số phòng"] = deal.SoPhong;
        issue["Số giường (nếu bán theo mô hình giường)"] = deal.SoGiuong;
        issue["Tên khách hàng"] = deal.Client_Contact_LastName_Name;
        issue["Chức vụ khách hàng"] = deal.Client_Contact_Position;
        issue["Số điện thoại khách hàng"] = deal.Client_Contact_Phone;
        issue["Email khách hàng"] = deal.Client_Contact_Email;
        issue["Chính sách giá"] = deal.ChinhSachGia;
        if (!await AddComponentsAsync(issue, deal)) return issue;
        issue["Máy chủ chạy phần mềm"] = deal.MayChuChayPhanMem;
        issue["Dùng thử hay dùng thật"] = deal.DungThuHayDungThat;
        issue["Trạng thái thanh toán"] = deal.TrangThaiThanhToanLan1;
        issue["Mô tả chi tiết thanh toán"] = deal.NoiDungChuyenKhoanLan1;
        issue["Phương thức triển khai"] = deal.PhuongThucTrienKhai;
        if (deal.ThoiDiemTrienKhai != null) issue["Thời điểm triển khai"] = deal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
        issue["Tài khoản dùng thử PMS (nếu có)"] = deal.TaikhoanDungThuPMS;
        issue["Nếu triển khai BE, có tích hợp ezFolio/Cloud không?"] = deal.Neu_TK_BE_CoTichHop_ezFolioKhong;
        issue["Thông tin triển khai web"] = deal.ThongTinTrienKhaiWeb;
        issue.Description = deal.GhiChu;
        issue["Customer Request Type"] = "Triển khai mới";
        issue = await issue.SaveChangesAsync();
        if (issue != null) await AttachFilesAsync(jira, issue, deal.HopDong_PhuLuc);
        return issue;
    }

    private async Task<Issue?> CreateTrienKhaiBoSungAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai);
        issue.Type = ConfigJiraBitrix.IdIssueType_Epic;
        issue.Summary = $"{deal.YeuCauThem} -  {deal.TenKhachSan}";
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        issue["Team bán hàng"] = GetTeamByCategoryId(deal.Pipeline);
        issue["Người yêu cầu"] = deal.Responsible_FirstName + " " + deal.Responsible_LastName;
        issue["Market"] = deal.ThiTruong;
        issue["Mã khách hàng (company ID)"] = deal.CompanyId;
        issue["Link CRM"] = deal.LinkCRM;
        issue["Mã hợp đồng (deal ID)"] = deal.DealID;
        if (!string.IsNullOrEmpty(deal.MaKhachSan))
        {
            if (!int.TryParse(deal.MaKhachSan, out _))
            {
                _logger.LogWarning("DealID {DealId}: Mã khách sạn chứa chữ: {MaKhachSan}", deal.DealID, deal.MaKhachSan);
                return issue;
            }
            issue["Mã khách sạn"] = deal.MaKhachSan;
        }
        issue["Epic Name"] = deal.TenKhachSan;
        issue["Địa điểm khách sạn"] = BuildDiaChiKhachSan(deal);
        issue["Số phòng"] = deal.SoPhong;
        issue["Số giường (nếu bán theo mô hình giường)"] = deal.SoGiuong;
        issue["Tên khách hàng"] = deal.Client_Contact_LastName_Name;
        issue["Chức vụ khách hàng"] = deal.Client_Contact_Position;
        issue["Số điện thoại khách hàng"] = deal.Client_Contact_Phone;
        issue["Email khách hàng"] = deal.Client_Contact_Email;
        issue["Chính sách giá"] = deal.ChinhSachGia;
        if (!await AddComponentsAsync(issue, deal)) return issue;
        issue["Yêu cầu thêm"] = deal.YeuCauThem;
        issue["Trạng thái thanh toán"] = deal.TrangThaiThanhToanLan1;
        issue["Mô tả chi tiết thanh toán"] = deal.NoiDungChuyenKhoanLan1;
        issue["Phương thức triển khai"] = deal.PhuongThucTrienKhai;
        if (deal.ThoiDiemTrienKhai != null) issue["Thời điểm triển khai"] = deal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
        issue.Description = deal.GhiChu;
        issue["Customer Request Type"] = "Triển khai bổ sung (Sale)";
        issue = await issue.SaveChangesAsync();
        if (issue != null) await AttachFilesAsync(jira, issue, deal.HopDong_PhuLuc);
        return issue;
    }

    private async Task<Issue?> CreateNgungHuyDichVuAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai);
        issue.Type = ConfigJiraBitrix.IdIssueType_HDNgungHuyDichVu;
        issue.Summary = $"Ngừng/Hủy dịch vụ - {deal.TenKhachSan}";
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        issue["Team bán hàng"] = GetTeamByCategoryId(deal.Pipeline);
        issue["Mã khách hàng (company ID)"] = deal.CompanyId;
        issue["Mã hợp đồng (deal ID)"] = deal.DealID;
        issue["Link CRM"] = deal.LinkCRM;
        if (!string.IsNullOrEmpty(deal.MaKhachSan))
        {
            if (!int.TryParse(deal.MaKhachSan, out _))
            {
                _logger.LogWarning("DealID {DealId}: Mã khách sạn chứa chữ: {MaKhachSan}", deal.DealID, deal.MaKhachSan);
                return issue;
            }
            issue["Mã khách sạn"] = deal.MaKhachSan;
        }
        issue["Địa điểm khách sạn"] = BuildDiaChiKhachSan(deal);
        issue["Tỉnh/Thành phố"] = deal.DC_Khach_TinhThanhPho;
        issue["Tên khách hàng"] = deal.Client_Contact_LastName_Name;
        issue["Chức vụ khách hàng"] = deal.Client_Contact_Position;
        issue["Số điện thoại khách hàng"] = deal.Client_Contact_Phone;
        issue["Email khách hàng"] = deal.Client_Contact_Email;
        if (!await AddComponentsAsync(issue, deal)) return issue;
        issue.CustomFields.AddArray("Lý do ngừng/hủy", deal.LyDoLost.ToArray());
        issue["Cụ thể yêu cầu"] = deal.GhichuChoLyDoLost;
        issue["Loại yêu cầu huỷ"] = deal.LoaiYeuCauHuy;
        if (deal.ThoiDiemTrienKhai != null) issue["Thời điểm triển khai"] = deal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
        issue.Description = deal.GhiChu;
        issue = await issue.SaveChangesAsync();
        return issue;
    }

    private async Task<Issue?> CreateChuyenDoiDichVuAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai);
        issue.Type = ConfigJiraBitrix.IdIssueType_Epic;
        issue.Summary = $"Chuyển đổi - {deal.NhuCauSanPham} -  {deal.TenKhachSan}";
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        issue["Người yêu cầu"] = deal.Responsible_FirstName + " " + deal.Responsible_LastName;
        issue["Team bán hàng"] = GetTeamByCategoryId(deal.Pipeline);
        issue["Market"] = deal.ThiTruong;
        issue["Phân khúc KH"] = deal.LoaiHinhKhachSan;
        issue["Mã khách hàng (company ID)"] = deal.CompanyId;
        issue["Mã hợp đồng (deal ID)"] = deal.DealID;
        issue["Link CRM"] = deal.LinkCRM;
        if (!string.IsNullOrEmpty(deal.MaKhachSan))
        {
            if (!int.TryParse(deal.MaKhachSan, out _))
            {
                _logger.LogWarning("DealID {DealId}: Mã khách sạn chứa chữ: {MaKhachSan}", deal.DealID, deal.MaKhachSan);
                return issue;
            }
            issue["Mã khách sạn"] = deal.MaKhachSan;
        }
        issue["Epic Name"] = deal.TenKhachSan;
        issue["Địa điểm khách sạn"] = BuildDiaChiKhachSan(deal);
        issue["Số phòng"] = deal.SoPhong;
        issue["Số giường (nếu bán theo mô hình giường)"] = deal.SoGiuong;
        issue["Tên khách hàng"] = deal.Client_Contact_LastName_Name;
        issue["Chức vụ khách hàng"] = deal.Client_Contact_Position;
        issue["Số điện thoại khách hàng"] = deal.Client_Contact_Phone;
        issue["Email khách hàng"] = deal.Client_Contact_Email;
        issue["Chính sách giá"] = deal.ChinhSachGia;
        if (!await AddComponentsAsync(issue, deal)) return issue;
        issue["Trạng thái thanh toán"] = deal.TrangThaiThanhToanLan1;
        issue["Mô tả chi tiết thanh toán"] = deal.NoiDungChuyenKhoanLan1;
        issue["Phương thức triển khai"] = deal.PhuongThucTrienKhai;
        if (deal.ThoiDiemTrienKhai != null) issue["Thời điểm triển khai"] = deal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
        issue.Description = deal.GhiChu;
        issue["Customer Request Type"] = "Chuyển đổi dịch vụ";
        issue = await issue.SaveChangesAsync();
        if (issue != null) await AttachFilesAsync(jira, issue, deal.HopDong_PhuLuc);
        return issue;
    }

    private async Task<Issue?> CreateCapKeyAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_ES);
        issue.Type = ConfigJiraBitrix.IdIssueType_ServiceRequest;
        issue.Summary = deal.TenDeal;
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        var productNames = deal.DanhSachSanPham.Select(p => p.ORIGINAL_PRODUCT_NAME);
        string ngayBD = deal.NgayBatDauTinh_GHBT?.ToString("dd/MM/yyyy") ?? "";
        string ngayKT = deal.NgayKetThucHan_GHBT?.ToString("dd/MM/yyyy") ?? "";
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"Cấp key GHBT cho {deal.TenKhachSan}");
        sb.AppendLine(string.Join(Environment.NewLine, productNames.Select(p => "- " + p)));
        sb.AppendLine($"Hạn từ {ngayBD} đến {ngayKT}");
        sb.AppendLine("Thanks & Best Regards!");
        issue.Description = sb.ToString();
        issue["Tình huống cần cấp key"] = deal.TinhHuongCanCapKey;
        issue["Contact"] = $"{deal.Client_Contact_LastName_Name} - {deal.Client_Contact_Position} - {deal.Client_Contact_Phone} - {deal.Client_Contact_Email}";
        issue = await issue.SaveChangesAsync();
        if (issue != null) await AttachFilesAsync(jira, issue, deal.HopDong_PhuLuc);
        return issue;
    }

    private async Task<Issue?> CreateHoTroKhacAsync(BitrixDataDeal deal)
    {
        var jira = Jira.CreateRestClient(_jiraSettings.Url, _jiraSettings.User, _jiraSettings.Password);
        var issue = jira.CreateIssue(ConfigJiraBitrix.IssueProject_ES);
        issue.Type = ConfigJiraBitrix.IdIssueType_ServiceRequest;
        issue.Summary = deal.TenDeal;
        string reporter = await GetJiraUsernameByEmailAsync(jira, deal.Responsible_Email);
        issue.Reporter = reporter;
        issue.Description = deal.GhiChu;
        issue["Contact"] = $"{deal.Client_Contact_LastName_Name} - {deal.Client_Contact_Position} - {deal.Client_Contact_Phone} - {deal.Client_Contact_Email}";
        issue = await issue.SaveChangesAsync();
        if (issue != null) await AttachFilesAsync(jira, issue, deal.HopDong_PhuLuc);
        return issue;
    }

    private async Task<bool> AddComponentsAsync(Issue issue, BitrixDataDeal deal)
    {
        string? lastProductName = null;
        try
        {
            foreach (var product in deal.DanhSachSanPham)
            {
                lastProductName = product.PRODUCT_NAME;
                issue.Components.Add(product.PRODUCT_NAME);
            }
            return true;
        }
        catch (Exception ex)
        {
            await SaveExceptionLogAsync(ex, deal.DealID);
            string subject = ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Error + " - DealID: " + deal.DealID;
            string body = $"<html><body><p>Không tìm thấy Jira Component tương ứng với Product <b>{lastProductName}</b> trên Bitrix</p></body></html>";
            await _emailService.SendEmailAsync(subject, body, deal.Responsible_Email, true);
            return false;
        }
    }

    private async Task AttachFilesAsync(Jira jira, Issue issue, List<FileHopDongAttach> files)
    {
        if (files == null || !files.Any()) return;
        foreach (var file in files)
        {
            if (!File.Exists(file.path_file)) continue;
            var info = new FileInfo(file.path_file);
            if (info.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("Skipping attachment {FileName} — exceeds 10MB", file.file_Name);
                continue;
            }
            var bytes = await File.ReadAllBytesAsync(file.path_file);
            await issue.AddAttachmentAsync(new[] { new UploadAttachmentInfo(file.file_Name, bytes) });
        }
    }

    private async Task<string> GetJiraUsernameByEmailAsync(Jira jira, string email)
    {
        try
        {
            string username = email.Replace("@ezcloud.vn", "");
            if (email == "dung.nguyen@ezcloud.vn") username = "dungnn";
            if (email == "duong.nguyen@ezcloud.vn") username = "duongnh";
            var user = await jira.Users.GetUserAsync(username);
            if (user != null && user.Username.Length > 0) return user.Username;
        }
        catch
        {
            // fallback to default user
        }
        return _jiraSettings.User;
    }

    private static string GetTeamByCategoryId(string categoryId) => categoryId switch
    {
        "0" => "Sales",
        "3" => "Renewal",
        "4" => "Cross-Sales",
        "6" => "Project Sale",
        _ => "Sales"
    };

    private static string BuildDiaChiKhachSan(BitrixDataDeal deal)
        => $"{deal.DC_Khach_SoNhaDuong}, {deal.DC_Khach_PhuongXa}, {deal.DC_Khach_QuanHuyen}, {deal.DC_Khach_TinhThanhPho}";

    public async Task SaveExceptionLogAsync(Exception ex, string dealId)
    {
        await _dbService.AddLogExceptionAsync(new ExceptionLog
        {
            DealID = int.TryParse(dealId, out int id) ? id : 0,
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace ?? "",
            ExceptionType = ex.GetType().ToString(),
            Source = ex.Source ?? "",
            LoggedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        });
    }
}
