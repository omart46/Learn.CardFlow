using System;
using Learn.CardFlow.CardRequests;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.ProductionBatches;

public class ProductionBatchRequestDto : EntityDto<Guid>
{
    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public CardRequestStatus Status { get; set; }
}
