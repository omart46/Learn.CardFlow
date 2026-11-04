using System;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.QualityChecks;

public class ReadyForQualityCheckDto : EntityDto<Guid>
{
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid ProductionBatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
}
