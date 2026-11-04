using System;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.Deliveries;

public class CardDeliveryDto : EntityDto<Guid>
{
    public Guid EmployeeCardRequestId { get; set; }
    public Guid CardQualityCheckId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public Guid DeliveredById { get; set; }
}
