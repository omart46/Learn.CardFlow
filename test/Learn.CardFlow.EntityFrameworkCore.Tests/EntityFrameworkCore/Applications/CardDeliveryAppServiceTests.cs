using System;
using System.Threading.Tasks;
using Learn.CardFlow.CardRequests;
using Learn.CardFlow.Deliveries;
using Learn.CardFlow.ProductionBatches;
using Learn.CardFlow.QualityChecks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Learn.CardFlow.EntityFrameworkCore.Applications;

[Collection(CardFlowTestConsts.CollectionDefinitionName)]
public class CardDeliveryAppServiceTests : CardFlowEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Only_Passed_Cards_Should_Be_Delivered_And_Receipt_Should_Be_Final()
    {
        var requests = GetRequiredService<IRepository<EmployeeCardRequest, Guid>>();
        var batches = GetRequiredService<IRepository<ProductionBatch, Guid>>();
        var checks = GetRequiredService<IRepository<CardQualityCheck, Guid>>();
        var deliveries = GetRequiredService<IRepository<CardDelivery, Guid>>();
        var service = GetRequiredService<ICardDeliveryAppService>();
        var manager = GetRequiredService<CardDeliveryManager>();
        var batch = new ProductionBatch(Guid.NewGuid(), "Delivery test");
        var request = NewRequest(batch.Id, "D-001");
        var failedRequest = NewRequest(batch.Id, "D-002");
        var pendingRequest = NewRequest(batch.Id, "D-003");
        batch.StartProduction(3); batch.CompleteProduction();
        var passed = new CardQualityCheck(Guid.NewGuid(), request.Id, batch.Id); passed.Pass();
        var failed = new CardQualityCheck(Guid.NewGuid(), failedRequest.Id, batch.Id); failed.Fail("Print issue");
        var pending = new CardQualityCheck(Guid.NewGuid(), pendingRequest.Id, batch.Id);
        await WithUnitOfWorkAsync(async () =>
        {
            await batches.InsertAsync(batch);
            await requests.InsertAsync(request); await requests.InsertAsync(failedRequest); await requests.InsertAsync(pendingRequest);
            await checks.InsertAsync(passed); await checks.InsertAsync(failed); await checks.InsertAsync(pending);
        });
        var page = new PagedAndSortedResultRequestDto { MaxResultCount = 100 };
        var ready = await service.GetReadyCardsAsync(page);
        ready.Items.ShouldContain(card => card.Id == passed.Id && card.EmployeeNumber == "D-001");
        ready.Items.ShouldNotContain(card => card.Id == failed.Id || card.Id == pending.Id);

        foreach (var check in new[] { failed, pending })
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(
                new RecordCardDeliveryDto { CardQualityCheckId = check.Id, RecipientName = "Recipient" }));
            exception.Code.ShouldBe("CardFlow:OnlyPassedCardsCanBeDelivered");
        }
        await WithUnitOfWorkAsync(async () =>
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => manager.CreateAsync(passed.Id, "   ", Guid.NewGuid()));
            exception.Code.ShouldBe("CardFlow:RecipientNameRequired");
        });
        var before = DateTime.UtcNow.AddMinutes(-1);
        var receipt = await service.CreateAsync(new RecordCardDeliveryDto { CardQualityCheckId = passed.Id, RecipientName = "  Card Recipient  " });
        receipt.RecipientName.ShouldBe("Card Recipient");
        receipt.EmployeeNumber.ShouldBe("D-001"); receipt.BatchName.ShouldBe(batch.Name);
        receipt.DeliveredById.ShouldNotBe(Guid.Empty);
        receipt.DeliveredAt.ToUniversalTime().ShouldBeGreaterThan(before);
        (await service.GetReadyCardsAsync(page)).Items.ShouldNotContain(card => card.Id == passed.Id);
        (await service.GetListAsync(page)).Items.ShouldContain(item => item.Id == receipt.Id);
        (await service.GetAsync(receipt.Id)).RecipientName.ShouldBe("Card Recipient");
        var duplicate = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(
            new RecordCardDeliveryDto { CardQualityCheckId = passed.Id, RecipientName = "Another recipient" }));
        duplicate.Code.ShouldBe("CardFlow:CardAlreadyDelivered");

        await WithUnitOfWorkAsync(() => deliveries.DeleteAsync(receipt.Id));
        (await service.GetReadyCardsAsync(page)).Items.ShouldNotContain(card => card.Id == passed.Id);
        await WithUnitOfWorkAsync(async () =>
        {
            var archived = await Should.ThrowAsync<BusinessException>(() => manager.CreateAsync(passed.Id, "Recipient", Guid.NewGuid()));
            archived.Code.ShouldBe("CardFlow:CardAlreadyDelivered");
        });
    }

    private static EmployeeCardRequest NewRequest(Guid batchId, string number)
    {
        var request = new EmployeeCardRequest(Guid.NewGuid(), "Delivery employee", number);
        request.StartReview(); request.Approve(); request.AssignToBatch(batchId);
        return request;
    }
}
