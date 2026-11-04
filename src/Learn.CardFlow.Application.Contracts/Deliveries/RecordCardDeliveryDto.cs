using System;
using System.ComponentModel.DataAnnotations;

namespace Learn.CardFlow.Deliveries;

public class RecordCardDeliveryDto
{
    public Guid CardQualityCheckId { get; set; }

    [Required]
    [StringLength(128)]
    public string RecipientName { get; set; } = string.Empty;
}
