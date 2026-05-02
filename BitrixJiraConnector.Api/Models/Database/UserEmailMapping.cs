using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

/// <summary>
/// Maps Bitrix/company email addresses to Jira usernames.
/// Replaces hardcoded if/else in JiraService.GetJiraUsernameByEmailAsync.
/// </summary>
public class UserEmailMapping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Email { get; set; } = "";
    [Required]
    public string JiraUsername { get; set; } = "";
}
