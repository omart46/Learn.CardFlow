using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.Deliveries;
using Learn.CardFlow.Permissions;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Learn.CardFlow.Dashboard;

[Authorize]
public class DashboardAppService : CardFlowAppService, IDashboardAppService
{
    private readonly IRepository<EmployeeCardRequest, Guid> _requests;
    private readonly IRepository<ProductionBatch, Guid> _batches;
    private readonly IRepository<CardQualityCheck, Guid> _checks;
    private readonly IRepository<CardDelivery, Guid> _deliveries;
    private readonly ICardDeliveryAppService _deliveryService;

    public DashboardAppService(
        IRepository<EmployeeCardRequest, Guid> requests,
        IRepository<ProductionBatch, Guid> batches,
        IRepository<CardQualityCheck, Guid> checks,
        IRepository<CardDelivery, Guid> deliveries,
        ICardDeliveryAppService deliveryService)
    {
        _requests = requests;
        _batches = batches;
        _checks = checks;
        _deliveries = deliveries;
        _deliveryService = deliveryService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var summary = new DashboardSummaryDto();

        if (await AuthorizationService.IsGrantedAsync(CardFlowPermissions.CardRequests.Default))
        {
            summary.CardRequests = new CardRequestsSummaryDto
            {
                Draft = await _requests.CountAsync(request => request.Status == CardRequestStatus.Draft),
                UnderReview = await _requests.CountAsync(request => request.Status == CardRequestStatus.UnderReview),
                Approved = await _requests.CountAsync(request => request.Status == CardRequestStatus.Approved),
                Rejected = await _requests.CountAsync(request => request.Status == CardRequestStatus.Rejected)
            };
        }

        if (await AuthorizationService.IsGrantedAsync(CardFlowPermissions.ProductionBatches.Default))
        {
            summary.Production = new ProductionSummaryDto
            {
                Draft = await _batches.CountAsync(batch => batch.Status == ProductionBatchStatus.Draft),
                InProduction = await _batches.CountAsync(batch => batch.Status == ProductionBatchStatus.InProduction),
                Completed = await _batches.CountAsync(batch => batch.Status == ProductionBatchStatus.Completed)
            };
        }

        if (await AuthorizationService.IsGrantedAsync(CardFlowPermissions.QualityChecks.Default))
        {
            summary.Quality = new QualitySummaryDto
            {
                Pending = await _checks.CountAsync(check => check.Status == CardQualityStatus.Pending),
                Passed = await _checks.CountAsync(check => check.Status == CardQualityStatus.Passed),
                Failed = await _checks.CountAsync(check => check.Status == CardQualityStatus.Failed)
            };
        }

        if (await AuthorizationService.IsGrantedAsync(CardFlowPermissions.Deliveries.Default))
        {
            var readyCards = await _deliveryService.GetReadyCardsAsync(
                new PagedAndSortedResultRequestDto { MaxResultCount = 1 });

            summary.Delivery = new DeliverySummaryDto
            {
                ReadyForDelivery = readyCards.TotalCount,
                Delivered = await _deliveries.GetCountAsync()
            };
        }

        summary.GeneratedAt = Clock.Now;
        return summary;
    }
}
