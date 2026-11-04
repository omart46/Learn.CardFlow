using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore.Applications;

[Collection(CardFlowTestConsts.CollectionDefinitionName)]
public class CardRequestFilterTests : CardFlowEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Search_Status_And_Dates_Should_Filter_Before_Paging()
    {
        var repository = GetRequiredService<IRepository<EmployeeCardRequest, Guid>>();
        var service = GetRequiredService<IEmployeeCardRequestAppService>();
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var early = new EmployeeCardRequest(Guid.NewGuid(), "أحمد " + prefix, prefix + "-01");
        var late = new EmployeeCardRequest(Guid.NewGuid(), "أحمد " + prefix, prefix + "-02");
        var outside = new EmployeeCardRequest(Guid.NewGuid(), "Other " + prefix, prefix + "-03");
        late.StartReview(); late.Approve(); outside.StartReview(); outside.Approve();
        SetCreation(early, new DateTime(2026, 10, 20, 0, 0, 0));
        SetCreation(late, new DateTime(2026, 10, 20, 23, 59, 59));
        SetCreation(outside, new DateTime(2026, 10, 21, 0, 0, 0));
        await WithUnitOfWorkAsync(async () =>
        {
            await repository.InsertAsync(early); await repository.InsertAsync(late); await repository.InsertAsync(outside);
        });
        var input = new GetCardRequestsInput
        {
            Filter = "  " + prefix + "  ", CreatedFrom = new DateTime(2026, 10, 20),
            CreatedTo = new DateTime(2026, 10, 20), MaxResultCount = 1
        };
        var firstPage = await service.GetListAsync(input);
        firstPage.TotalCount.ShouldBe(2);
        firstPage.Items.Count.ShouldBe(1);
        firstPage.Items[0].Id.ShouldBe(late.Id);
        input.SkipCount = 1;
        (await service.GetListAsync(input)).Items[0].Id.ShouldBe(early.Id);
        input.SkipCount = 0; input.Status = CardRequestStatus.Draft;
        var draftOnly = await service.GetListAsync(input);
        draftOnly.TotalCount.ShouldBe(1); draftOnly.Items[0].Id.ShouldBe(early.Id);
        input.Status = null; input.Filter = prefix + "-02";
        (await service.GetListAsync(input)).Items[0].Id.ShouldBe(late.Id);
        input.Filter = "أحمد " + prefix; input.OnlyMine = true;
        (await service.GetListAsync(input)).TotalCount.ShouldBe(2);
        input.NeedsAttention = true;
        (await service.GetListAsync(input)).TotalCount.ShouldBe(0);
        input.CreatedFrom = new DateTime(2026, 10, 22);
        var exception = await Should.ThrowAsync<BusinessException>(() => service.GetListAsync(input));
        exception.Code.ShouldBe("CardFlow:InvalidRequestDateRange");
    }

    private static void SetCreation(EmployeeCardRequest request, DateTime date)
    {
        typeof(EmployeeCardRequest).GetProperty(nameof(EmployeeCardRequest.CreationTime))!.SetValue(request, date);
    }
}
