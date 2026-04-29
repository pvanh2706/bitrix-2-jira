using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using BitrixJiraConnector.Api.Configurations;

namespace BitrixJiraConnector.Api.Models.Bitrix;

public class FileHopDongAttach
{
    public string id { get; set; } = "";
    public string showUrl { get; set; } = "";
    public string downloadUrl { get; set; } = "";
    public string path_file { get; set; } = "";
    public string file_Name { get; set; } = "";

    public async Task<List<string>> DownloadFile(string dealId, string url, string bitrixBaseUrl, string bitrixUser, string bitrixPassword, string attachFilePath)
    {
        string pathFile = attachFilePath;
        string fileName = "";
        List<string> result = new();
        try
        {
            using HttpClient client = new();
            string downloadUrl = bitrixBaseUrl + url;
            HttpRequestMessage request = new(HttpMethod.Get, downloadUrl);
            string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{bitrixUser}:{bitrixPassword}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentDisposition != null)
            {
                string? startFilename = response.Content.Headers.ContentDisposition.FileName?.Trim('"');
                fileName = dealId + "_" + startFilename;
                pathFile = pathFile + fileName;
                using var fileStream = new FileStream(pathFile, FileMode.Create, FileAccess.Write, FileShare.None);
                await (await response.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
            }
        }
        catch
        {
            throw;
        }
        result.Add(fileName);
        result.Add(pathFile);
        return result;
    }
}
