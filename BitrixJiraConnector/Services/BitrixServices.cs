using BitrixJiraConnector.Configurations;
using BitrixJiraConnector.Helpers;
using BitrixJiraConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Atlassian.Jira;

namespace BitrixJiraConnector.Services
{
	class BitrixServices
	{
		public static async Task getDeals_Bitrix(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			int dealIDLog = 0;
			try
			{
				List<int> dealIDProcessed = ConfigJiraBitrix.dealIDProcessed;

				JObject dataCustomField_Bitrix = await getValueFiledCustom_Bitrix();
				string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_LIST_DEAL_DATA;

				int numDaySearchBack = ConfigJiraBitrix.soNgayQuet;
				string beginDateSearch = DateTime.Now.AddDays(numDaySearchBack * (-1)).ToString("yyyy/MM/dd");
				string[] arrayDealSearch = new string[] { "S", "F" };
				var data = new
				{
					order = new Dictionary<string, object>()
					{
						["CLOSEDATE"] = "DESC",
					},
					filter = new Dictionary<string, object>()
					{
						["STAGE_SEMANTIC_ID"] = arrayDealSearch, // S ~ Won deal
						// [">=DATE_MODIFY"] = beginDateSearch, // Đổi từ CLOSEDATE thành DATE_MODIFY do có deal từ năm ngoái được chỉnh lại thông tin nhưng CLOSEDATE không đổi
						["ID"] = 48407,
					},
				};


				string responseBody = await sendApiBitrix_POST(url, data);
				dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(responseBody);

				string totalItem = responseConvert.total;
				JToken responseResult = responseConvert.result;
				dealIDProcessed = new List<int>();
				if (responseResult != null && responseResult.Any())
				{
					foreach (dynamic item in responseResult)
					{
						int idDeal = item.ID;
						dealIDLog = item.ID;
						if (!dealIDProcessed.Contains(idDeal))
						{
							await getDataDealBitrix_And_CreateIssues(idDeal, dataCustomField_Bitrix);
						}
					}
				}
			}
			catch (Exception ex)
			{
				JiraServices jiraServices = new JiraServices();
				await jiraServices.SaveExceptionLog(ex, dealIDLog.ToString());
				throw ex;
			}
		}
		public static async Task getDataDealBitrix_And_CreateIssues(int dealID, JObject dataCustomField_Bitrix)
		{
			

			string beginDateSearch = DateTime.Now.ToString("yyyy/MM/dd");
		
			// Lấy thông tin chi tiết của 1 Deal
			BitrixDataDealAPI_Result bitrixDataDeal = await getDataDealByID_Bitrix(dealID, dataCustomField_Bitrix);
			if (bitrixDataDeal.HaveGetLate)
			{
				return;
			} 
			else if (bitrixDataDeal.HaveError)
			{
				string subjectMail = ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Error + " - DealID: " + dealID.ToString();
				BitrixJiraInfo itemCheckResendEmail = await BitrixJiraDBServices.GetDealByDealID(dealID);
				if (itemCheckResendEmail == null)
				{
					await PushNotify.SendEmailSendGrid(subjectMail, bitrixDataDeal.Message, bitrixDataDeal.ToAddressEmail);

					BitrixDataDeal dataDeal = bitrixDataDeal.DataDeal;
					BitrixJiraInfo bitrixJiraDB = new BitrixJiraInfo();
					bitrixJiraDB.Bitrix_DealID = dealID;
					bitrixJiraDB.Bitrix_DealLink = dataDeal.LinkCRM;
					bitrixJiraDB.Bitrix_DateSearch = beginDateSearch;
					bitrixJiraDB.IsSendDataToJira = 0;
					bitrixJiraDB.IsSendEmail = 1;
					bitrixJiraDB.Jira_Link = "";
					bitrixJiraDB.HaveError = 1;
					bitrixJiraDB.ErrorInfo = bitrixDataDeal.Message;
					bitrixJiraDB.DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
					bitrixJiraDB.NumberCheckError = 0;
					bitrixJiraDB.DateTimeSendMailFirst = DateTimeOffset.Now.ToUnixTimeSeconds();
					bitrixJiraDB.LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
					await BitrixJiraDBServices.InsertData(bitrixJiraDB);
				} 
				else
				{
					CheckSendEmail checkSendEmail = await CheckSendEmail(itemCheckResendEmail);
					if (checkSendEmail.IsSendMail)
					{
						string pipeLineDeal = bitrixDataDeal.DataDeal.Pipeline;
						bool isGetRenewalManagerMail = pipeLineDeal == TYPE_PIPE_LINE.RENEWAL.ToString() || pipeLineDeal == TYPE_PIPE_LINE.RENEWAL.ToString();

						string mailMangerBitrix = isGetRenewalManagerMail ? ConfigJiraBitrix.MailInfo_RenewalManagerEmail : ConfigJiraBitrix.MailInfo_SalesManagerEmail;
						string mailUserCreateDeal = bitrixDataDeal.ToAddressEmail;
						bool isSendEmailThird = checkSendEmail.SaveTimeSendMailTo == (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD;

						string toAddressEmail = isSendEmailThird ? mailMangerBitrix : mailUserCreateDeal;
						await PushNotify.SendEmailSendGrid(subjectMail, bitrixDataDeal.Message, toAddressEmail);
						await BitrixJiraDBServices.UpdateDateTimeSendMail(dealID, checkSendEmail.SaveTimeSendMailTo);
						if (isSendEmailThird) ConfigJiraBitrix.dealIDProcessed.Add(dealID);
					}
				}
			}
			// Deal đã tạo iss rồi hoặc loại deal không nằm trong danh sách tạo iss
			else if (bitrixDataDeal.HaveCreateIssues)
			{
				BitrixDataDeal dataDeal = bitrixDataDeal.DataDeal;
				BitrixJiraInfo bitrixJiraDB = new BitrixJiraInfo();

				bitrixJiraDB.Bitrix_DealID = dealID;
				bitrixJiraDB.Bitrix_DealLink = dataDeal.LinkCRM;
				bitrixJiraDB.Bitrix_DateSearch = beginDateSearch;
				bitrixJiraDB.IsSendDataToJira = 0;
				bitrixJiraDB.IsSendEmail = 0;
				bitrixJiraDB.Jira_Link = "";
				bitrixJiraDB.HaveError = 1;
				bitrixJiraDB.ErrorInfo = bitrixDataDeal.Message;
				bitrixJiraDB.DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				bitrixJiraDB.NumberCheckError = 0;
				bitrixJiraDB.LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

				await BitrixJiraDBServices.InsertData(bitrixJiraDB);
				ConfigJiraBitrix.dealIDProcessed.Add(dealID);
			}
			else
			{
				// Tạo issues trên Jira
				JiraServices jiraServices = new JiraServices();
				Issue issueCreated = await jiraServices.CreateIssJira(bitrixDataDeal.DataDeal);

				if (issueCreated.Key != null)
				{
					await postDeadDataAsync(bitrixDataDeal.DataDeal, issueCreated.Key.ToString());

					string urlDealBitrix = bitrixDataDeal.DataDeal.LinkCRM;
					// string urlIssuesCreated = ConfigJiraBitrix.jiraUrl + issueCreated.Key.ToString();
					string urlIssuesCreated = ConfigJiraBitrix.jiraUrl + "/browse/" + issueCreated.Key.ToString();

					var htmlContent = @$"
					<html>
						<body>
							<p>Issue đã được tạo thành công từ Bitrix</p>
							<p>Link Deal: <a href='{urlDealBitrix}'>{urlDealBitrix}</a></p>
							<p>Link Issue: <a href='{urlIssuesCreated}'>{urlIssuesCreated}</a></p>
						</body>
					</html>";

					string bodyMail = htmlContent;
					await PushNotify.SendEmailSendGrid(ConfigJiraBitrix.MailInfo_Subject_Create_Iss_Success + " - DealID: " + dealID.ToString(), bodyMail, bitrixDataDeal.DataDeal.Responsible_Email);

					BitrixJiraInfo itemDealID = await BitrixJiraDBServices.GetDealByDealID(dealID);
					bool haveInsertData = itemDealID != null;
					if (haveInsertData)
					{
						await BitrixJiraDBServices.SetBitrixCreateIssSuccess(dealID, urlIssuesCreated);
						ConfigJiraBitrix.dealIDProcessed.Add(dealID);
					}
					else
					{
						BitrixJiraInfo bitrixJiraDB = new BitrixJiraInfo();
						bitrixJiraDB.Bitrix_DealID = dealID;
						bitrixJiraDB.Bitrix_DealLink = urlDealBitrix;
						bitrixJiraDB.Bitrix_DateSearch = beginDateSearch;
						bitrixJiraDB.IsSendDataToJira = 1;
						bitrixJiraDB.IsSendEmail = 1;
						bitrixJiraDB.Jira_Link = urlIssuesCreated;
						bitrixJiraDB.HaveError = 0;
						bitrixJiraDB.ErrorInfo = "";
						bitrixJiraDB.DateTimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						bitrixJiraDB.NumberCheckError = 0;
						bitrixJiraDB.LastChangeData = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

						await BitrixJiraDBServices.InsertData(bitrixJiraDB);
					}
					ConfigJiraBitrix.dealIDProcessed.Add(dealID);
				}
			}
			
		}
		private static async Task postDeadDataAsync(BitrixDataDeal deal, string jiraKey)
		{
			try
			{
				string baseUrl = ConfigJiraBitrix.bitrixAPI + "/crm.deal.update";
				string jiraUrl = ConfigJiraBitrix.jiraUrl + "/browse/" + jiraKey;
				var data = new
				{
					id = deal.DealID,
					fields = new Dictionary<string, object>()
					{
						["UF_CRM_1616066175"] = jiraKey,
						["UF_CRM_1616066206"] = jiraUrl,
					},
				};
				await sendApiBitrix_POST(baseUrl, data);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public static async Task<JObject> getValueFiledCustom_Bitrix()
		{
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_DEAL_FIELD;
			string response_dataFields = await sendApiBitrix_GET(url);
			JObject result = JObject.Parse(response_dataFields);
			return result;
		}
		public static async Task<JObject> getProductList_Bitrix()
		{
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_LIST_PRODUCT_DATA;
			string response_dataFields = await sendApiBitrix_GET(url);
			JObject result = JObject.Parse(response_dataFields);
			return result;
		}
		public static async Task<JObject> getProductByID_Bitrix(string idProduct)
		{
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_PRODUCT_BY_ID_DATA + idProduct;
			string response_dataFields = await sendApiBitrix_GET(url);
			JObject result = JObject.Parse(response_dataFields);
			return result;
		}
		public static async Task<JObject> getProductSectionList_Bitrix()
		{
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_LIST_SECTION_DATA;
			string response_dataFields = await sendApiBitrix_GET(url);
			JObject result = JObject.Parse(response_dataFields);
			return result;
		}
		public static async Task<BitrixDataDealAPI_Result> getDataDealByID_Bitrix(int dealID, JObject DataCustomFiled)
		{
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_DEAL_DATA + dealID.ToString();
			string response_dataDeals = await sendApiBitrix_GET(url);

			dynamic responseResult = JsonConvert.DeserializeObject<dynamic>(response_dataDeals);
			dynamic dynJson = responseResult.result;

			BitrixDataDealAPI_Result bitrixDataDeal = await CreateDataDealFromDataApi_BiTrix(dynJson, DataCustomFiled);
			return bitrixDataDeal;
		}
		public static async Task<UserBitrix> getResponsible_UserById_Bitrix(string userID)
		{
			UserBitrix userBitrix = new UserBitrix();
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_USER_DATA + userID;
			string responseUser = await sendApiBitrix_GET(url);

			dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(responseUser);
			JToken resonseResult = responseConvert.result;

			if (resonseResult != null && resonseResult.Any())
			{
				dynamic userFirst = resonseResult.FirstOrDefault();
				userBitrix.FirstName = userFirst.NAME;
				userBitrix.LastName = userFirst.LAST_NAME;
				userBitrix.Email = userFirst.EMAIL;
			}
			return userBitrix;
		}
		public static async Task<ContactBitrix> getContact_LaThongTinLienHeKhiTrienKhai_Bitrix(string dealID)
		{
			ContactBitrix contactBitrix = new ContactBitrix();
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_LIST_CONTACT_OF_DEAL + dealID;
			string responseContact = await sendApiBitrix_GET(url);

			dynamic responseBodyConvert = JsonConvert.DeserializeObject<dynamic>(responseContact);
			JToken responseResult = responseBodyConvert.result;
			if (responseResult == null && responseResult.Any()) return contactBitrix;

			foreach (dynamic item in responseResult)
			{
				string contactID = item.CONTACT_ID;
				ContactInfor_Result contactInfor_Result = await getClient_ContactById_Bitrix(contactID);
				if (contactInfor_Result.LaThongTinLienHeKhiTrienKhai)
				{
					contactBitrix = contactInfor_Result.ContactBitrix;
					return contactBitrix;
				}
			}
			return contactBitrix;
		}
		public static async Task<ContactInfor_Result> getClient_ContactById_Bitrix(string contactID)
		{
			ContactInfor_Result contactInfor_Result = new ContactInfor_Result();
			contactInfor_Result.LaThongTinLienHeKhiTrienKhai = false;
			string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_CONTACT_DATA + contactID;
			string responseUser = await sendApiBitrix_GET(url);

			dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(responseUser);
			dynamic resonseResult = responseConvert.result;

			if (resonseResult != null)
			{
				bool laThongTinLienHeKhiTrienKhai = resonseResult.UF_CRM_1715045713.ToString() == "1";
				
				if (laThongTinLienHeKhiTrienKhai)
				{
					contactInfor_Result.LaThongTinLienHeKhiTrienKhai = laThongTinLienHeKhiTrienKhai;
					// dynamic userFirst = resonseResult.FirstOrDefault();

					ContactBitrix contactBitrix = new ContactBitrix();
					contactBitrix.LastName = resonseResult.LAST_NAME;
					contactBitrix.Name = resonseResult.NAME;
					contactBitrix.Position = resonseResult.POST;
					JToken phones = resonseResult.PHONE;
					if (phones != null && phones.Any())
					{
						dynamic phoneFirst = phones.FirstOrDefault();
						contactBitrix.Phone = phoneFirst.VALUE;
					}
					JToken emails = resonseResult.EMAIL;
					if (emails != null && emails.Any())
					{
						dynamic emailFirst = emails.FirstOrDefault();
						contactBitrix.Email = emailFirst.VALUE;
					}
					contactInfor_Result.ContactBitrix = contactBitrix;
					return contactInfor_Result;
				} 
				else
				{
					return contactInfor_Result;
				}
				
			}
			return contactInfor_Result;
		}
		private static async Task<List<DataProductInDeal>> getProductInDeal_Bitrix(string dealId)
		{
			List<DataProductInDeal> result = new List<DataProductInDeal>();
			try
			{
				string url = ConfigJiraBitrix.bitrixAPI + ApiBitrix.API_GET_PRODUCT_DATA + dealId;
				string responseProduct = await sendApiBitrix_GET(url);

				dynamic responseBodyConvert = JsonConvert.DeserializeObject<dynamic>(responseProduct);
				JToken responseResult = responseBodyConvert.result;
				if (responseResult == null && responseResult.Any()) return result;

				foreach (dynamic itemResult in responseResult)
				{
					DataProductInDeal objProduct = new DataProductInDeal();
					objProduct.ID = itemResult.ID;
					objProduct.OWNER_ID = itemResult.OWNER_ID;
					objProduct.OWNER_TYPE = itemResult.OWNER_TYPE;
					objProduct.PRODUCT_ID = itemResult.PRODUCT_ID;
					objProduct.PRODUCT_NAME = itemResult.PRODUCT_NAME;
					objProduct.ORIGINAL_PRODUCT_NAME = itemResult.ORIGINAL_PRODUCT_NAME;
					objProduct.PRODUCT_DESCRIPTION = itemResult.PRODUCT_DESCRIPTION;
					objProduct.PRICE = itemResult.PRICE;
					objProduct.PRICE_EXCLUSIVE = itemResult.PRICE_EXCLUSIVE;
					objProduct.PRICE_NETTO = itemResult.PRICE_NETTO;
					objProduct.PRICE_BRUTTO = itemResult.PRICE_BRUTTO;
					objProduct.PRICE_ACCOUNT = itemResult.PRICE_ACCOUNT;
					objProduct.QUANTITY = itemResult.QUANTITY;
					objProduct.DISCOUNT_TYPE_ID = itemResult.DISCOUNT_TYPE_ID;
					objProduct.DISCOUNT_RATE = itemResult.DISCOUNT_RATE;
					objProduct.DISCOUNT_SUM = itemResult.DISCOUNT_SUM;
					objProduct.TAX_RATE = itemResult.TAX_RATE == null ? 0 : itemResult.TAX_RATE;
					objProduct.TAX_INCLUDED = itemResult.TAX_INCLUDED;
					objProduct.CUSTOMIZED = itemResult.CUSTOMIZED;
					objProduct.MEASURE_CODE = itemResult.MEASURE_CODE;
					objProduct.MEASURE_NAME = itemResult.MEASURE_NAME;
					objProduct.SORT = itemResult.SORT;
					result.Add(objProduct);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return result;
		}
		public static async Task<BitrixDataDealAPI_Result> CreateDataDealFromDataApi_BiTrix(dynamic dataDealApi, JObject DataCustomFiled)
		{
			BitrixDataDeal result = new BitrixDataDeal();
			BitrixDataDealAPI_Result dataResult = new BitrixDataDealAPI_Result();
			dataResult.HaveError = false;
			dataResult.HaveCreateIssues = false;
			dataResult.HaveGetLate = false;

			if (dataDealApi.DATE_MODIFY != null)
			{
				// string dateTimeString = "2024-09-18T12:26:31+07:00";
				string dateTimeString = dataDealApi.DATE_MODIFY;
				DateTime dateTimeModify = DateTime.Parse(dateTimeString);
				DateTime now = DateTime.Now;
				DateTime dateTimeModifyPlusThreeMinutes = dateTimeModify.AddMinutes(3);
				// Trường hợp vừa sửa issue sẽ bỏ qua và quét lại sau 3 phút
				// Do gặp lỗi vừa upload file lên Deal mà quét luôn thì sẽ không tải được file về để đẩy lên Jira
				if (dateTimeModifyPlusThreeMinutes >= now)
				{
					dataResult.HaveGetLate = true;
					return dataResult;
				}
			}

			string urlDeal = $"https://work.ezcloudhotel.com/crm/deal/details/{dataDealApi.ID}/";
			result.LoaiDeal = dataDealApi.UF_CRM_1713881390;
			result.LinkCRM = urlDeal;
			result.Pipeline = dataDealApi.CATEGORY_ID;
			bool isLostDeal = dataDealApi.STAGE_SEMANTIC_ID == "F";
			if (isLostDeal && result.LoaiDeal != ConfigJiraBitrix.LoaiDeal_NgungHuyDichVu)
			{
				dataResult.HaveCreateIssues = true;
				dataResult.Message = "Lost deal không nằm trong danh mục tạo iss";
				dataResult.DataDeal = result;
				return dataResult;
			}
			// Nếu đã tạo iss rồi thì bỏ qua 
			// UF_CRM_1616066206 => trường Jira url
			bool isCreateIssues = dataDealApi.UF_CRM_1616066206 != null && dataDealApi.UF_CRM_1616066206 != "";
			if (isCreateIssues)
			{
				dataResult.HaveCreateIssues = true;
				dataResult.Message = "Deal đã tạo iss";
				dataResult.DataDeal = result;
				return dataResult;
			}


			//Nếu chưa chọn Loại Deal thì không tạo iss
			if (string.IsNullOrEmpty(result.LoaiDeal))
			{
				//StringBuilder sb = new StringBuilder();
				//sb.AppendLine();
				//sb.AppendLine("Không tạo được iss do không chọn Loại Deal.");
				//sb.AppendLine($"Link Deal: {urlDeal}");

				var htmlContent = @$"
					<html>
						<body>
							<p>Không tạo được iss do không chọn Loại Deal.</p>
							<p>Link Deal: <a href='{urlDeal}'>{urlDeal}</a></p>
						</body>
					</html>";


				dataResult.Message = htmlContent;
				dataResult.HaveError = true;
				dataResult.DataDeal = result;
				return dataResult;
			}

			
			// Kiểm tra trường bắt buộc của từng loại deal
			List<string> dataFieldNull = new List<string>();
			bool dealNotNeedCreateIss = false;
			switch (result.LoaiDeal)
			{
				case ConfigJiraBitrix.LoaiDeal_TrienKhaiMoi:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_TrienKhaiMoi);
					break;
				case ConfigJiraBitrix.LoaiDeal_TrienKhaiBoSung:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_TrienKhaiBoSung);
					break;
				case ConfigJiraBitrix.LoaiDeal_NgungHuyDichVu:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_NgungHuyDichVu);
					break;
				case ConfigJiraBitrix.LoaiDeal_ChuyenDoiDichVu:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_ChuyenDoiDichVu);
					break;
				case ConfigJiraBitrix.LoaiDeal_CapKey:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_CapKey);
					break;
				case ConfigJiraBitrix.LoaiDeal_HoTroKhac:
					dataFieldNull = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, ConfigJiraBitrix.fieldRequire_HoTroKhac);
					break;
				default:
					dealNotNeedCreateIss = true;
					break;
			}
			// get user info Assigned in Deal, khi cấu hình Inbound Webhook trên Bitrix, mục Access Permissions cần tích chọn Users 
			result.Responsible_UserID = dataDealApi.ASSIGNED_BY_ID;
			UserBitrix dataUser_Responsible = await getResponsible_UserById_Bitrix(result.Responsible_UserID);
			result.Responsible_Email = dataUser_Responsible.Email;
			result.Responsible_FirstName = dataUser_Responsible.FirstName;
			result.Responsible_LastName = dataUser_Responsible.LastName;

			//{ "THONTINLIENHE_TENKHACH", "Tên khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
			//{ "THONTINLIENHE_CHUCVUKHACHHANG", "Chức vụ khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
			//{ "THONTINLIENHE_SDT", "Số điện thoại khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
			//{ "THONTINLIENHE_EMAIL", "Email khách hàng tích chọn Là thông tin liên hệ khi triển khai/hỗ trợ" },
			string dealIDPost = dataDealApi.ID;
			ContactBitrix dataContactBitrix = await getContact_LaThongTinLienHeKhiTrienKhai_Bitrix(dealIDPost);
			result.Client_Contact_LastName_Name = $"{dataContactBitrix.LastName} {dataContactBitrix.Name}";
			result.Client_Contact_Position = dataContactBitrix.Position;
			result.Client_Contact_Phone = dataContactBitrix.Phone;
			result.Client_Contact_Email = dataContactBitrix.Email;

			if (string.IsNullOrEmpty(result.Client_Contact_LastName_Name) || string.IsNullOrWhiteSpace(result.Client_Contact_LastName_Name))
			{
				dataFieldNull.Add("THONTINLIENHE_TENKHACH");
			}
			if (string.IsNullOrEmpty(result.Client_Contact_Phone) || string.IsNullOrWhiteSpace(result.Client_Contact_Phone))
			{
				dataFieldNull.Add("THONTINLIENHE_SDT");
			}
			if (string.IsNullOrEmpty(result.Client_Contact_Email) || string.IsNullOrWhiteSpace(result.Client_Contact_Email))
			{
				dataFieldNull.Add("THONTINLIENHE_EMAIL");
			}


			if (dealNotNeedCreateIss)
			{
				dataResult.HaveCreateIssues = true;
				dataResult.Message = "Deal Không nằm trong loại tạo iss";
				dataResult.DataDeal = result;
				return dataResult;
			}

			if (dataFieldNull.Any())
			{
				List<string> nameFieldNull = CheckRequireFieldBitrix.GetValuesForKeys(ConfigJiraBitrix.keyValueField_Bitrix, dataFieldNull);
				string strNameFieldNull = string.Join(", ", nameFieldNull);

				//StringBuilder sb = new StringBuilder();
				//sb.AppendLine();
				//sb.AppendLine($"Không tạo được iss do không nhập trường bắt buộc: {strNameFieldNull}");
				//sb.AppendLine($"Link Deal: {urlDeal}");

				var htmlContent = @$"
					<html>
						<body>
							<p>Không tạo được iss do không nhập trường bắt buộc: {strNameFieldNull}</p>
							<p>Link Deal: <a href='{urlDeal}'>{urlDeal}</a></p>
						</body>
					</html>";

				dataResult.Message = htmlContent;
				dataResult.HaveError = true;
				dataResult.ToAddressEmail = result.Responsible_Email;
				dataResult.DataDeal = result;
				return dataResult;
			}
			// Map thông tin field API với data cần lấy
			JToken DanhSachNhuCauSanPham = dataDealApi.UF_CRM_5D98084D1476E;
			if (DanhSachNhuCauSanPham != null && DanhSachNhuCauSanPham.Any())
			{
				List<string> TenNhuCauSanPham = new List<string>();
				foreach (var item in DanhSachNhuCauSanPham)
				{
					string valueItem = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_5D98084D1476E", item.ToString());
					TenNhuCauSanPham.Add(valueItem);
				}
				result.NhuCauSanPham += string.Join(",", TenNhuCauSanPham);
			}
			result.TenKhachSan = dataDealApi.UF_CRM_5B3F32B1118E0;


			result.ThiTruong = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1708697569", dataDealApi.UF_CRM_1708697569);
			result.LoaiHinhKhachSan = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1616123841", dataDealApi.UF_CRM_1616123841);
			result.Source = dataDealApi.SOURCE_ID;
			result.CompanyId = dataDealApi.COMPANY_ID;
			// result.LinkCRM = urlDeal;
			result.DealID = dataDealApi.ID;
			result.DC_Khach_SoNhaDuong = dataDealApi.UF_CRM_5D984E2CA62C9;
			result.DC_Khach_TinhThanhPho = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_5D984E38B7502", dataDealApi.UF_CRM_5D984E38B7502);
			result.DC_Khach_QuanHuyen = dataDealApi.UF_CRM_5D984E34ACFB6;
			result.DC_Khach_PhuongXa = dataDealApi.UF_CRM_5D984E30EEE34;
			result.SoPhong = dataDealApi.UF_CRM_5B3F32B1068C7;
			result.SoGiuong = dataDealApi.UF_CRM_6054492BF24D8;

			result.Client_Contact_ContactID = dataDealApi.CONTACT_ID;
			// ContactBitrix dataContactBitrix = await getClient_ContactById_Bitrix(result.Client_Contact_ContactID);
			

			result.ChinhSachGia = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1708697773", dataDealApi.UF_CRM_1708697773);
			result.DanhSachSanPham = await getProductInDeal_Bitrix(result.DealID);
			bool haveGetSectionProduct = result.LoaiDeal == ConfigJiraBitrix.LoaiDeal_CapKey && result.DanhSachSanPham != null && result.DanhSachSanPham.Any();
			if (haveGetSectionProduct)
			{
				// Đóng do api này chỉ trả ra 50 sản phẩm 1 lần => Chuyển sang gọi api lấy sản phẩm theo ID (sẽ bị chậm)
				// JObject dataListProduct_Bitrix = await getProductList_Bitrix();
				JObject dataListSectionProduct_Bitrix = await getProductSectionList_Bitrix();
				foreach (DataProductInDeal item in result.DanhSachSanPham)
				{
					try
					{
						JObject dataProduct = await getProductByID_Bitrix(item.PRODUCT_ID.ToString());
						string sectionIDOfProduct = BitrixConvertData.GetValueFromObjectJson(dataProduct, "SECTION_ID", item.PRODUCT_ID);
						bool haveSectionProductID = !string.IsNullOrEmpty(sectionIDOfProduct);
						if (haveSectionProductID)
						{
							string sectionNameOfProduct = BitrixConvertData.GetValueFromArrayJson(dataListSectionProduct_Bitrix, "NAME", sectionIDOfProduct);
							item.PRODUCT_NAME = sectionNameOfProduct;
						}
					}
					catch (Exception ex)
					{
						// Không lấy được thông tin section của sản phẩm thì bỏ qua
						continue;
					}
				}
			}
			result.MayChuChayPhanMem = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1708698013", dataDealApi.UF_CRM_1708698013);
			result.DungThuHayDungThat = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1715996452", dataDealApi.UF_CRM_1715996452);
			result.TrangThaiThanhToanLan1 = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1616123465", dataDealApi.UF_CRM_1616123465);
			result.NoiDungChuyenKhoanLan1 = dataDealApi.UF_CRM_1614068621890;
			result.PhuongThucTrienKhai = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1616130516", dataDealApi.UF_CRM_1616130516);
			// result.ThoiDiemTrienKhai = dataDealApi.UF_CRM_1616130611;
			result.ThoiDiemTrienKhai = string.IsNullOrEmpty(dataDealApi.UF_CRM_1616130611.ToString()) ? null : dataDealApi.UF_CRM_1616130611;
			result.TaikhoanDungThuPMS = dataDealApi.UF_CRM_1616130767;
			result.Neu_TK_BE_CoTichHop_ezFolioKhong = dataDealApi.UF_CRM_1616130823 == "1" ? "Có" : "Không";
			result.ThongTinTrienKhaiWeb = dataDealApi.UF_CRM_1616130900;
			if (dataDealApi.UF_CRM_1531008138 != null)
			{
				JToken note = dataDealApi.UF_CRM_1531008138;
				result.GhiChu = note != null && note.Any() ? note[0].ToString() : "";
			}
			result.MaKhachSan = dataDealApi.UF_CRM_1613727861184;
			result.YeuCauThem = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1708698698", dataDealApi.UF_CRM_1708698698);
			if (dataDealApi.UF_CRM_1570243307 != null && dataDealApi.UF_CRM_1570243307.ToString() != "False")
			{
				List<string> lyDoLosts = new List<string>();
				foreach (var item in dataDealApi.UF_CRM_1570243307)
				{
					string itemLyDoLost = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1570243307", item);
					lyDoLosts.Add(itemLyDoLost);
				}
				result.LyDoLost = lyDoLosts;
			}
			result.GhichuChoLyDoLost = dataDealApi.UF_CRM_1570243354;
			result.LoaiYeuCauHuy = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1708699318", dataDealApi.UF_CRM_1708699318);
			result.ThoiDiemNgungHuy = string.IsNullOrEmpty(dataDealApi.UF_CRM_1715997025.ToString()) ? null : dataDealApi.UF_CRM_1715997025;
			result.TenDeal = dataDealApi.TITLE;
			result.NgayBatDauTinh_GHBT = string.IsNullOrEmpty(dataDealApi.UF_CRM_1600922068.ToString()) ? null : dataDealApi.UF_CRM_1600922068;
			result.NgayKetThucHan_GHBT = string.IsNullOrEmpty(dataDealApi.UF_CRM_1600922103.ToString()) ? null : dataDealApi.UF_CRM_1600922103;
			result.TinhHuongCanCapKey = BitrixConvertData.GetValueItemInArrayFromJson(DataCustomFiled, "UF_CRM_1715997550", dataDealApi.UF_CRM_1715997550);
			// Lấy thông tin file hợp đồng Attach
			JToken fileAttach = dataDealApi.UF_CRM_1613788692724;
			// Kiểm tra xem có file nào download về bị lỗi không
			bool haveFileDownloadError = false;
			if (fileAttach != null && fileAttach.Any())
			{
				List<FileHopDongAttach> lstFile = new List<FileHopDongAttach>();
				foreach (var itemfile in fileAttach)
				{
					FileHopDongAttach objFile = new FileHopDongAttach();
					JObject objTmp = (JObject)itemfile;
					foreach (var subitemfile in objTmp)
					{
						if (subitemfile.Key == "id") objFile.id = subitemfile.Value.ToString();
						if (subitemfile.Key == "showUrl") objFile.showUrl = subitemfile.Value.ToString();
						if (subitemfile.Key == "downloadUrl")
						{
							objFile.downloadUrl = subitemfile.Value.ToString();
							string fileName = "";
							List<string> fileBitrixs = await objFile.downloadFile(result.DealID, objFile.downloadUrl);
							objFile.path_file = fileBitrixs[1];
							objFile.file_Name = fileBitrixs[0];
							if (!haveFileDownloadError)
							{
								haveFileDownloadError = objFile.path_file == ConfigJiraBitrix.folderAttachFile;
							}
						}
					}
					lstFile.Add(objFile);
				}
				result.HopDong_PhuLuc = lstFile;
			}
			//if (haveFileDownloadError)
			//{
			//	dataResult.HaveGetLate = true;
			//	return dataResult;
			//}
			dataResult.DataDeal = result;
			return dataResult;
		}

		public static async Task<CheckSendEmail> CheckSendEmail(BitrixJiraInfo infoDeal)
		{
			CheckSendEmail result = new CheckSendEmail();
			result.IsSendMail = false;
			result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.NO_SAVE;

			// Nếu chưa lưu thời gian gửi mail lần đầu thì gửi mail + lưu lại thông tin thời gian gửi mail lần đầu
			if (infoDeal.DateTimeSendMailFirst == null) 
			{
				result.IsSendMail = true;
				result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_FIRST;
				return result;
			} 

			// Nếu có thông tin thời gian gửi mail lần 3 thì ko gửi mail nữa
			if (infoDeal.DateTimeSendMailThird != null) return result;

			DateTime dateTimeSendMailFirst = DateTimeOffset.FromUnixTimeSeconds(infoDeal.DateTimeSendMailFirst ?? 0).LocalDateTime;
			DateTime dateAt17h = new DateTime(dateTimeSendMailFirst.Year, dateTimeSendMailFirst.Month, dateTimeSendMailFirst.Day, 17, 0, 0);


			DateTime now = DateTime.Now;
			DateTime dateTimeSendMailFirstNextDay = dateTimeSendMailFirst.AddDays(1);
			DateTime nineAMFirstNextDay = new DateTime(dateTimeSendMailFirstNextDay.Year, dateTimeSendMailFirstNextDay.Month, dateTimeSendMailFirstNextDay.Day, 9, 0, 0);

			// Nếu Lỗi lần 1 gửi trước 17h và hiện tại sau 17h hoặc lỗi lần 1 gửi sau 17h và hiện tại qua 9h sáng ngày hôm sau thì gửi mail lần 2
			bool isSendMailSecond = (dateTimeSendMailFirst < dateAt17h && dateAt17h < now) || (dateTimeSendMailFirst > dateAt17h && now > nineAMFirstNextDay);
			if (infoDeal.DateTimeSendMailSecond == null && isSendMailSecond)
			{
				result.IsSendMail = true;
				result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_SECOND;
				return result;
			}

			// Nếu đã gửi lỗi lần 2 thì 9h sáng ngày hôm sau gửi lại
			DateTime dateTimeSendMailSecond = DateTimeOffset.FromUnixTimeSeconds(infoDeal.DateTimeSendMailSecond ?? 0).LocalDateTime;
			DateTime dateTimeSendMailSecondNextDay = dateTimeSendMailSecond.AddDays(1);
			DateTime nineSecondNextDay = new DateTime(dateTimeSendMailSecondNextDay.Year, dateTimeSendMailSecondNextDay.Month, dateTimeSendMailSecondNextDay.Day, 9, 0, 0);

			if (infoDeal.DateTimeSendMailThird == null && infoDeal.DateTimeSendMailSecond != null && now > nineSecondNextDay)
			{
				result.IsSendMail = true;
				result.SaveTimeSendMailTo = (int)SAVE_TIME_SEND_MAIL_TO.TIME_SEND_MAIL_THIRD;
				return result;
			}
			return result;
		}

		#region Call API to Bitrix
		public static async Task<string> sendApiBitrix_POST(string url, dynamic data)
		{
			HttpClient client = new HttpClient();
			var contentText = JsonConvert.SerializeObject(data);
			var content = new StringContent(contentText, Encoding.UTF8, "application/json");
			HttpResponseMessage response = client.PostAsync(url, content).Result;

			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return responseBody;
		}
		public static async Task<string> sendApiBitrix_GET(string url)
		{
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return responseBody;
		}
		#endregion
	}
}
