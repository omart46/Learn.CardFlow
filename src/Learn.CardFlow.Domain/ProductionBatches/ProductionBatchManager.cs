using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Learn.CardFlow.ProductionBatches;

public class ProductionBatchManager : DomainService
{
    private readonly IRepository<ProductionBatch, Guid> _batchRepository;
    private readonly IRepository<EmployeeCardRequest, Guid> _requestRepository;

    public ProductionBatchManager(
        IRepository<ProductionBatch, Guid> batchRepository,
        IRepository<EmployeeCardRequest, Guid> requestRepository)
    {
        _batchRepository = batchRepository;
        _requestRepository = requestRepository;
    }

    public ProductionBatch Create(string name)
    {
        return new ProductionBatch(GuidGenerator.Create(), name);
    }

    public async Task AddRequestAsync(Guid batchId, Guid requestId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        batch.EnsureCanModifyRequests();

        var request = await _requestRepository.GetAsync(requestId);
        request.AssignToBatch(batch.Id);

        await _requestRepository.UpdateAsync(request);
        await _batchRepository.UpdateAsync(batch);
    }

    public async Task RemoveRequestAsync(Guid batchId, Guid requestId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        batch.EnsureCanModifyRequests();

        var request = await _requestRepository.GetAsync(requestId);
        request.RemoveFromBatch(batch.Id);

        await _requestRepository.UpdateAsync(request);
        await _batchRepository.UpdateAsync(batch);
    }

    public async Task<ProductionBatch> StartProductionAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var requestCount = await _requestRepository.CountAsync(
            request => request.ProductionBatchId == batch.Id);

        batch.StartProduction(requestCount);
        await _batchRepository.UpdateAsync(batch);

        return batch;
    }

    public async Task<ProductionBatch> CompleteProductionAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        batch.CompleteProduction();
        await _batchRepository.UpdateAsync(batch);

        return batch;
    }
}
