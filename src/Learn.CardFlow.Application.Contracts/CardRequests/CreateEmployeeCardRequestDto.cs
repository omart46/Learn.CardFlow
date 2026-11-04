using System.ComponentModel.DataAnnotations;

namespace Learn.CardFlow.CardRequests;

public class CreateEmployeeCardRequestDto
{
    [Required]
    [StringLength(128)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string EmployeeNumber { get; set; } = string.Empty;
}