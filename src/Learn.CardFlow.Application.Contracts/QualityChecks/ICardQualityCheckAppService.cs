using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace Learn.CardFlow.QualityChecks;
public interface ICardQualityCheckAppService : IApplicationService
{
    Task<PagedResultDto<ReadyForQualityCheckDto>> GetReadyRequestsAsync(PagedAndSortedResultRequestDto input);
    Task<CardQualityCheckDto> CreateAsync(Guid requestId);
    Task<CardQualityCheckDto> GetAsync(Guid id);
    Task<PagedResultDto<CardQualityCheckDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<CardQualityCheckDto> PassAsync(Guid id);
    Task<CardQualityCheckDto> FailAsync(Guid id, FailCardQualityCheckDto input);
}
