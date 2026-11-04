using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Learn.CardFlow.ProductionBatches;

public class ProductionBatch : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public ProductionBatchStatus Status { get; private set; }

    protected ProductionBatch()
    {
    }

    public ProductionBatch(Guid id, string name)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            maxLength: 128
        ).Trim();

        Status = ProductionBatchStatus.Draft;
    }
    public void EnsureCanModifyRequests()
    {
        if (Status != ProductionBatchStatus.Draft)
        {
            throw new BusinessException(
                "CardFlow:OnlyDraftBatchesCanBeModified"
            );
        }
    }

    public void StartProduction(int requestCount)
    {
        EnsureCanModifyRequests();

        if (requestCount <= 0)
        {
            throw new BusinessException(
                "CardFlow:CannotStartEmptyBatch"
            );
        }

        Status = ProductionBatchStatus.InProduction;
    }

    public void CompleteProduction()
    {
        if (Status != ProductionBatchStatus.InProduction)
        {
            throw new BusinessException(
                "CardFlow:BatchMustBeInProductionToComplete"
            );
        }

        Status = ProductionBatchStatus.Completed;
    }
}