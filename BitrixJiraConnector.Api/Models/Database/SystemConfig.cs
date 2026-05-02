using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

public class SystemConfig
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string ConfigKey { get; set; } = "";
    [Required]
    public string ConfigValue { get; set; } = "";
    public string Description { get; set; } = "";
}
