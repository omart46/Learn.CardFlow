using System;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.Deliveries;

public class ReadyForDeliveryDto : EntityDto<Guid>
{
    // Id identifies the passed quality check to be handed over.
    public Guid EmployeeCardRequestId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
}
