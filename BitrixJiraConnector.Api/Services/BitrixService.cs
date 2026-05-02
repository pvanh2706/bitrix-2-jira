using System.Text;
using BitrixJiraConnector.Api.Configurations;
using BitrixJiraConnector.Api.Helpers;
using BitrixJiraConnector.Api.Models.Bitrix;
using BitrixJiraConnector.Api.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitrixJiraConnector.Api.Services;

public class BitrixService : IBitrixService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDbService _dbService;
    private readonly ILogger<BitrixService> _logger;

    private BitrixSettings _bitrixSettings = null!;
    private JiraSettings _jiraSettings = null!;

    private async Task EnsureConfigLoadedAsync()
    {
        if (_bitrixSettings is not null) return;
        var all = (await _dbService.GetAllSystemConfigsAsync())
                  .GroupBy(c => c.ConfigKey)
                  .ToDictionary(g => g.Key, g => g.First().ConfigValue);
        _bitrixSettings = new BitrixSettings
        {
            ApiUrl         = all.GetValueOrDefault("bitrix_api_url", ""),
            BaseUrl        = all.GetValueOrDefault("bitrix_base_url", ""),
            DealDetailUrl  = all.GetValueOrDefault("bitrix_deal_detail_url", ""),
            User           = all.GetValueOrDefault("bitrix_user", ""),
            Password       = all.GetValueOrDefault("bitrix_password", ""),
            AttachFilePath = all.GetValueOrDefault("bitrix_attach_file_path", ""),
        };
        _jiraSettings = new JiraSettings
        {
            Url      = all.GetValueOrDefault("jira_url", ""),
            User     = all.GetValueOrDefault("jira_user", ""),
            Password = all.GetValueOrDefault("jira_password", ""),
        };
    }

    public BitrixService(
        IHttpClientFactory httpClientFactory,
        IDbService dbService,
        ILogger<BitrixService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _dbService = dbService;
        _logger = logger;
    }

    public async Task<List<int>> GetDealIdsToProcessAsync()
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_LIST_DEAL_DATA;
        string cutoff = DateTime.Now.AddMinutes(-3).ToString("yyyy-MM-ddTHH:mm:ss");
        var data = new
        {
            order = new Dictionary<string, object> { ["CLOSEDATE"] = "DESC" },
            filter = new Dictionary<string, object>
            {
                ["STAGE_SEMANTIC_ID"] = new[] { "S", "F" },
                [">=DATE_MODIFY"] = cutoff,
            },
        };

        string responseBody = await PostAsync(url, data);
        dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(responseBody)!;
        JToken responseResult = responseConvert.result;

        var ids = new List<int>();
        if (responseResult != null && responseResult.Any())
        {
            foreach (dynamic item in responseResult)
                ids.Add((int)item.ID);
        }
        return ids;
    }

    public async Task<JObject> GetCustomFieldsAsync()
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_DEAL_FIELD;
        string response = await GetAsync(url);
        return JObject.Parse(response);
    }

    public async Task<JObject> GetProductListAsync()
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_LIST_PRODUCT_DATA;
        string response = await GetAsync(url);
        return JObject.Parse(response);
    }

    public async Task<JObject> GetProductByIdAsync(string productId)
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_PRODUCT_BY_ID_DATA + productId;
        string response = await GetAsync(url);
        return JObject.Parse(response);
    }

    public async Task<JObject> GetProductSectionListAsync()
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_LIST_SECTION_DATA;
        string response = await GetAsync(url);
        return JObject.Parse(response);
    }

    public async Task<BitrixDataDealApiResult> GetDealByIdAsync(int dealId, JObject customFields)
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_DEAL_DATA + dealId;
        string response = await GetAsync(url);
        dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(response)!;
        dynamic dynJson = responseConvert.result;
        return await BuildDealResultAsync(dynJson, customFields);
    }

    public async Task<UserBitrix> GetResponsibleUserByIdAsync(string userId)
    {
        await EnsureConfigLoadedAsync();
        var user = new UserBitrix();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_USER_DATA + userId;
        string response = await GetAsync(url);
        dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(response)!;
        JToken result = responseConvert.result;
        if (result != null && result.Any())
        {
            dynamic first = result.First!;
            user.FirstName = first.NAME;
            user.LastName = first.LAST_NAME;
            user.Email = first.EMAIL;
        }
        return user;
    }

    public async Task<ContactBitrix> GetContactLienHeKhiTrienKhaiAsync(string dealId)
    {
        await EnsureConfigLoadedAsync();
        var contact = new ContactBitrix();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_LIST_CONTACT_OF_DEAL + dealId;
        string response = await GetAsync(url);
        dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(response)!;
        JToken result = responseConvert.result;
        if (result == null || !result.Any()) return contact;

        // Batch-fetch all contacts in a single API call to avoid N+1 calls
        var contactIds = result
            .Select(item => item["CONTACT_ID"]?.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToArray();
        if (contactIds.Length == 0) return contact;

        var batchData = new
        {
            filter = new Dictionary<string, object> { ["ID"] = contactIds },
            select = new[] { "ID", "NAME", "LAST_NAME", "POST", "PHONE", "EMAIL", "UF_CRM_1715045713" },
        };
        string batchResponse = await PostAsync(_bitrixSettings.ApiUrl + "/crm.contact.list", batchData);
        dynamic batchConvert = JsonConvert.DeserializeObject<dynamic>(batchResponse)!;
        JToken contacts = batchConvert.result;
        if (contacts == null) return contact;

        foreach (dynamic item in contacts)
        {
            bool isLienHe = item.UF_CRM_1715045713?.ToString() == "1";
            if (!isLienHe) continue;

            var c = new ContactBitrix
            {
                LastName = item.LAST_NAME ?? "",
                Name = item.NAME ?? "",
                Position = item.POST ?? "",
            };
            JToken phones = item.PHONE;
            if (phones != null && phones.Any()) c.Phone = phones.First!["VALUE"]?.ToString() ?? "";
            JToken emails = item.EMAIL;
            if (emails != null && emails.Any()) c.Email = emails.First!["VALUE"]?.ToString() ?? "";
            return c;
        }
        return contact;
    }

    private async Task<List<DataProductInDeal>> GetProductsInDealAsync(string dealId)
    {
        var products = new List<DataProductInDeal>();
        string url = _bitrixSettings.ApiUrl + ApiBitrixConstants.API_GET_PRODUCT_DATA + dealId;
        string response = await GetAsync(url);
        dynamic responseConvert = JsonConvert.DeserializeObject<dynamic>(response)!;
        JToken result = responseConvert.result;
        if (result == null || !result.Any()) return products;

        foreach (dynamic item in result)
        {
            products.Add(new DataProductInDeal
            {
                ID = item.ID,
                OWNER_ID = item.OWNER_ID ?? "",
                OWNER_TYPE = item.OWNER_TYPE ?? "",
                PRODUCT_ID = item.PRODUCT_ID,
                PRODUCT_NAME = item.PRODUCT_NAME ?? "",
                ORIGINAL_PRODUCT_NAME = item.ORIGINAL_PRODUCT_NAME ?? "",
                PRODUCT_DESCRIPTION = item.PRODUCT_DESCRIPTION ?? "",
                PRICE = item.PRICE ?? 0,
                PRICE_EXCLUSIVE = item.PRICE_EXCLUSIVE ?? 0,
                PRICE_NETTO = item.PRICE_NETTO ?? 0,
                PRICE_BRUTTO = item.PRICE_BRUTTO ?? 0,
                PRICE_ACCOUNT = item.PRICE_ACCOUNT ?? 0,
                QUANTITY = item.QUANTITY ?? 0,
                DISCOUNT_TYPE_ID = item.DISCOUNT_TYPE_ID ?? 0,
                DISCOUNT_RATE = item.DISCOUNT_RATE ?? 0,
                DISCOUNT_SUM = item.DISCOUNT_SUM ?? 0,
                TAX_RATE = item.TAX_RATE ?? 0,
                TAX_INCLUDED = item.TAX_INCLUDED ?? "",
                CUSTOMIZED = item.CUSTOMIZED ?? "",
                MEASURE_CODE = item.MEASURE_CODE ?? "",
                MEASURE_NAME = item.MEASURE_NAME ?? "",
                SORT = item.SORT ?? 0,
            });
        }
        return products;
    }

    public async Task PostJiraDataToDealAsync(BitrixDataDeal deal, string jiraKey)
    {
        await EnsureConfigLoadedAsync();
        string url = _bitrixSettings.ApiUrl + "/crm.deal.update";
        string jiraUrl = _jiraSettings.Url + "/browse/" + jiraKey;
        var data = new
        {
            id = deal.DealID,
            fields = new Dictionary<string, object>
            {
                ["UF_CRM_1616066175"] = jiraKey,
                ["UF_CRM_1616066206"] = jiraUrl,
            }
        };
        await PostAsync(url, data);
    }

    private async Task<BitrixDataDealApiResult> BuildDealResultAsync(dynamic dataDealApi, JObject customFields)
    {
        var deal = new BitrixDataDeal();
        var result = new BitrixDataDealApiResult { HaveError = false, HaveCreateIssues = false, HaveGetLate = false };

        if (dataDealApi.DATE_MODIFY != null)
        {
            DateTime dateModified = DateTime.Parse((string)dataDealApi.DATE_MODIFY);
            if (dateModified.AddMinutes(3) >= DateTime.Now)
            {
                result.HaveGetLate = true;
                return result;
            }
        }

        string urlDeal = $"{_bitrixSettings.DealDetailUrl}/{dataDealApi.ID}/";
        deal.LoaiDeal = dataDealApi.UF_CRM_1713881390 ?? "";
        deal.LinkCRM = urlDeal;
        deal.Pipeline = dataDealApi.CATEGORY_ID ?? "";

        bool isLostDeal = dataDealApi.STAGE_SEMANTIC_ID == "F";
        if (isLostDeal && deal.LoaiDeal != ConfigJiraBitrix.LoaiDeal_NgungHuyDichVu)
        {
            result.HaveCreateIssues = true;
            result.Message = "Lost deal không nằm trong danh mục tạo iss";
            result.DataDeal = deal;
            return result;
        }

        bool alreadyCreated = dataDealApi.UF_CRM_1616066206 != null && (string)dataDealApi.UF_CRM_1616066206 != "";
        if (alreadyCreated)
        {
            result.HaveCreateIssues = true;
            result.Message = "Deal đã tạo iss";
            result.DataDeal = deal;
            return result;
        }

        if (string.IsNullOrEmpty(deal.LoaiDeal))
        {
            result.Message = BuildErrorHtml("Không tạo được iss do không chọn Loại Deal.", urlDeal);
            result.HaveError = true;
            result.DataDeal = deal;
            return result;
        }

        List<string> missingFields = new();
        bool dealNotNeedCreateIss = false;
        var requiredFields = await _dbService.GetRequiredFieldsAsync(deal.LoaiDeal);
        if (requiredFields.Count > 0)
            missingFields = CheckRequireFieldBitrix.GetKeysWithNullOrEmptyValues(dataDealApi, requiredFields);
        else
            dealNotNeedCreateIss = true;

        deal.Responsible_UserID = dataDealApi.ASSIGNED_BY_ID ?? "";
        var user = await GetResponsibleUserByIdAsync(deal.Responsible_UserID);
        deal.Responsible_Email = user.Email;
        deal.Responsible_FirstName = user.FirstName;
        deal.Responsible_LastName = user.LastName;

        string dealIdStr = dataDealApi.ID.ToString();
        var contactBitrix = await GetContactLienHeKhiTrienKhaiAsync(dealIdStr);
        deal.Client_Contact_LastName_Name = $"{contactBitrix.LastName} {contactBitrix.Name}".Trim();
        deal.Client_Contact_Position = contactBitrix.Position;
        deal.Client_Contact_Phone = contactBitrix.Phone;
        deal.Client_Contact_Email = contactBitrix.Email;

        if (string.IsNullOrWhiteSpace(deal.Client_Contact_LastName_Name)) missingFields.Add("THONTINLIENHE_TENKHACH");
        if (string.IsNullOrWhiteSpace(deal.Client_Contact_Phone)) missingFields.Add("THONTINLIENHE_SDT");
        if (string.IsNullOrWhiteSpace(deal.Client_Contact_Email)) missingFields.Add("THONTINLIENHE_EMAIL");

        if (dealNotNeedCreateIss)
        {
            result.HaveCreateIssues = true;
            result.Message = "Deal Không nằm trong loại tạo iss";
            result.DataDeal = deal;
            return result;
        }

        if (missingFields.Any())
        {
            var fieldLabels = await _dbService.GetAllFieldLabelsAsync();
            var fieldNames = CheckRequireFieldBitrix.GetValuesForKeys(fieldLabels, missingFields);
            string fields = string.Join(", ", fieldNames);
            result.Message = BuildErrorHtml($"Không tạo được iss do không nhập trường bắt buộc: {fields}", urlDeal);
            result.HaveError = true;
            result.ToAddressEmail = deal.Responsible_Email;
            result.DataDeal = deal;
            return result;
        }

        // Map all fields
        JToken sanPhamToken = dataDealApi.UF_CRM_5D98084D1476E;
        if (sanPhamToken != null && sanPhamToken.Any())
        {
            var names = new List<string>();
            foreach (var item in sanPhamToken)
                names.Add(BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_5D98084D1476E", item.ToString()) ?? "");
            deal.NhuCauSanPham = string.Join(",", names);
        }

        deal.TenKhachSan = dataDealApi.UF_CRM_5B3F32B1118E0 ?? "";
        deal.ThiTruong = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1708697569", dataDealApi.UF_CRM_1708697569) ?? "";
        deal.LoaiHinhKhachSan = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1616123841", dataDealApi.UF_CRM_1616123841) ?? "";
        deal.Source = dataDealApi.SOURCE_ID ?? "";
        deal.CompanyId = dataDealApi.COMPANY_ID ?? "";
        deal.DealID = dataDealApi.ID.ToString();
        deal.DC_Khach_SoNhaDuong = dataDealApi.UF_CRM_5D984E2CA62C9 ?? "";
        deal.DC_Khach_TinhThanhPho = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_5D984E38B7502", dataDealApi.UF_CRM_5D984E38B7502) ?? "";
        deal.DC_Khach_QuanHuyen = dataDealApi.UF_CRM_5D984E34ACFB6 ?? "";
        deal.DC_Khach_PhuongXa = dataDealApi.UF_CRM_5D984E30EEE34 ?? "";
        deal.SoPhong = dataDealApi.UF_CRM_5B3F32B1068C7 ?? "";
        deal.SoGiuong = dataDealApi.UF_CRM_6054492BF24D8 ?? "";
        deal.Client_Contact_ContactID = dataDealApi.CONTACT_ID ?? "";
        deal.ChinhSachGia = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1708697773", dataDealApi.UF_CRM_1708697773) ?? "";
        deal.DanhSachSanPham = await GetProductsInDealAsync(deal.DealID);

        bool needSectionLookup = deal.LoaiDeal == ConfigJiraBitrix.LoaiDeal_CapKey && deal.DanhSachSanPham.Any();
        if (needSectionLookup)
        {
            JObject sectionData = await GetProductSectionListAsync();
            foreach (var product in deal.DanhSachSanPham)
            {
                try
                {
                    JObject productData = await GetProductByIdAsync(product.PRODUCT_ID.ToString());
                    string? sectionId = BitrixConvertData.GetValueFromObjectJson(productData, "SECTION_ID", product.PRODUCT_ID);
                    if (!string.IsNullOrEmpty(sectionId))
                    {
                        string? sectionName = BitrixConvertData.GetValueFromArrayJson(sectionData, "NAME", sectionId);
                        product.PRODUCT_NAME = sectionName ?? product.PRODUCT_NAME;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get section for product {ProductId}", product.PRODUCT_ID);
                }
            }
        }

        deal.MayChuChayPhanMem = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1708698013", dataDealApi.UF_CRM_1708698013) ?? "";
        deal.DungThuHayDungThat = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1715996452", dataDealApi.UF_CRM_1715996452) ?? "";
        deal.TrangThaiThanhToanLan1 = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1616123465", dataDealApi.UF_CRM_1616123465) ?? "";
        deal.NoiDungChuyenKhoanLan1 = dataDealApi.UF_CRM_1614068621890 ?? "";
        deal.PhuongThucTrienKhai = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1616130516", dataDealApi.UF_CRM_1616130516) ?? "";
        deal.ThoiDiemTrienKhai = string.IsNullOrEmpty(dataDealApi.UF_CRM_1616130611?.ToString()) ? null : (DateTime?)dataDealApi.UF_CRM_1616130611;
        deal.TaikhoanDungThuPMS = dataDealApi.UF_CRM_1616130767 ?? "";
        deal.Neu_TK_BE_CoTichHop_ezFolioKhong = dataDealApi.UF_CRM_1616130823 == "1" ? "Có" : "Không";
        deal.ThongTinTrienKhaiWeb = dataDealApi.UF_CRM_1616130900 ?? "";
        JToken ghiChuToken = dataDealApi.UF_CRM_1531008138;
        deal.GhiChu = ghiChuToken != null && ghiChuToken.Any() ? ghiChuToken[0]?.ToString() ?? "" : "";
        deal.MaKhachSan = dataDealApi.UF_CRM_1613727861184 ?? "";
        deal.YeuCauThem = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1708698698", dataDealApi.UF_CRM_1708698698) ?? "";

        if (dataDealApi.UF_CRM_1570243307 != null && dataDealApi.UF_CRM_1570243307.ToString() != "False")
        {
            var lyDos = new List<string>();
            foreach (var item in dataDealApi.UF_CRM_1570243307)
                lyDos.Add(BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1570243307", item) ?? "");
            deal.LyDoLost = lyDos;
        }

        deal.GhichuChoLyDoLost = dataDealApi.UF_CRM_1570243354 ?? "";
        deal.LoaiYeuCauHuy = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1708699318", dataDealApi.UF_CRM_1708699318) ?? "";
        deal.ThoiDiemNgungHuy = string.IsNullOrEmpty(dataDealApi.UF_CRM_1715997025?.ToString()) ? null : (DateTime?)dataDealApi.UF_CRM_1715997025;
        deal.TenDeal = dataDealApi.TITLE ?? "";
        deal.NgayBatDauTinh_GHBT = string.IsNullOrEmpty(dataDealApi.UF_CRM_1600922068?.ToString()) ? null : (DateTime?)dataDealApi.UF_CRM_1600922068;
        deal.NgayKetThucHan_GHBT = string.IsNullOrEmpty(dataDealApi.UF_CRM_1600922103?.ToString()) ? null : (DateTime?)dataDealApi.UF_CRM_1600922103;
        deal.TinhHuongCanCapKey = BitrixConvertData.GetValueItemInArrayFromJson(customFields, "UF_CRM_1715997550", dataDealApi.UF_CRM_1715997550) ?? "";

        JToken fileAttach = dataDealApi.UF_CRM_1613788692724;
        if (fileAttach != null && fileAttach.Any())
        {
            var files = new List<FileHopDongAttach>();
            foreach (var itemFile in fileAttach)
            {
                var objFile = new FileHopDongAttach();
                JObject objTmp = (JObject)itemFile;
                foreach (var sub in objTmp)
                {
                    if (sub.Key == "id") objFile.id = sub.Value?.ToString() ?? "";
                    if (sub.Key == "showUrl") objFile.showUrl = sub.Value?.ToString() ?? "";
                    if (sub.Key == "downloadUrl")
                    {
                        objFile.downloadUrl = sub.Value?.ToString() ?? "";
                        var fileParts = await objFile.DownloadFile(
                            deal.DealID, objFile.downloadUrl,
                            _bitrixSettings.BaseUrl, _bitrixSettings.User, _bitrixSettings.Password,
                            _bitrixSettings.AttachFilePath);
                        objFile.file_Name = fileParts[0];
                        objFile.path_file = fileParts[1];
                    }
                }
                files.Add(objFile);
            }
            deal.HopDong_PhuLuc = files;
        }

        result.DataDeal = deal;
        return result;
    }

    private static string BuildErrorHtml(string message, string urlDeal)
    {
        return $"<html><body><p>{message}</p><p>Link Deal: <a href='{urlDeal}'>{urlDeal}</a></p></body></html>";
    }

    private async Task<string> PostAsync(string url, object data)
    {
        return await RetryAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("Bitrix");
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        });
    }

    private async Task<string> GetAsync(string url)
    {
        return await RetryAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("Bitrix");
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        });
    }

    private async Task<string> RetryAsync(Func<Task<string>> action, int maxAttempts = 3)
    {
        int delayMs = 1000;
        Exception? lastEx = null;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                lastEx = ex;
                _logger.LogWarning(ex, "HTTP attempt {Attempt}/{Max} failed, retrying in {Delay}ms",
                    attempt, maxAttempts, delayMs);
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }
        throw lastEx!;
    }
}
