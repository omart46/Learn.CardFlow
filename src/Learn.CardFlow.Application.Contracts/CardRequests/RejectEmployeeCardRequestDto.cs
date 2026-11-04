using System.ComponentModel.DataAnnotations;

namespace Learn.CardFlow.CardRequests;

public class RejectEmployeeCardRequestDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
