using Newtonsoft.Json.Linq;

namespace BitrixJiraConnector.Api.Helpers;

public static class CheckRequireFieldBitrix
{
    public static List<string> GetKeysWithNullOrEmptyValues(dynamic dataDealApi, IReadOnlyList<string> keys)
    {
        var missing = new List<string>();
        JObject data = dataDealApi as JObject ?? throw new ArgumentException("dataDealApi must be JObject");

        foreach (var key in keys)
        {
            try
            {
                var value = data[key];
                if (value == null || value.Type == JTokenType.Null) { missing.Add(key); continue; }
                if (value.Type == JTokenType.String && string.IsNullOrEmpty(value.ToString())) { missing.Add(key); continue; }
                if (value.Type == JTokenType.Array && ((JArray)value).Count == 0) { missing.Add(key); continue; }
                if (value.Type == JTokenType.Object && !((JObject)value).HasValues) { missing.Add(key); continue; }
            }
            catch
            {
                missing.Add(key);
            }
        }
        return missing;
    }

    public static List<string> GetValuesForKeys(IReadOnlyDictionary<string, string> keyValuePairs, List<string> keys)
    {
        return keys.Select(k => keyValuePairs.TryGetValue(k, out var v) ? v : "").ToList();
    }
}
