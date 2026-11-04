using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Learn.CardFlow.CardRequests;

public interface IEmployeeCardRequestAppService : IApplicationService
{
    Task<EmployeeCardRequestDto> CreateAsync(
        CreateEmployeeCardRequestDto input);

    Task<EmployeeCardRequestDto> GetAsync(Guid id);

    Task<PagedResultDto<EmployeeCardRequestDto>> GetListAsync(
        GetCardRequestsInput input);

    Task<EmployeeCardRequestDto> CorrectAsync(Guid id, CorrectEmployeeCardRequestDto input);

    Task<EmployeeCardRequestDto> ResubmitAsync(Guid id);

    Task<int> GetMyAttentionCountAsync();
    Task<EmployeeCardRequestDto> StartReviewAsync(Guid id);

    Task<EmployeeCardRequestDto> ApproveAsync(Guid id);

    Task<EmployeeCardRequestDto> RejectAsync(Guid id, RejectEmployeeCardRequestDto input);
}
