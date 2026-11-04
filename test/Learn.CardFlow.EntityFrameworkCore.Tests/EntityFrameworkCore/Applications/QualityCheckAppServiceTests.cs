using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore.Applications;

[Collection(CardFlowTestConsts.CollectionDefinitionName)]
public class QualityCheckAppServiceTests : CardFlowEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Ready_Queue_And_Inspection_Results_Should_Follow_Production()
    {
        var requests = GetRequiredService<IRepository<EmployeeCardRequest, Guid>>();
        var batches = GetRequiredService<IRepository<ProductionBatch, Guid>>();
        var service = GetRequiredService<ICardQualityCheckAppService>();
        var completed = new ProductionBatch(Guid.NewGuid(), "Completed batch");
        var draft = new ProductionBatch(Guid.NewGuid(), "Draft batch");
        var first = new EmployeeCardRequest(Guid.NewGuid(), "First employee", "QC-001");
        var second = new EmployeeCardRequest(Guid.NewGuid(), "Second employee", "QC-002");
        var excluded = new EmployeeCardRequest(Guid.NewGuid(), "Not produced", "QC-003");
        foreach (var request in new[] { first, second, excluded })
        {
            request.StartReview(); request.Approve();
        }
        first.AssignToBatch(completed.Id); second.AssignToBatch(completed.Id);
        excluded.AssignToBatch(draft.Id);
        completed.StartProduction(2); completed.CompleteProduction();
        await WithUnitOfWorkAsync(async () =>
        {
            await batches.InsertAsync(completed); await batches.InsertAsync(draft);
            await requests.InsertAsync(first); await requests.InsertAsync(second); await requests.InsertAsync(excluded);
        });

        var page = new PagedAndSortedResultRequestDto { MaxResultCount = 100 };
        var ready = await service.GetReadyRequestsAsync(page);
        ready.Items.ShouldContain(item => item.Id == first.Id && item.BatchName == completed.Name);
        ready.Items.ShouldNotContain(item => item.Id == excluded.Id);

        var firstCheck = await service.CreateAsync(first.Id);
        firstCheck.EmployeeName.ShouldBe(first.EmployeeName);
        (await service.GetReadyRequestsAsync(page)).Items.ShouldNotContain(item => item.Id == first.Id);
        (await service.PassAsync(firstCheck.Id)).Status.ShouldBe(CardQualityStatus.Passed);

        var secondCheck = await service.CreateAsync(second.Id);
        var failed = await service.FailAsync(secondCheck.Id, new FailCardQualityCheckDto { Reason = "Unclear print" });
        failed.Status.ShouldBe(CardQualityStatus.Failed);
        failed.FailureReason.ShouldBe("Unclear print");
        (await service.GetListAsync(page)).Items.ShouldContain(item =>
            item.Id == secondCheck.Id && item.EmployeeNumber == second.EmployeeNumber && item.BatchName == completed.Name);
    }
}
