using System;
using Volo.Abp.Application.Dtos;
namespace Learn.CardFlow.QualityChecks;
public class CardQualityCheckDto : EntityDto<Guid>
{
    public Guid EmployeeCardRequestId { get; set; }
    public Guid ProductionBatchId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public CardQualityStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreationTime { get; set; }
}
