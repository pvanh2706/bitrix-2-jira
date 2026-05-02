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
        const long maxFileSizeBytes = 10L * 1024 * 1024; // 10MB
        List<string> result = new() { "", attachFilePath };

        using HttpClient client = new();
        string downloadUrl = bitrixBaseUrl + url;
        string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{bitrixUser}:{bitrixPassword}"));

        // Check file size with a HEAD request before downloading to avoid wasting bandwidth
        var headRequest = new HttpRequestMessage(HttpMethod.Head, downloadUrl);
        headRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
        var headResponse = await client.SendAsync(headRequest);
        if (headResponse.Content.Headers.ContentLength.HasValue &&
            headResponse.Content.Headers.ContentLength.Value > maxFileSizeBytes)
        {
            return result; // Skip — file exceeds 10MB limit
        }

        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode || response.Content.Headers.ContentDisposition == null)
            return result;

        string? startFilename = response.Content.Headers.ContentDisposition.FileName?.Trim('"');
        string fileName = dealId + "_" + startFilename;
        string pathFile = attachFilePath + fileName;

        try
        {
            using var fileStream = new FileStream(pathFile, FileMode.Create, FileAccess.Write, FileShare.None);
            await (await response.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
        }
        catch
        {
            // Clean up any partial file to avoid accumulating garbage on disk
            if (File.Exists(pathFile)) File.Delete(pathFile);
            throw;
        }

        result[0] = fileName;
        result[1] = pathFile;
        return result;
    }
}
