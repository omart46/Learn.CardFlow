using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Learn.CardFlow.Deliveries;

public class CardDeliveryManager : DomainService
{
    private readonly IRepository<CardDelivery, Guid> _deliveries;
    private readonly IRepository<CardQualityCheck, Guid> _checks;
    private readonly IRepository<EmployeeCardRequest, Guid> _requests;
    private readonly IRepository<ProductionBatch, Guid> _batches;
    private readonly IDataFilter _filter;

    public CardDeliveryManager(IRepository<CardDelivery, Guid> deliveries,
        IRepository<CardQualityCheck, Guid> checks,
        IRepository<EmployeeCardRequest, Guid> requests,
        IRepository<ProductionBatch, Guid> batches, IDataFilter filter)
    {
        _deliveries = deliveries; _checks = checks; _requests = requests;
        _batches = batches; _filter = filter;
    }

    public async Task<CardDelivery> CreateAsync(Guid qualityCheckId, string recipientName, Guid actorId)
    {
        var check = await _checks.GetAsync(qualityCheckId);
        if (check.Status != CardQualityStatus.Passed)
            throw new BusinessException("CardFlow:OnlyPassedCardsCanBeDelivered");
        var request = await _requests.GetAsync(check.EmployeeCardRequestId);
        var batch = await _batches.GetAsync(check.ProductionBatchId);
        if (request.ProductionBatchId != batch.Id || batch.Status != ProductionBatchStatus.Completed)
            throw new BusinessException("CardFlow:DeliveryProductionMismatch");

        // A historical receipt still prevents delivering the same card twice.
        using (_filter.Disable<ISoftDelete>())
        {
            if (await _deliveries.AnyAsync(delivery => delivery.EmployeeCardRequestId == request.Id))
                throw new BusinessException("CardFlow:CardAlreadyDelivered");
        }
        return new CardDelivery(GuidGenerator.Create(), request.Id, check.Id, recipientName, Clock.Now, actorId);
    }
}
