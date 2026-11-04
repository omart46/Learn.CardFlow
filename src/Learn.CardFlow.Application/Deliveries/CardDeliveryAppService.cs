using System;
using System.Linq;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.Permissions;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Learn.CardFlow.Deliveries;

[Authorize(CardFlowPermissions.Deliveries.Default)]
public class CardDeliveryAppService : CardFlowAppService, ICardDeliveryAppService
{
    private readonly CardDeliveryManager _manager;
    private readonly IRepository<CardDelivery, Guid> _deliveries;
    private readonly IRepository<CardQualityCheck, Guid> _checks;
    private readonly IRepository<EmployeeCardRequest, Guid> _requests;
    private readonly IRepository<ProductionBatch, Guid> _batches;
    private readonly IDataFilter _filter;

    public CardDeliveryAppService(CardDeliveryManager manager, IRepository<CardDelivery, Guid> deliveries,
        IRepository<CardQualityCheck, Guid> checks, IRepository<EmployeeCardRequest, Guid> requests,
        IRepository<ProductionBatch, Guid> batches, IDataFilter filter)
    {
        _manager = manager; _deliveries = deliveries; _checks = checks;
        _requests = requests; _batches = batches; _filter = filter;
    }

    [Authorize(CardFlowPermissions.Deliveries.Record)]
    public async Task<CardDeliveryDto> CreateAsync(RecordCardDeliveryDto input)
    {
        var delivery = await _manager.CreateAsync(input.CardQualityCheckId, input.RecipientName, CurrentUser.GetId());
        await _deliveries.InsertAsync(delivery, autoSave: true);
        return await GetAsync(delivery.Id);
    }

    public async Task<CardDeliveryDto> GetAsync(Guid id)
    {
        await _deliveries.GetAsync(id);
        return await AsyncExecuter.FirstAsync((await GetDisplayQueryAsync()).Where(delivery => delivery.Id == id));
    }

    public async Task<PagedResultDto<CardDeliveryDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await GetDisplayQueryAsync();
        var count = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(delivery => delivery.DeliveredAt)
            .ThenBy(delivery => delivery.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CardDeliveryDto>(count, items);
    }

    private async Task<IQueryable<CardDeliveryDto>> GetDisplayQueryAsync()
    {
        var deliveries = await _deliveries.GetQueryableAsync();
        var checks = await _checks.GetQueryableAsync();
        var requests = await _requests.GetQueryableAsync();
        var batches = await _batches.GetQueryableAsync();
        return from delivery in deliveries
               join request in requests on delivery.EmployeeCardRequestId equals request.Id into requestGroup
               from request in requestGroup.DefaultIfEmpty()
               join check in checks on delivery.CardQualityCheckId equals check.Id into checkGroup
               from check in checkGroup.DefaultIfEmpty()
               join batch in batches on (check == null ? Guid.Empty : check.ProductionBatchId) equals batch.Id into batchGroup
               from batch in batchGroup.DefaultIfEmpty()
               select new CardDeliveryDto
               {
                   Id = delivery.Id, EmployeeCardRequestId = delivery.EmployeeCardRequestId,
                   CardQualityCheckId = delivery.CardQualityCheckId, RecipientName = delivery.RecipientName,
                   DeliveredAt = delivery.DeliveredAt, DeliveredById = delivery.DeliveredById,
                   EmployeeName = request == null ? "Unavailable employee" : request.EmployeeName,
                   EmployeeNumber = request == null ? "" : request.EmployeeNumber,
                   BatchName = batch == null ? "Unavailable batch" : batch.Name
               };
    }

    public async Task<PagedResultDto<ReadyForDeliveryDto>> GetReadyCardsAsync(PagedAndSortedResultRequestDto input)
    {
        // Evaluate inside the scope so archived receipts also exclude delivered cards.
        using (_filter.Disable<ISoftDelete>())
        {
            var deliveries = await _deliveries.GetQueryableAsync();
            var checks = await _checks.GetQueryableAsync();
            var requests = await _requests.GetQueryableAsync();
            var batches = await _batches.GetQueryableAsync();
            var query = from check in checks
                        join request in requests on check.EmployeeCardRequestId equals request.Id
                        join batch in batches on check.ProductionBatchId equals batch.Id
                        where !check.IsDeleted && !request.IsDeleted && !batch.IsDeleted
                            && check.Status == CardQualityStatus.Passed
                            && request.ProductionBatchId == batch.Id
                            && batch.Status == ProductionBatchStatus.Completed
                            && !deliveries.Any(delivery => delivery.EmployeeCardRequestId == request.Id)
                        select new ReadyForDeliveryDto
                        {
                            Id = check.Id, EmployeeCardRequestId = request.Id, EmployeeName = request.EmployeeName,
                            EmployeeNumber = request.EmployeeNumber, BatchName = batch.Name
                        };
            var count = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.OrderBy(card => card.Id)
                .Skip(input.SkipCount).Take(input.MaxResultCount));
            return new PagedResultDto<ReadyForDeliveryDto>(count, items);
        }
    }
}
