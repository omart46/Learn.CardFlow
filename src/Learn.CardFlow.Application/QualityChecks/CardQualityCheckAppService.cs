using System;
using System.Linq;
using System.Threading.Tasks;
using Learn.CardFlow.Permissions;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.ProductionBatches;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
namespace Learn.CardFlow.QualityChecks;
[Authorize(CardFlowPermissions.QualityChecks.Default)]
public class CardQualityCheckAppService : CardFlowAppService, ICardQualityCheckAppService
{
    private readonly CardQualityCheckManager _manager;
    private readonly IRepository<CardQualityCheck, Guid> _repository;
    private readonly IRepository<EmployeeCardRequest, Guid> _requests;
    private readonly IRepository<ProductionBatch, Guid> _batches;
    public CardQualityCheckAppService(CardQualityCheckManager manager,
        IRepository<CardQualityCheck, Guid> repository,
        IRepository<EmployeeCardRequest, Guid> requests,
        IRepository<ProductionBatch, Guid> batches)
    {
        _manager = manager;
        _repository = repository;
        _requests = requests;
        _batches = batches;
    }
    [Authorize(CardFlowPermissions.QualityChecks.Create)]
    public async Task<CardQualityCheckDto> CreateAsync(Guid requestId)
    {
        var check = await _manager.CreateAsync(requestId);
        await _repository.InsertAsync(check, autoSave: true);
        return await GetAsync(check.Id);
    }
    public async Task<CardQualityCheckDto> GetAsync(Guid id)
    {
        await _repository.GetAsync(id);
        return await AsyncExecuter.FirstAsync((await GetDisplayQueryAsync()).Where(check => check.Id == id));
    }
    public async Task<PagedResultDto<CardQualityCheckDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await GetDisplayQueryAsync();
        var count = await AsyncExecuter.CountAsync(query);
        var checks = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(check => check.CreationTime).ThenBy(check => check.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CardQualityCheckDto>(count, checks);
    }
    [Authorize(CardFlowPermissions.QualityChecks.Inspect)]
    public async Task<CardQualityCheckDto> PassAsync(Guid id)
    {
        var check = await _repository.GetAsync(id);
        check.Pass();
        await _repository.UpdateAsync(check, autoSave: true);
        return await GetAsync(check.Id);
    }
    [Authorize(CardFlowPermissions.QualityChecks.Inspect)]
    public async Task<CardQualityCheckDto> FailAsync(Guid id, FailCardQualityCheckDto input)
    {
        var check = await _repository.GetAsync(id);
        check.Fail(input.Reason);
        await _repository.UpdateAsync(check, autoSave: true);
        return await GetAsync(check.Id);
    }
    private async Task<IQueryable<CardQualityCheckDto>> GetDisplayQueryAsync()
    {
        var checks = await _repository.GetQueryableAsync();
        var requests = await _requests.GetQueryableAsync();
        var batches = await _batches.GetQueryableAsync();
        return from check in checks
               join request in requests on check.EmployeeCardRequestId equals request.Id into requestGroup
               from request in requestGroup.DefaultIfEmpty()
               join batch in batches on check.ProductionBatchId equals batch.Id into batchGroup
               from batch in batchGroup.DefaultIfEmpty()
               select new CardQualityCheckDto
               {
                   Id = check.Id, EmployeeCardRequestId = check.EmployeeCardRequestId,
                   ProductionBatchId = check.ProductionBatchId, Status = check.Status,
                   FailureReason = check.FailureReason, CreationTime = check.CreationTime,
                   EmployeeName = request == null ? "Unavailable employee" : request.EmployeeName,
                   EmployeeNumber = request == null ? "" : request.EmployeeNumber,
                   BatchName = batch == null ? "Unavailable batch" : batch.Name
               };
    }

    public async Task<PagedResultDto<ReadyForQualityCheckDto>> GetReadyRequestsAsync(PagedAndSortedResultRequestDto input)
    {
        var requests = await _requests.GetQueryableAsync();
        var batches = await _batches.GetQueryableAsync();
        var checks = await _repository.GetQueryableAsync();
        var query = from request in requests
                    join batch in batches on request.ProductionBatchId equals (Guid?)batch.Id
                    where batch.Status == ProductionBatchStatus.Completed
                        && !checks.Any(check => check.EmployeeCardRequestId == request.Id && check.ProductionBatchId == batch.Id)
                    select new ReadyForQualityCheckDto
                    {
                        Id = request.Id, EmployeeName = request.EmployeeName,
                        EmployeeNumber = request.EmployeeNumber, ProductionBatchId = batch.Id, BatchName = batch.Name
                    };
        var count = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(request => request.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<ReadyForQualityCheckDto>(count, items);
    }
}
