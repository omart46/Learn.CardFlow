using System;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.ProductionBatches;

public class ProductionBatchDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public ProductionBatchStatus Status { get; set; }

    public int RequestCount { get; set; }

    public DateTime CreationTime { get; set; }
}
