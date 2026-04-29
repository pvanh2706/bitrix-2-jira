using Atlassian.Jira;
using BitrixJiraConnector.Configurations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BitrixJiraConnector.Models
{
	class BitrixDataDeal
	{
		// Property. Tham khao link https://training.bitrix24.com/rest_help/crm/deals/crm_deal_fields.php
		public string LoaiDeal { get; set; }
		public string NhuCauSanPham { get; set; }
		public string TenKhachSan { get; set; }
		public string Responsible_UserID { get; set; }
		public string Responsible_Email { get; set; }
		public string Responsible_FirstName { get; set; }
		public string Responsible_LastName { get; set; }
		public string Pipeline { get; set; }
		public string ThiTruong { get; set; }
		public string LoaiHinhKhachSan { get; set; }
		public string Source { get; set; }
		public string CompanyId { get; set; }
		public string LinkCRM { get; set; }
		public string DealID { get; set; }
		public string DC_Khach_SoNhaDuong { get; set; }
		public string DC_Khach_TinhThanhPho { get; set; }
		public string DC_Khach_QuanHuyen { get; set; }
		public string DC_Khach_PhuongXa { get; set; }
		public string SoPhong { get; set; }
		public string SoGiuong { get; set; }
		public string Client_Contact_ContactID { get; set; }
		public string Client_Contact_LastName_Name { get; set; }
		public string Client_Contact_Position { get; set; }
		public string Client_Contact_Phone { get; set; }
		public string Client_Contact_Email { get; set; }
		public string ChinhSachGia { get; set; }
		public List<DataProductInDeal> DanhSachSanPham { get; set; }
		public string MayChuChayPhanMem { get; set; }
		public string DungThuHayDungThat { get; set; }
		// public string FileHopDongAttach { get; set; } 
		public string TrangThaiThanhToanLan1 { get; set; }
		public string NoiDungChuyenKhoanLan1 { get; set; }
		public string PhuongThucTrienKhai { get; set; }
		public DateTime? ThoiDiemTrienKhai { get; set; }
		public string TaikhoanDungThuPMS { get; set; }
		public string Neu_TK_BE_CoTichHop_ezFolioKhong { get; set; }
		public string ThongTinTrienKhaiWeb { get; set; }
		public string GhiChu { get; set; }
		public string MaKhachSan { get; set; }
		public string YeuCauThem { get; set; }
		public List<string> LyDoLost { get; set; }
		public string GhichuChoLyDoLost { get; set; }
		public string LoaiYeuCauHuy { get; set; }
		public DateTime? ThoiDiemNgungHuy { get; set; }
		public string TenDeal { get; set; }
		public DateTime? NgayBatDauTinh_GHBT { get; set; }
		public DateTime? NgayKetThucHan_GHBT { get; set; }
		public string TinhHuongCanCapKey { get; set; }
		public List<FileHopDongAttach> HopDong_PhuLuc { get; set; }
	}
	class UserBitrix
	{
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
	class ContactBitrix
	{
		public string LastName { get; set; }
		public string Name { get; set; }
		public string Position { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
	}

	class FileHopDongAttach
	{
		public string id { get; set; }
		public string showUrl { get; set; }
		public string downloadUrl { get; set; }
		public string path_file { get; set; }
		public string file_Name { get; set; }
		//public string downloadFileOrigin(string dealId, string url, out string file_name)
		//{
		//	return "";
		//	string pathFile = ConfigJiraBitrix.folderAttachFile;
		//	try
		//	{

		//		string downloadUrl = ConfigJiraBitrix.bitrixUrl + url;
		//		string URI = ConfigJiraBitrix.bitrixURI;
		//		string myParameters = "AUTH_FORM=Y&TYPE=AUTH&backurl=%2Fauth%2Findex.php%3Fbackurl%3D%252Fmarketplace%252Fhook%252Fap%252F89%252F&USER_LOGIN=" + ConfigJiraBitrix.bitrixUser + "&USER_PASSWORD=" + ConfigJiraBitrix.bitrixPass;

		//		using (WebClient wc = new WebClient())
		//		{
		//			wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
		//			wc.Headers[HttpRequestHeader.Accept] = "/";
		//			wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate, br";
		//			wc.Headers[HttpRequestHeader.Cookie] = "BITRIX_SM_SALE_UID=" + ConfigJiraBitrix.bitrixBITRIX_SM_SALE_UID + "; PHPSESSID=" + ConfigJiraBitrix.bitrixPHPSESSID;
		//			string HtmlResult = wc.UploadString(URI, myParameters);
		//			var data = wc.DownloadData(downloadUrl);
		//			string fileName = "";
		//			file_name = fileName;
		//			// (01) Vanh bổ sung ghi log do thi thoảng gặp trường hợp không lấy được "Content-Disposition" để gán tên file nhưng khi debug lại lấy được
		//			string pathLog_test = ConfigJiraBitrix.folderLog + DateTime.Now.ToString("ddMMyyy") + "_LogSaveFile.txt";
		//			File.AppendAllText(pathLog_test, "-----------------  Start ------------------------" + Environment.NewLine);
		//			string content_test = dealId;
		//			File.AppendAllText(pathLog_test, content_test + Environment.NewLine);
		//			foreach (string headerKey in wc.ResponseHeaders)
		//			{
		//				string headerValue = wc.ResponseHeaders[headerKey];
		//				File.AppendAllText(pathLog_test, $"{headerKey}: {headerValue}" + Environment.NewLine);
		//			}
		//			File.AppendAllText(pathLog_test, "-----------------  End ------------------------" + Environment.NewLine);
		//			// Kết thúc bổ sung Ghi log (01)

		//			// Try to extract the filename from the Content-Disposition header
		//			if (!String.IsNullOrEmpty(wc.ResponseHeaders["Content-Disposition"]))
		//			{
		//				string startFilename = wc.ResponseHeaders["Content-Disposition"].Substring(wc.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 10);
		//				int pos = startFilename.IndexOf("\"");
		//				fileName = dealId.ToString() + "_" + startFilename.Substring(0, pos);
		//				file_name = fileName;
		//				pathFile = pathFile + fileName;
		//				//save file
		//				File.WriteAllBytes(pathFile, data);
		//			}
		//			else
		//			{
		//				string pathLog = ConfigJiraBitrix.folderLog + DateTime.Now.ToString("ddMMyyy") + "_LogSaveFile.txt";
		//				File.AppendAllText(pathLog, "-----------------  Start ------------------------" + Environment.NewLine);
		//				string content = dealId;
		//				File.AppendAllText(pathLog, content + Environment.NewLine);
		//				var contentDisposition = wc.ResponseHeaders["Content-Disposition"];
		//				File.AppendAllText(pathLog, "Content-Disposition:" + Environment.NewLine);
		//				File.AppendAllText(pathLog, contentDisposition + Environment.NewLine);
		//				File.AppendAllText(pathLog, "-----------------  End ------------------------" + Environment.NewLine);
		//			}

		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		// clsLog.Write("Error: class fileAttach =>downloadFile():" + ex.Message);
		//		string pathLog = ConfigJiraBitrix.folderLog + DateTime.Now.ToString("ddMMyyy") + "_LogSaveFile_Exception.txt";
		//		File.AppendAllText(pathLog, "-----------------  Start ------------------------" + Environment.NewLine);
		//		string content = dealId;
		//		File.AppendAllText(pathLog, content + Environment.NewLine);
		//		File.AppendAllText(pathLog, "Exception downloadFile" + Environment.NewLine);
		//		File.AppendAllText(pathLog, ex.Message + Environment.NewLine);
		//		File.AppendAllText(pathLog, "-----------------  End ------------------------" + Environment.NewLine);
		//		throw ex;
		//	}

		//	return pathFile;
		//}
		public async Task<List<string>> downloadFile(string dealId, string url)
		{
			string pathFile = ConfigJiraBitrix.folderAttachFile;
			string fileName = "";
			List<string> result = new List<string>();
			try
			{
				using (HttpClient client = new HttpClient())
				{
					string downloadUrl = ConfigJiraBitrix.bitrixUrl + url;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
					string userName = "connector";
					string password = "Batdau@2023";
					string base64AuthString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
					request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthString);

					var response = await client.SendAsync(request);
					if (response.IsSuccessStatusCode)
					{
						if (response.Content.Headers.ContentDisposition != null)
						{
							string startFilename = response.Content.Headers.ContentDisposition.FileName?.Trim('"'); 
							fileName = dealId.ToString() + "_" + startFilename;
							pathFile = pathFile + fileName;
							//save file
							using (var fileStream = new FileStream(pathFile, FileMode.Create, FileAccess.Write, FileShare.None))
							{
								await (await response.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			result.Add(fileName);
			result.Add(pathFile);
			return result;
		}
	}
	class DataProductInDeal
	{
		public int ID { get; set; } // ID
		public string OWNER_ID { get; set; }
		public string OWNER_TYPE { get; set; }
		public int PRODUCT_ID { get; set; }
		public string PRODUCT_NAME { get; set; }
		public string ORIGINAL_PRODUCT_NAME { get; set; }
		public string PRODUCT_DESCRIPTION { get; set; }
		public double PRICE { get; set; }
		public double PRICE_EXCLUSIVE { get; set; }
		public double PRICE_NETTO { get; set; }
		public double PRICE_BRUTTO { get; set; }
		public double PRICE_ACCOUNT { get; set; }
		public double QUANTITY { get; set; }
		public int DISCOUNT_TYPE_ID { get; set; }
		public double DISCOUNT_RATE { get; set; }
		public double DISCOUNT_SUM { get; set; }
		public double TAX_RATE { get; set; }
		public string TAX_INCLUDED { get; set; }
		public string CUSTOMIZED { get; set; }
		public string MEASURE_CODE { get; set; }
		public string MEASURE_NAME { get; set; }
		public int SORT { get; set; }
	}
	class BitrixDataDealAPI_Result
	{
		public BitrixDataDeal DataDeal { get; set; }
		public bool HaveError { get; set; }
		public bool HaveCreateIssues { get; set; }
		public bool HaveGetLate { get; set; }
		public string Message { get; set; }
		public string ToAddressEmail { get; set; }
	}
	class ContactInfor_Result
	{
		public ContactBitrix ContactBitrix { get; set; }
		public bool LaThongTinLienHeKhiTrienKhai { get; set; }
	}
	
}
