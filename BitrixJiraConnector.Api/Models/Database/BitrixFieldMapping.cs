using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitrixJiraConnector.Api.Models.Database;

/// <summary>
/// Maps logical field key (e.g. UF_CRM_1713881390) to a Vietnamese display label.
/// Replaces ConfigJiraBitrix.KeyValueField_Bitrix dictionary.
/// </summary>
public class BitrixFieldMapping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string FieldKey { get; set; } = "";
    [Required]
    public string FieldLabel { get; set; } = "";
}
