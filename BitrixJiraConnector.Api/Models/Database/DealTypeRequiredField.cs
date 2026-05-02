using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

/// <summary>
/// Lists required Bitrix field keys per deal type.
/// Replaces ConfigJiraBitrix.FieldRequire_* static lists.
/// </summary>
public class DealTypeRequiredField
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    /// <summary>Bitrix deal type ID (e.g. "3095")</summary>
    [Required]
    public string DealTypeId { get; set; } = "";
    /// <summary>Bitrix field key (e.g. "UF_CRM_1713881390" or "ASSIGNED_BY_ID")</summary>
    [Required]
    public string FieldKey { get; set; } = "";
}
