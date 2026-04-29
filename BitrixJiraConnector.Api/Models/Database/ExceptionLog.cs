using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

public class ExceptionLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int DealID { get; set; }
    public string ExceptionMessage { get; set; } = "";
    public string StackTrace { get; set; } = "";
    public string ExceptionType { get; set; } = "";
    public string Source { get; set; } = "";
    [Required]
    public string LoggedAt { get; set; } = "";
}
