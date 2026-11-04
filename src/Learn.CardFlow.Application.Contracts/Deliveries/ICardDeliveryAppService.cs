using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Learn.CardFlow.Deliveries;

public interface ICardDeliveryAppService : IApplicationService
{
    Task<PagedResultDto<ReadyForDeliveryDto>> GetReadyCardsAsync(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<CardDeliveryDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<CardDeliveryDto> GetAsync(Guid id);
    Task<CardDeliveryDto> CreateAsync(RecordCardDeliveryDto input);
}
