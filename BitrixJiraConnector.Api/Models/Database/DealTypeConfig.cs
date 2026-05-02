using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

/// <summary>
/// Maps a Bitrix deal type ID to its Jira project key and issue type.
/// Replaces ConfigJiraBitrix.LoaiDeal_*, IssueProject_*, IdIssueType_* constants.
/// </summary>
public class DealTypeConfig
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    /// <summary>Bitrix deal type ID value (e.g. "3095")</summary>
    [Required]
    public string DealTypeId { get; set; } = "";
    /// <summary>Human-readable name (e.g. "Triển Khai Mới")</summary>
    [Required]
    public string DealTypeName { get; set; } = "";
    /// <summary>Jira project key (e.g. "TRIENKHAI")</summary>
    [Required]
    public string JiraProjectKey { get; set; } = "";
    /// <summary>Jira issue type ID (e.g. "10000")</summary>
    [Required]
    public string JiraIssueTypeId { get; set; } = "";
    /// <summary>1 = deal needs a Jira issue created; 0 = skip</summary>
    public int ShouldCreateIssue { get; set; } = 1;
}
