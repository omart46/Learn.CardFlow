using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Learn.CardFlow.ProductionBatches;

public interface IProductionBatchAppService : IApplicationService
{
    Task<PagedResultDto<ProductionBatchRequestDto>> GetReadyRequestsAsync(
        PagedAndSortedResultRequestDto input);

    Task<ProductionBatchDto> CreateAsync(CreateProductionBatchDto input);

    Task<ProductionBatchDto> GetAsync(Guid id);

    Task<PagedResultDto<ProductionBatchDto>> GetListAsync(
        PagedAndSortedResultRequestDto input);

    Task<PagedResultDto<ProductionBatchRequestDto>> GetRequestsAsync(
        Guid id,
        PagedAndSortedResultRequestDto input);

    Task AddRequestAsync(Guid id, Guid requestId);

    Task RemoveRequestAsync(Guid id, Guid requestId);

    Task<ProductionBatchDto> StartProductionAsync(Guid id);

    Task<ProductionBatchDto> CompleteProductionAsync(Guid id);
}
