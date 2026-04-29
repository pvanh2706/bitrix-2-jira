using Newtonsoft.Json.Linq;

namespace BitrixJiraConnector.Api.Helpers;

public static class BitrixConvertData
{
    public static string? GetValueItemInArrayFromJson(JObject jobject, string key, dynamic id)
    {
        var items = jobject["result"]?[key]?["items"];
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item["ID"]?.ToString() == id?.ToString())
                    return item["VALUE"]?.ToString();
            }
        }
        return null;
    }

    public static string? GetValueFromArrayJson(JObject jobject, string key, dynamic id)
    {
        var items = jobject["result"];
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item["ID"]?.ToString() == id?.ToString())
                    return item[key]?.ToString();
            }
        }
        return null;
    }

    public static string? GetValueFromObjectJson(JObject jobject, string key, dynamic id)
    {
        var item = jobject["result"];
        if (item != null && item["ID"]?.ToString() == ((object?)id)?.ToString())
            return item[key]?.ToString();
        return null;
    }
}
