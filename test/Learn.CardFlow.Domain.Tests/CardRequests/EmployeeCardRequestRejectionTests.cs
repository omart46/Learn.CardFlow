using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Learn.CardFlow.CardRequests;

public class EmployeeCardRequestRejectionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_Reason_Should_Not_Change_The_Request(string? reason)
    {
        var request = CreateUnderReview();
        var error = Should.Throw<BusinessException>(() => request.Reject(reason!));
        error.Code.ShouldBe("CardFlow:RejectionReasonRequired");
        request.Status.ShouldBe(CardRequestStatus.UnderReview);
        request.RejectionReason.ShouldBeNull();
    }

    [Fact]
    public void Oversized_Reason_Should_Not_Change_The_Request()
    {
        var request = CreateUnderReview();
        var error = Should.Throw<BusinessException>(() => request.Reject(new string('x', 501)));
        error.Code.ShouldBe("CardFlow:RejectionReasonTooLong");
        request.Status.ShouldBe(CardRequestStatus.UnderReview);
        request.RejectionReason.ShouldBeNull();
    }

    [Fact]
    public void Should_Trim_And_Save_The_Reason()
    {
        var request = CreateUnderReview();
        request.Reject("  Employee number needs correction.  ");
        request.Status.ShouldBe(CardRequestStatus.Rejected);
        request.RejectionReason.ShouldBe("Employee number needs correction.");
        Should.Throw<BusinessException>(() => request.Reject("Another reason"));
        request.RejectionReason.ShouldBe("Employee number needs correction.");
    }

    [Fact]
    public void Should_Accept_Exactly_500_Characters()
    {
        var request = CreateUnderReview();
        request.Reject(new string('x', 500));
        request.RejectionReason!.Length.ShouldBe(500);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Should_Not_Reject_A_Draft_Or_Approved_Request(bool approved)
    {
        var request = new EmployeeCardRequest(Guid.NewGuid(), "Test Employee", "001");
        if (approved) { request.StartReview(); request.Approve(); }
        var error = Should.Throw<BusinessException>(() => request.Reject("Needs correction"));
        error.Code.ShouldBe("CardFlow:CardRequestMustBeUnderReviewToReject");
        request.RejectionReason.ShouldBeNull();
    }

    private static EmployeeCardRequest CreateUnderReview()
    {
        var request = new EmployeeCardRequest(Guid.NewGuid(), "Test Employee", "001");
        request.StartReview();
        return request;
    }
}
