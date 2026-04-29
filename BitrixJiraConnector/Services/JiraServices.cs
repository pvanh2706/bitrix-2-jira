using Atlassian.Jira;
using BitrixJiraConnector.Configurations;
using BitrixJiraConnector.Helpers;
using BitrixJiraConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BitrixJiraConnector.Services
{
	class JiraServices
	{
		public Issue returnIssue;
		public string UserName = ConfigJiraBitrix.jiraUser;
		public string Password = ConfigJiraBitrix.jiraPass;
		public string JiraUrl = ConfigJiraBitrix.jiraUrl;
		public string productNameCheck = "";
		public async Task<Issue> CreateIssJira(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				switch (bitrixDataDeal.LoaiDeal)
				{
					case ConfigJiraBitrix.LoaiDeal_TrienKhaiMoi:
						returnIssue = await CreateIssFromDeal_TrienKhaiMoi(bitrixDataDeal);
						break;
					case ConfigJiraBitrix.LoaiDeal_TrienKhaiBoSung:
						returnIssue = await CreateIssFromDeal_TrienKhaiBoSung(bitrixDataDeal);
						break;
					case ConfigJiraBitrix.LoaiDeal_NgungHuyDichVu:
						returnIssue = await CreateIssFromDeal_NgungHuyDichVu(bitrixDataDeal);
						break;
					case ConfigJiraBitrix.LoaiDeal_ChuyenDoiDichVu:
						returnIssue = await CreateIssFromDeal_ChuyenDoiDichVu(bitrixDataDeal);
						break;
					case ConfigJiraBitrix.LoaiDeal_CapKey:
						returnIssue = await CreateIssFromDeal_CapKey(bitrixDataDeal);
						break;
					case ConfigJiraBitrix.LoaiDeal_HoTroKhac:
						returnIssue = await CreateIssFromDeal_HoTroKhac(bitrixDataDeal);
						break;
					default:
						string message = $"Không thể tạo iss do chưa chọn Loại Deal {bitrixDataDeal.LinkCRM}";
						break;
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_TrienKhaiMoi(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_Epic; // Issue Type
				returnIssue.Summary = $"Triển Khai - {bitrixDataDeal.NhuCauSanPham} - {bitrixDataDeal.TenKhachSan}";
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				returnIssue["Người yêu cầu"] = bitrixDataDeal.Responsible_FirstName + " " + bitrixDataDeal.Responsible_LastName;
				returnIssue["Team bán hàng"] = await getTeamByCategoryID_Jira(bitrixDataDeal.Pipeline);
				returnIssue["Market"] = bitrixDataDeal.ThiTruong;
				returnIssue["Phân khúc KH"] = bitrixDataDeal.LoaiHinhKhachSan;
				returnIssue["Khách hàng mới hay cũ"] = bitrixDataDeal.Source == "9" ? "Cũ" : "Mới"; // Direct - Khách cũ mua thêm => Cũ
				returnIssue["Mã khách hàng (company ID)"] = bitrixDataDeal.CompanyId;
				returnIssue["Link CRM"] = bitrixDataDeal.LinkCRM;
				returnIssue["Mã hợp đồng (deal ID)"] = bitrixDataDeal.DealID;
				returnIssue["Epic Name"] = bitrixDataDeal.TenKhachSan;
				returnIssue["Địa điểm khách sạn"] = await getDiaChiKhachSan_Jira(bitrixDataDeal);
				returnIssue["Tỉnh/Thành phố"] = bitrixDataDeal.DC_Khach_TinhThanhPho;
				returnIssue["Quận/Huyện"] = bitrixDataDeal.DC_Khach_QuanHuyen;
				returnIssue["Phường/Xã"] = bitrixDataDeal.DC_Khach_PhuongXa;
				returnIssue["Số phòng"] = bitrixDataDeal.SoPhong;
				returnIssue["Số giường (nếu bán theo mô hình giường)"] = bitrixDataDeal.SoGiuong;
				returnIssue["Tên khách hàng"] = bitrixDataDeal.Client_Contact_LastName_Name;
				returnIssue["Chức vụ khách hàng"] = bitrixDataDeal.Client_Contact_Position;
				returnIssue["Số điện thoại khách hàng"] = bitrixDataDeal.Client_Contact_Phone;
				returnIssue["Email khách hàng"] = bitrixDataDeal.Client_Contact_Email;
				returnIssue["Chính sách giá"] = bitrixDataDeal.ChinhSachGia;
				try
				{
					foreach (var itemProduct in bitrixDataDeal.DanhSachSanPham)
					{
						productNameCheck = itemProduct.PRODUCT_NAME;
						returnIssue.Components.Add(itemProduct.PRODUCT_NAME);
					}
				}
				catch (Exception ex)
				{
					await SaveExceptionLog(ex, bitrixDataDeal.DealID);
					await SendEmailErrorCreateIssue(bitrixDataDeal);
					return returnIssue;
				}
			
				returnIssue["Máy chủ chạy phần mềm"] = bitrixDataDeal.MayChuChayPhanMem;
				returnIssue["Dùng thử hay dùng thật"] = bitrixDataDeal.DungThuHayDungThat;
				returnIssue["Trạng thái thanh toán"] = bitrixDataDeal.TrangThaiThanhToanLan1;
				returnIssue["Mô tả chi tiết thanh toán"] = bitrixDataDeal.NoiDungChuyenKhoanLan1;
				returnIssue["Phương thức triển khai"] = bitrixDataDeal.PhuongThucTrienKhai;
				if (bitrixDataDeal.ThoiDiemTrienKhai != null)
				{
					returnIssue["Thời điểm triển khai"] = bitrixDataDeal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
				}
				returnIssue["Tài khoản dùng thử PMS (nếu có)"] = bitrixDataDeal.TaikhoanDungThuPMS;
				returnIssue["Nếu triển khai BE, có tích hợp ezFolio/Cloud không?"] = bitrixDataDeal.Neu_TK_BE_CoTichHop_ezFolioKhong;
				returnIssue["Thông tin triển khai web"] = bitrixDataDeal.ThongTinTrienKhaiWeb;
				returnIssue.Description = bitrixDataDeal.GhiChu;
				returnIssue["Customer Request Type"] = "Triển khai mới";

				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
				if (returnIssue != null) await setFileAttach_Jira(Jira, returnIssue, bitrixDataDeal.HopDong_PhuLuc);
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				// throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_TrienKhaiBoSung(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_Epic; // Issue Type
				returnIssue.Summary = $"{bitrixDataDeal.YeuCauThem} -  {bitrixDataDeal.TenKhachSan}";
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				returnIssue["Team bán hàng"] = await getTeamByCategoryID_Jira(bitrixDataDeal.Pipeline);
				returnIssue["Người yêu cầu"] = bitrixDataDeal.Responsible_FirstName + " " + bitrixDataDeal.Responsible_LastName;
				returnIssue["Market"] = bitrixDataDeal.ThiTruong;
				returnIssue["Mã khách hàng (company ID)"] = bitrixDataDeal.CompanyId;
				returnIssue["Link CRM"] = bitrixDataDeal.LinkCRM;
				returnIssue["Mã hợp đồng (deal ID)"] = bitrixDataDeal.DealID;
				string MaKhachSan = bitrixDataDeal.MaKhachSan;
				if (!string.IsNullOrEmpty(MaKhachSan))
				{
					if (int.TryParse(MaKhachSan, out int result))
					{
						returnIssue["Mã khách sạn"] = result.ToString();
					}
					else
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine();
						sb.AppendLine("Không tạo được iss do [Mã khách sạn] chứa chữ");
						sb.AppendLine($"Link Deal: {bitrixDataDeal.LinkCRM}");
						return returnIssue;
					}
				}
				returnIssue["Epic Name"] = bitrixDataDeal.TenKhachSan;
				returnIssue["Địa điểm khách sạn"] = await getDiaChiKhachSan_Jira(bitrixDataDeal);
				returnIssue["Số phòng"] = bitrixDataDeal.SoPhong;
				returnIssue["Số giường (nếu bán theo mô hình giường)"] = bitrixDataDeal.SoGiuong;
				returnIssue["Tên khách hàng"] = bitrixDataDeal.Client_Contact_LastName_Name;
				returnIssue["Chức vụ khách hàng"] = bitrixDataDeal.Client_Contact_Position;
				returnIssue["Số điện thoại khách hàng"] = bitrixDataDeal.Client_Contact_Phone;
				returnIssue["Email khách hàng"] = bitrixDataDeal.Client_Contact_Email;
				returnIssue["Chính sách giá"] = bitrixDataDeal.ChinhSachGia;
				try
				{
					foreach (var itemProduct in bitrixDataDeal.DanhSachSanPham)
					{
						productNameCheck = itemProduct.PRODUCT_NAME;
						returnIssue.Components.Add(itemProduct.PRODUCT_NAME);
					}
				}
				catch (Exception ex)
				{
					await SaveExceptionLog(ex, bitrixDataDeal.DealID);
					await SendEmailErrorCreateIssue(bitrixDataDeal);
					return returnIssue;
				}
				returnIssue["Yêu cầu thêm"] = bitrixDataDeal.YeuCauThem;
				returnIssue["Trạng thái thanh toán"] = bitrixDataDeal.TrangThaiThanhToanLan1;
				returnIssue["Mô tả chi tiết thanh toán"] = bitrixDataDeal.NoiDungChuyenKhoanLan1;
				returnIssue["Phương thức triển khai"] = bitrixDataDeal.PhuongThucTrienKhai;
				if (bitrixDataDeal.ThoiDiemTrienKhai != null)
				{
					returnIssue["Thời điểm triển khai"] = bitrixDataDeal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
				}
				returnIssue.Description = bitrixDataDeal.GhiChu;
				returnIssue["Customer Request Type"] = "Triển khai bổ sung (Sale)";
				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
				if (returnIssue != null) await setFileAttach_Jira(Jira, returnIssue, bitrixDataDeal.HopDong_PhuLuc);
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_NgungHuyDichVu(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_HDNgungHuyDichVu; // Issue Type
				returnIssue.Summary = $"Ngừng/Hủy dịch vụ - {bitrixDataDeal.TenKhachSan}";
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				returnIssue["Team bán hàng"] = await getTeamByCategoryID_Jira(bitrixDataDeal.Pipeline);
				returnIssue["Mã khách hàng (company ID)"] = bitrixDataDeal.CompanyId;
				returnIssue["Mã hợp đồng (deal ID)"] = bitrixDataDeal.DealID;
				returnIssue["Link CRM"] = bitrixDataDeal.LinkCRM;
				string MaKhachSan = bitrixDataDeal.MaKhachSan;
				if (!string.IsNullOrEmpty(MaKhachSan))
				{
					if (int.TryParse(MaKhachSan, out int result))
					{
						returnIssue["Mã khách sạn"] = result.ToString();
					}
					else
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine();
						sb.AppendLine("Không tạo được iss do [Mã khách sạn] chứa chữ");
						sb.AppendLine($"Link Deal: {bitrixDataDeal.LinkCRM}");
						return returnIssue;
					}
				}
				returnIssue["Địa điểm khách sạn"] = await getDiaChiKhachSan_Jira(bitrixDataDeal);
				returnIssue["Tỉnh/Thành phố"] = bitrixDataDeal.DC_Khach_TinhThanhPho;
				returnIssue["Tên khách hàng"] = bitrixDataDeal.Client_Contact_LastName_Name;
				returnIssue["Chức vụ khách hàng"] = bitrixDataDeal.Client_Contact_Position;
				returnIssue["Số điện thoại khách hàng"] = bitrixDataDeal.Client_Contact_Phone;
				returnIssue["Email khách hàng"] = bitrixDataDeal.Client_Contact_Email;
				try
				{
					foreach (var itemProduct in bitrixDataDeal.DanhSachSanPham)
					{
						productNameCheck = itemProduct.PRODUCT_NAME;
						returnIssue.Components.Add(itemProduct.PRODUCT_NAME);
					}
				}
				catch (Exception ex)
				{
					await SaveExceptionLog(ex, bitrixDataDeal.DealID);
					await SendEmailErrorCreateIssue(bitrixDataDeal);
					return returnIssue;
				}
				returnIssue.CustomFields.AddArray("Lý do ngừng/hủy", bitrixDataDeal.LyDoLost.ToArray());
				returnIssue["Cụ thể yêu cầu"] = bitrixDataDeal.GhichuChoLyDoLost;
				returnIssue["Loại yêu cầu huỷ"] = bitrixDataDeal.LoaiYeuCauHuy;
				if (bitrixDataDeal.ThoiDiemTrienKhai != null)
				{
					returnIssue["Thời điểm triển khai"] = bitrixDataDeal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
				}
				returnIssue.Description = bitrixDataDeal.GhiChu;
				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_ChuyenDoiDichVu(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_TrienKhai); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_Epic; // Issue Type
				returnIssue.Summary = $"Chuyển đổi - {bitrixDataDeal.NhuCauSanPham} -  {bitrixDataDeal.TenKhachSan}";
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				returnIssue["Người yêu cầu"] = bitrixDataDeal.Responsible_FirstName + " " + bitrixDataDeal.Responsible_LastName;
				returnIssue["Team bán hàng"] = await getTeamByCategoryID_Jira(bitrixDataDeal.Pipeline);
				returnIssue["Market"] = bitrixDataDeal.ThiTruong;
				returnIssue["Phân khúc KH"] = bitrixDataDeal.LoaiHinhKhachSan;
				returnIssue["Mã khách hàng (company ID)"] = bitrixDataDeal.CompanyId;
				returnIssue["Mã hợp đồng (deal ID)"] = bitrixDataDeal.DealID;
				returnIssue["Link CRM"] = bitrixDataDeal.LinkCRM;
				string MaKhachSan = bitrixDataDeal.MaKhachSan;
				if (!string.IsNullOrEmpty(MaKhachSan))
				{
					if (int.TryParse(MaKhachSan, out int result))
					{
						returnIssue["Mã khách sạn"] = result.ToString();
					}
					else
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine();
						sb.AppendLine("Không tạo được iss do [Mã khách sạn] chứa chữ");
						sb.AppendLine($"Link Deal: {bitrixDataDeal.LinkCRM}");
						return returnIssue;
					}
				}
				returnIssue["Epic Name"] = bitrixDataDeal.TenKhachSan;
				returnIssue["Địa điểm khách sạn"] = await getDiaChiKhachSan_Jira(bitrixDataDeal);
				returnIssue["Số phòng"] = bitrixDataDeal.SoPhong;
				returnIssue["Số giường (nếu bán theo mô hình giường)"] = bitrixDataDeal.SoGiuong;
				returnIssue["Tên khách hàng"] = bitrixDataDeal.Client_Contact_LastName_Name;
				returnIssue["Chức vụ khách hàng"] = bitrixDataDeal.Client_Contact_Position;
				returnIssue["Số điện thoại khách hàng"] = bitrixDataDeal.Client_Contact_Phone;
				returnIssue["Email khách hàng"] = bitrixDataDeal.Client_Contact_Email;
				returnIssue["Chính sách giá"] = bitrixDataDeal.ChinhSachGia;
				try
				{
					foreach (var itemProduct in bitrixDataDeal.DanhSachSanPham)
					{
						productNameCheck = itemProduct.PRODUCT_NAME;
						returnIssue.Components.Add(itemProduct.PRODUCT_NAME);
					}
				}
				catch (Exception ex)
				{
					await SaveExceptionLog(ex, bitrixDataDeal.DealID);
					await SendEmailErrorCreateIssue(bitrixDataDeal);
					return returnIssue;
				}
				
				returnIssue["Trạng thái thanh toán"] = bitrixDataDeal.TrangThaiThanhToanLan1;
				returnIssue["Mô tả chi tiết thanh toán"] = bitrixDataDeal.NoiDungChuyenKhoanLan1;
				returnIssue["Phương thức triển khai"] = bitrixDataDeal.PhuongThucTrienKhai;
				if (bitrixDataDeal.ThoiDiemTrienKhai != null)
				{
					returnIssue["Thời điểm triển khai"] = bitrixDataDeal.ThoiDiemTrienKhai?.ToString("yyyy-MM-dd");
				}
				returnIssue.Description = bitrixDataDeal.GhiChu;
				returnIssue["Customer Request Type"] = "Chuyển đổi dịch vụ";
				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
				if (returnIssue != null) await setFileAttach_Jira(Jira, returnIssue, bitrixDataDeal.HopDong_PhuLuc);
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_CapKey(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_ES); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_ServiceRequest; // Issue Type
				returnIssue.Summary = bitrixDataDeal.TenDeal;
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				List<string> danhSachTenSanPham = new List<string>();
				foreach (var itemProduct in bitrixDataDeal.DanhSachSanPham)
				{
					danhSachTenSanPham.Add(itemProduct.ORIGINAL_PRODUCT_NAME);
				}
				string ngayBatDauTinhGHBT = bitrixDataDeal.NgayBatDauTinh_GHBT == null ? "" : bitrixDataDeal.NgayBatDauTinh_GHBT?.ToString("dd/MM/yyyy");
				string ngayKetThucHanGHBT = bitrixDataDeal.NgayKetThucHan_GHBT == null ? "" : bitrixDataDeal.NgayKetThucHan_GHBT?.ToString("dd/MM/yyyy");
				StringBuilder sb = new StringBuilder();
				sb.AppendLine();
				sb.AppendLine($"Cấp key GHBT cho {bitrixDataDeal.TenKhachSan}");
				sb.AppendLine(string.Join(Environment.NewLine, danhSachTenSanPham.Select(p => "- " + p)));
				sb.AppendLine($"Hạn từ {ngayBatDauTinhGHBT} đến {ngayKetThucHanGHBT}");
				sb.AppendLine($"Thanks & Best Regards!");
				returnIssue.Description = sb.ToString();

				returnIssue["Tình huống cần cấp key"] = bitrixDataDeal.TinhHuongCanCapKey;
				returnIssue["Contact"] = $"{bitrixDataDeal.Client_Contact_LastName_Name} - {bitrixDataDeal.Client_Contact_Position} - {bitrixDataDeal.Client_Contact_Phone} - {bitrixDataDeal.Client_Contact_Email}";
				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
				if (returnIssue != null) await setFileAttach_Jira(Jira, returnIssue, bitrixDataDeal.HopDong_PhuLuc);
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				throw ex;
			}
			return returnIssue;
		}

		private async Task<Issue> CreateIssFromDeal_HoTroKhac(BitrixDataDeal bitrixDataDeal)
		{
			try
			{
				// Tạo iss
				Jira Jira = Jira.CreateRestClient(JiraUrl, UserName, Password);
				returnIssue = Jira.CreateIssue(ConfigJiraBitrix.IssueProject_ES); // Project
				returnIssue.Type = ConfigJiraBitrix.IdIssueType_ServiceRequest; // Issue Type
				returnIssue.Summary = bitrixDataDeal.TenDeal;
				string reporter = await getUserByEmail_Jira(Jira, bitrixDataDeal.Responsible_Email);
				if (reporter.Length == 0) reporter = ConfigJiraBitrix.jiraUser;
				returnIssue.Reporter = reporter;
				returnIssue.Description = bitrixDataDeal.GhiChu;
				returnIssue["Contact"] = $"{bitrixDataDeal.Client_Contact_LastName_Name} - {bitrixDataDeal.Client_Contact_Position} - {bitrixDataDeal.Client_Contact_Phone} - {bitrixDataDeal.Client_Contact_Email}";
				// Attachment file Hợp đồng
				returnIssue = await returnIssue.SaveChangesAsync();
				if (returnIssue != null) await setFileAttach_Jira(Jira, returnIssue, bitrixDataDeal.HopDong_PhuLuc);
			}
			catch (Exception ex)
			{
				await SaveExceptionLog(ex, bitrixDataDeal.DealID);
				throw ex;
			}
			return returnIssue;
		}

		private async Task<string> setFileAttach_Jira_(Jira jira, Issue issue, List<FileHopDongAttach> fileHopDongAttaches)
		{
			string pathLog = ConfigJiraBitrix.folderLog + DateTime.Now.ToString("ddMMyyy") + ".txt";
			File.AppendAllText(pathLog, "-----------------  Start ------------------------" + Environment.NewLine);
			string content = issue.Key.ToString();
			File.AppendAllText(pathLog, content + Environment.NewLine);

			var issueOfAttachFile = jira.Issues.GetIssueAsync(issue.Key.ToString());
			if (issueOfAttachFile != null)
			{
				foreach (var item in fileHopDongAttaches)
				{
					var path = item.path_file;
					string contentFile = $"FileName: {item.file_Name}_PathFile: {item.path_file}_UrlDownLoad: {item.downloadUrl}";
					File.AppendAllText(pathLog, contentFile + Environment.NewLine);
					if (File.Exists(path))
					{
						var fileAsByteArray = File.ReadAllBytes(path);
						var attachment = new UploadAttachmentInfo(item.file_Name, fileAsByteArray);
						await issue.AddAttachmentAsync(new UploadAttachmentInfo[] { attachment });
						// File.Delete(path);
						File.AppendAllText(pathLog, "=== Attach Success ==" + Environment.NewLine);
					}
				}
			}
			File.AppendAllText(pathLog, "-----------------  End ------------------------" + Environment.NewLine);
			return issue.Key.ToString();
		}

		private async Task<string> setFileAttach_Jira(Jira jira, Issue issue, List<FileHopDongAttach> fileHopDongAttaches)
		{
			string pathLog = ConfigJiraBitrix.folderLog + DateTime.Now.ToString("ddMMyyyy") + ".txt";
			File.AppendAllText(pathLog, "-----------------  Start ------------------------" + Environment.NewLine);
			string content = issue.Key.ToString();
			File.AppendAllText(pathLog, content + Environment.NewLine);

			var issueOfAttachFile = jira.Issues.GetIssueAsync(issue.Key.ToString());
			if (issueOfAttachFile != null)
			{
				foreach (var item in fileHopDongAttaches)
				{
					var path = item.path_file;
					string contentFile = $"FileName: {item.file_Name}_PathFile: {item.path_file}_UrlDownLoad: {item.downloadUrl}";
					File.AppendAllText(pathLog, contentFile + Environment.NewLine);

					if (File.Exists(path))
					{
						FileInfo fileInfo = new FileInfo(path);
						long fileSizeInBytes = fileInfo.Length;
						long tenMB = 10 * 1024 * 1024;

						if (fileSizeInBytes > tenMB)
						{
							File.AppendAllText(pathLog, $"--- File too large (>10MB): {item.file_Name} --- Skipped." + Environment.NewLine);
							continue; // Bỏ qua file này
						}

						var fileAsByteArray = File.ReadAllBytes(path);
						var attachment = new UploadAttachmentInfo(item.file_Name, fileAsByteArray);
						await issue.AddAttachmentAsync(new UploadAttachmentInfo[] { attachment });
						// File.Delete(path);
						File.AppendAllText(pathLog, "=== Attach Success ===" + Environment.NewLine);
					}
					else
					{
						File.AppendAllText(pathLog, $"--- File not found: {item.file_Name} ---" + Environment.NewLine);
					}
				}
			}

			File.AppendAllText(pathLog, "-----------------  End ------------------------" + Environment.NewLine);
			return issue.Key.ToString();
		}


		private async Task<string> getDiaChiKhachSan_Jira(BitrixDataDeal bitrixDataDeal)
		{
			return $"{bitrixDataDeal.DC_Khach_SoNhaDuong}, {bitrixDataDeal.DC_Khach_PhuongXa}, {bitrixDataDeal.DC_Khach_QuanHuyen}, {bitrixDataDeal.DC_Khach_TinhThanhPho}";
		}

		/// <summary>
		/// Hàm đang fix để lấy tên Team trên jira
		/// Dev chưa tìm được api + bị deadline nên fix tạm
		/// </summary>
		/// <param name="categoryID">CategoryID(PipeLine) trả ra khi lấy thông tin chi tiết của Deal trên Bitrix</param>
		/// <returns>Chuối để gán vào trường Team trên Jira</returns>
		private async Task<string> getTeamByCategoryID_Jira(string categoryID)
		{
			string result = "Sales";
			switch (categoryID)
			{
				case "0":
					result = "Sales";
					break;
				case "3":
					result = "Renewal";
					break;
				case "4":
					result = "Cross-Sales";
					break;
				case "6":
					result = "Project Sale";
					break;
				default:
					break;
			}
			return result;
		}

		/// <summary>
		/// Lấy thông tin username trên jira 
		/// </summary>
		/// <param name="objJira"></param>
		/// <param name="email"></param>
		/// <returns></returns>
		private async Task<string> getUserByEmail_Jira(Jira objJira, string email)
		{
			string userName = "";
			try
			{
				string userTmp = email.Replace("@ezcloud.vn", "");
				if (email == "dung.nguyen@ezcloud.vn") userTmp = "dungnn";
				if (email == "duong.nguyen@ezcloud.vn") userTmp = "duongnh";

				var obj = objJira.Users.GetUserAsync(userTmp).Result;
				if (obj != null && obj.Username.Length > 0) userName = obj.Username;
			}
			catch (Exception ex)
			{
				return ConfigJiraBitrix.jiraUser;
			}
			return userName;
		}

		public async Task SaveExceptionLog(Exception ex, string DealID)
		{
			ExceptionLog exceptionLog = new ExceptionLog();
			exceptionLog.DealID = int.Parse(DealID);
			exceptionLog.ExceptionMessage = ex.Message;
			exceptionLog.StackTrace = ex.StackTrace ?? "";
			exceptionLog.ExceptionType = ex.GetType().ToString();
			exceptionLog.Source = ex.Source ?? "";
			exceptionLog.LoggedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			await BitrixJiraDBServices.AddLogException(exceptionLog);
		}

		public async Task SendEmailErrorCreateIssue(BitrixDataDeal bitrixDataDeal)
		{
			string subjectMail = ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Error + " - DealID: " + bitrixDataDeal.DealID;
			string addressEmailCreateDeal = bitrixDataDeal.Responsible_Email;
			bool isSendEmailToAdminBitrixJira = true;

			string productNameNotMap = productNameCheck;
			var htmlContent = @$"
					<html>
						<body>
							<p>Không tìm thấy Jira Component tương ứng với Product <b>{productNameNotMap}</b> trên Bitrix </p>
						</body>
					</html>";

			await PushNotify.SendEmailSendGrid(subjectMail, htmlContent, addressEmailCreateDeal, isSendEmailToAdminBitrixJira);

		}

		//private async Task createComponent(Jira objJira, List<bitrixData.productData> lstProduct)
		//{
		//	try
		//	{
		//		Issue objComponents = objJira.CreateIssue(ConfigJiraBitrix.IssueProject);

		//		foreach (var citem in lstProduct)
		//		{
		//			var objs = objJira.Components.GetComponentsAsync(ConfigJiraBitrix.IssueProject).Result.Where(a => a.Name == citem.PRODUCT_NAME.Trim()).FirstOrDefault();

		//			if (objs == null)
		//			{
		//				ProjectComponentCreationInfo tmpPC = new ProjectComponentCreationInfo(citem.PRODUCT_NAME.Trim());
		//				tmpPC.ProjectKey = ConfigJiraBitrix.IssueProject;
		//				tmpPC.Name = citem.PRODUCT_NAME.Trim();
		//				tmpPC.Description = citem.PRODUCT_DESCRIPTION.Trim();
		//				await objJira.Components.CreateComponentAsync(tmpPC);
		//			}
		//		}

		//	}
		//	catch (Exception ex)
		//	{

		//		throw ex;
		//	}

		//}
	}
}
