using System.ComponentModel.DataAnnotations;

namespace Learn.CardFlow.ProductionBatches;

public class CreateProductionBatchDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
}
