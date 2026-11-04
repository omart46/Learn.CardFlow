using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.ProductionBatches;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
namespace Learn.CardFlow.QualityChecks;
public class CardQualityCheckManager : DomainService
{
    private readonly IRepository<EmployeeCardRequest, Guid> _requestRepository;
    private readonly IRepository<ProductionBatch, Guid> _batchRepository;
    private readonly IRepository<CardQualityCheck, Guid> _qualityCheckRepository;
    public CardQualityCheckManager(
        IRepository<EmployeeCardRequest, Guid> requestRepository,
        IRepository<ProductionBatch, Guid> batchRepository,
        IRepository<CardQualityCheck, Guid> qualityCheckRepository)
    {
        _requestRepository = requestRepository;
        _batchRepository = batchRepository;
        _qualityCheckRepository = qualityCheckRepository;
    }
    public async Task<CardQualityCheck> CreateAsync(Guid requestId)
    {
        var request = await _requestRepository.GetAsync(requestId);
        if (!request.ProductionBatchId.HasValue)
            throw new BusinessException("CardFlow:RequestMustBelongToProductionBatch");
        var batch = await _batchRepository.GetAsync(request.ProductionBatchId.Value);
        if (batch.Status != ProductionBatchStatus.Completed)
            throw new BusinessException("CardFlow:ProductionMustBeCompletedBeforeQualityCheck");
        if (await _qualityCheckRepository.AnyAsync(check =>
            check.EmployeeCardRequestId == request.Id && check.ProductionBatchId == batch.Id))
            throw new BusinessException("CardFlow:QualityCheckAlreadyExists");
        return new CardQualityCheck(GuidGenerator.Create(), request.Id, batch.Id);
    }
}
