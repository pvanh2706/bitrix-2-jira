using System.ComponentModel.DataAnnotations;

namespace BitrixJiraConnector.Api.Models.Database;

public class ConfigData
{
    [Key]
    public int ID { get; set; }
    public string KeyConfig { get; set; } = "";
    public string ValueConfig { get; set; } = "";
    public string Description { get; set; } = "";
}
