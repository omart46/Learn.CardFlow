using System.ComponentModel.DataAnnotations;
namespace Learn.CardFlow.QualityChecks;
public class FailCardQualityCheckDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
