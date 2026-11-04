using System;
using System.Linq;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Learn.CardFlow.ProductionBatches;

[Authorize(CardFlowPermissions.ProductionBatches.Default)]
public class ProductionBatchAppService : CardFlowAppService, IProductionBatchAppService
{
    private readonly ProductionBatchManager _manager;
    private readonly IRepository<ProductionBatch, Guid> _batchRepository;
    private readonly IRepository<EmployeeCardRequest, Guid> _requestRepository;

    public ProductionBatchAppService(
        ProductionBatchManager manager,
        IRepository<ProductionBatch, Guid> batchRepository,
        IRepository<EmployeeCardRequest, Guid> requestRepository)
    {
        _manager = manager;
        _batchRepository = batchRepository;
        _requestRepository = requestRepository;
    }

    public async Task<PagedResultDto<ProductionBatchRequestDto>> GetReadyRequestsAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = (await _requestRepository.GetQueryableAsync())
            .Where(request => request.Status == CardRequestStatus.Approved && request.ProductionBatchId == null);
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(request => request.CreationTime).ThenBy(request => request.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount)
            .Select(request => new ProductionBatchRequestDto
            {
                Id = request.Id,
                EmployeeName = request.EmployeeName,
                EmployeeNumber = request.EmployeeNumber,
                Status = request.Status
            }));
        return new PagedResultDto<ProductionBatchRequestDto>(total, items);
    }

    [Authorize(CardFlowPermissions.ProductionBatches.Create)]
    public async Task<ProductionBatchDto> CreateAsync(CreateProductionBatchDto input)
    {
        var batch = _manager.Create(input.Name);
        await _batchRepository.InsertAsync(batch, autoSave: true);
        return MapToDto(batch, 0);
    }

    public async Task<ProductionBatchDto> GetAsync(Guid id)
    {
        var batch = await _batchRepository.GetAsync(id);
        return await MapWithCountAsync(batch);
    }

    public async Task<PagedResultDto<ProductionBatchDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var batches = await _batchRepository.GetQueryableAsync();
        var requests = await _requestRepository.GetQueryableAsync();
        var totalCount = await AsyncExecuter.CountAsync(batches);
        var items = await AsyncExecuter.ToListAsync(batches
            .OrderByDescending(batch => batch.CreationTime).ThenBy(batch => batch.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount)
            .Select(batch => new ProductionBatchDto
            {
                Id = batch.Id,
                Name = batch.Name,
                Status = batch.Status,
                CreationTime = batch.CreationTime,
                RequestCount = requests.Count(request => request.ProductionBatchId == batch.Id)
            }));

        return new PagedResultDto<ProductionBatchDto>(totalCount, items);
    }

    public async Task<PagedResultDto<ProductionBatchRequestDto>> GetRequestsAsync(
        Guid id, PagedAndSortedResultRequestDto input)
    {
        await _batchRepository.GetAsync(id);
        var query = (await _requestRepository.GetQueryableAsync())
            .Where(request => request.ProductionBatchId == id);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query
            .OrderBy(request => request.CreationTime).ThenBy(request => request.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount)
            .Select(request => new ProductionBatchRequestDto
            {
                Id = request.Id,
                EmployeeName = request.EmployeeName,
                EmployeeNumber = request.EmployeeNumber,
                Status = request.Status
            }));

        return new PagedResultDto<ProductionBatchRequestDto>(totalCount, items);
    }

    [Authorize(CardFlowPermissions.ProductionBatches.ManageRequests)]
    public async Task AddRequestAsync(Guid id, Guid requestId)
    {
        await _manager.AddRequestAsync(id, requestId);
    }

    [Authorize(CardFlowPermissions.ProductionBatches.ManageRequests)]
    public async Task RemoveRequestAsync(Guid id, Guid requestId)
    {
        await _manager.RemoveRequestAsync(id, requestId);
    }

    [Authorize(CardFlowPermissions.ProductionBatches.Produce)]
    public async Task<ProductionBatchDto> StartProductionAsync(Guid id)
    {
        var batch = await _manager.StartProductionAsync(id);
        return await MapWithCountAsync(batch);
    }

    [Authorize(CardFlowPermissions.ProductionBatches.Produce)]
    public async Task<ProductionBatchDto> CompleteProductionAsync(Guid id)
    {
        var batch = await _manager.CompleteProductionAsync(id);
        return await MapWithCountAsync(batch);
    }

    private async Task<ProductionBatchDto> MapWithCountAsync(ProductionBatch batch)
    {
        var count = await _requestRepository.CountAsync(
            request => request.ProductionBatchId == batch.Id);
        return MapToDto(batch, count);
    }

    private static ProductionBatchDto MapToDto(ProductionBatch batch, int requestCount)
    {
        return new ProductionBatchDto
        {
            Id = batch.Id,
            Name = batch.Name,
            Status = batch.Status,
            CreationTime = batch.CreationTime,
            RequestCount = requestCount
        };
    }
}
