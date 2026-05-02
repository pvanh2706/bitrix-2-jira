using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

/// <summary>
/// Maps Bitrix CATEGORY_ID to a Jira team/pipeline name.
/// Replaces hardcoded switch in JiraService.GetTeamByCategoryId.
/// </summary>
public class PipelineMapping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    /// <summary>Bitrix CATEGORY_ID value (e.g. "0")</summary>
    [Required]
    public string CategoryId { get; set; } = "";
    /// <summary>Jira team/pipeline display name (e.g. "Sales")</summary>
    [Required]
    public string PipelineName { get; set; } = "";
}
