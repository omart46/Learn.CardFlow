using System;
using Learn.CardFlow.CardRequests;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Learn.CardFlow.ProductionBatches;

public class ProductionBatchTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Empty_Batch_Cannot_Start(int count)
    {
        var batch = new ProductionBatch(Guid.NewGuid(), "Test Batch");
        Should.Throw<BusinessException>(() => batch.StartProduction(count));
        batch.Status.ShouldBe(ProductionBatchStatus.Draft);
    }

    [Fact]
    public void Production_Should_Lock_The_Batch_And_Complete_Only_Once()
    {
        var batch = new ProductionBatch(Guid.NewGuid(), "Test Batch");
        Should.Throw<BusinessException>(() => batch.CompleteProduction());
        batch.StartProduction(1);
        Should.Throw<BusinessException>(() => batch.EnsureCanModifyRequests());
        Should.Throw<BusinessException>(() => batch.StartProduction(1));
        batch.CompleteProduction();
        batch.Status.ShouldBe(ProductionBatchStatus.Completed);
        Should.Throw<BusinessException>(() => batch.CompleteProduction());
    }

    [Fact]
    public void Assignment_Requires_Approval_And_Cannot_Replace_An_Existing_Batch()
    {
        var request = new EmployeeCardRequest(Guid.NewGuid(), "Test Employee", "001");
        var batchId = Guid.NewGuid();
        Should.Throw<BusinessException>(() => request.AssignToBatch(batchId));
        request.StartReview();
        request.Approve();
        Should.Throw<BusinessException>(() => request.AssignToBatch(Guid.Empty));
        request.AssignToBatch(batchId);
        Should.Throw<BusinessException>(() => request.AssignToBatch(Guid.NewGuid()));
        request.ProductionBatchId.ShouldBe(batchId);
        Should.Throw<BusinessException>(() => request.RemoveFromBatch(Guid.NewGuid()));
        request.ProductionBatchId.ShouldBe(batchId);
        request.RemoveFromBatch(batchId);
        request.ProductionBatchId.ShouldBeNull();
        request.Status.ShouldBe(CardRequestStatus.Approved);
    }
}
