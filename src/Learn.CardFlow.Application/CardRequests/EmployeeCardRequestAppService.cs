using System;
using System.Linq;
using System.Threading.Tasks;
using Learn.CardFlow.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp;

namespace Learn.CardFlow.CardRequests;

[Authorize(CardFlowPermissions.CardRequests.Default)]
public class EmployeeCardRequestAppService
    : CardFlowAppService, IEmployeeCardRequestAppService
{
    private readonly EmployeeCardRequestManager _manager;
    private readonly IRepository<EmployeeCardRequest, Guid> _repository;

    public EmployeeCardRequestAppService(
        EmployeeCardRequestManager manager,
        IRepository<EmployeeCardRequest, Guid> repository)
    {
        _manager = manager;
        _repository = repository;
    }

    [Authorize(CardFlowPermissions.CardRequests.StartReview)]
    public async Task<EmployeeCardRequestDto> StartReviewAsync(Guid id)
    {
        var request = await _repository.GetAsync(id);

        request.StartReview();

        await _repository.UpdateAsync(request, autoSave: true);

        return MapToDto(request);
    }

    [Authorize(CardFlowPermissions.CardRequests.Decide)]
    public async Task<EmployeeCardRequestDto> ApproveAsync(Guid id)
    {
        var request = await _repository.GetAsync(id);

        request.Approve();

        await _repository.UpdateAsync(request, autoSave: true);

        return MapToDto(request);
    }

    [Authorize(CardFlowPermissions.CardRequests.Decide)]
    public async Task<EmployeeCardRequestDto> RejectAsync(Guid id, RejectEmployeeCardRequestDto input)
    {
        var request = await _repository.GetAsync(id);

        request.Reject(input.Reason);

        await _repository.UpdateAsync(request, autoSave: true);

        return MapToDto(request);
    }

    [Authorize(CardFlowPermissions.CardRequests.Create)]
    public async Task<EmployeeCardRequestDto> CreateAsync(
        CreateEmployeeCardRequestDto input)
    {
        var request = _manager.Create(
            input.EmployeeName,
            input.EmployeeNumber
        );

        await _repository.InsertAsync(request, autoSave: true);

        return MapToDto(request);
    }

    public async Task<EmployeeCardRequestDto> GetAsync(Guid id)
    {
        var request = await _repository.GetAsync(id);

        return MapToDto(request);
    }

    public async Task<PagedResultDto<EmployeeCardRequestDto>> GetListAsync(
        GetCardRequestsInput input)
    {
        if (input.CreatedFrom.HasValue && input.CreatedTo.HasValue && input.CreatedFrom.Value.Date > input.CreatedTo.Value.Date)
            throw new BusinessException("CardFlow:InvalidRequestDateRange");
        var query = await _repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(request => request.EmployeeName.Contains(filter) || request.EmployeeNumber.Contains(filter));
        }
        if (input.Status.HasValue)
            query = query.Where(request => request.Status == input.Status.Value);
        if (input.CreatedFrom.HasValue)
        {
            var from = input.CreatedFrom.Value.Date;
            query = query.Where(request => request.CreationTime >= from);
        }
        if (input.CreatedTo.HasValue)
        {
            var to = input.CreatedTo.Value.Date;
            if (to < DateTime.MaxValue.Date)
            {
                var endExclusive = to.AddDays(1);
                query = query.Where(request => request.CreationTime < endExclusive);
            }
        }
        if (input.OnlyMine || input.NeedsAttention)
        {
            var userId = CurrentUser.GetId();
            query = query.Where(request => request.CreatorId == userId);
        }
        if (input.NeedsAttention)
        {
            query = query.Where(request => request.Status == CardRequestStatus.Rejected || request.NeedsResubmission);
        }
        var totalCount = await AsyncExecuter.CountAsync(query);
        var requests = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(request => request.CreationTime).ThenBy(request => request.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = requests.Select(MapToDto).ToList();

        return new PagedResultDto<EmployeeCardRequestDto>(
            totalCount,
            items
        );
    }

    [Authorize(CardFlowPermissions.CardRequests.Edit)]
    public async Task<EmployeeCardRequestDto> CorrectAsync(Guid id, CorrectEmployeeCardRequestDto input)
    {
        var request = await _repository.GetAsync(id);
        request.Correct(CurrentUser.GetId(), input.EmployeeName, input.EmployeeNumber);
        await _repository.UpdateAsync(request, autoSave: true);
        return MapToDto(request);
    }

    [Authorize(CardFlowPermissions.CardRequests.Edit)]
    public async Task<EmployeeCardRequestDto> ResubmitAsync(Guid id)
    {
        var request = await _repository.GetAsync(id);
        request.Resubmit(CurrentUser.GetId());
        await _repository.UpdateAsync(request, autoSave: true);
        return MapToDto(request);
    }

    public async Task<int> GetMyAttentionCountAsync()
    {
        var userId = CurrentUser.GetId();
        return await _repository.CountAsync(request => request.CreatorId == userId &&
            (request.Status == CardRequestStatus.Rejected || request.NeedsResubmission));
    }

    private EmployeeCardRequestDto MapToDto(
        EmployeeCardRequest request)
    {
        return new EmployeeCardRequestDto
        {
            Id = request.Id,
            EmployeeName = request.EmployeeName,
            EmployeeNumber = request.EmployeeNumber,
            Status = request.Status,
            RejectionReason = request.RejectionReason,
            IsMine = request.CreatorId == CurrentUser.Id,
            NeedsResubmission = request.NeedsResubmission,
            CreationTime = request.CreationTime
        };
    }
}
