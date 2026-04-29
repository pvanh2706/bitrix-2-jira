namespace BitrixJiraConnector.Api.Models.Bitrix;

public class BitrixDataDeal
{
    public string LoaiDeal { get; set; } = "";
    public string NhuCauSanPham { get; set; } = "";
    public string TenKhachSan { get; set; } = "";
    public string Responsible_UserID { get; set; } = "";
    public string Responsible_Email { get; set; } = "";
    public string Responsible_FirstName { get; set; } = "";
    public string Responsible_LastName { get; set; } = "";
    public string Pipeline { get; set; } = "";
    public string ThiTruong { get; set; } = "";
    public string LoaiHinhKhachSan { get; set; } = "";
    public string Source { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string LinkCRM { get; set; } = "";
    public string DealID { get; set; } = "";
    public string DC_Khach_SoNhaDuong { get; set; } = "";
    public string DC_Khach_TinhThanhPho { get; set; } = "";
    public string DC_Khach_QuanHuyen { get; set; } = "";
    public string DC_Khach_PhuongXa { get; set; } = "";
    public string SoPhong { get; set; } = "";
    public string SoGiuong { get; set; } = "";
    public string Client_Contact_ContactID { get; set; } = "";
    public string Client_Contact_LastName_Name { get; set; } = "";
    public string Client_Contact_Position { get; set; } = "";
    public string Client_Contact_Phone { get; set; } = "";
    public string Client_Contact_Email { get; set; } = "";
    public string ChinhSachGia { get; set; } = "";
    public List<DataProductInDeal> DanhSachSanPham { get; set; } = new();
    public string MayChuChayPhanMem { get; set; } = "";
    public string DungThuHayDungThat { get; set; } = "";
    public string TrangThaiThanhToanLan1 { get; set; } = "";
    public string NoiDungChuyenKhoanLan1 { get; set; } = "";
    public string PhuongThucTrienKhai { get; set; } = "";
    public DateTime? ThoiDiemTrienKhai { get; set; }
    public string TaikhoanDungThuPMS { get; set; } = "";
    public string Neu_TK_BE_CoTichHop_ezFolioKhong { get; set; } = "";
    public string ThongTinTrienKhaiWeb { get; set; } = "";
    public string GhiChu { get; set; } = "";
    public string MaKhachSan { get; set; } = "";
    public string YeuCauThem { get; set; } = "";
    public List<string> LyDoLost { get; set; } = new();
    public string GhichuChoLyDoLost { get; set; } = "";
    public string LoaiYeuCauHuy { get; set; } = "";
    public DateTime? ThoiDiemNgungHuy { get; set; }
    public string TenDeal { get; set; } = "";
    public DateTime? NgayBatDauTinh_GHBT { get; set; }
    public DateTime? NgayKetThucHan_GHBT { get; set; }
    public string TinhHuongCanCapKey { get; set; } = "";
    public List<FileHopDongAttach> HopDong_PhuLuc { get; set; } = new();
}
