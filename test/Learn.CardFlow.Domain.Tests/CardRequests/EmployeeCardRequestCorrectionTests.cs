using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace Learn.CardFlow.CardRequests;

public class EmployeeCardRequestCorrectionTests
{
    private class OwnedRequest : EmployeeCardRequest
    {
        public OwnedRequest(Guid owner) : base(Guid.NewGuid(), "Original", "001")
        {
            CreatorId = owner;
        }
    }

    [Fact]
    public void Correction_And_Resubmission_Should_Preserve_Then_Clear_The_Reason()
    {
        var owner = Guid.NewGuid();
        var request = Rejected(owner);
        request.Correct(owner, "  Corrected Employee  ", "002");
        request.Status.ShouldBe(CardRequestStatus.Draft);
        request.NeedsResubmission.ShouldBeTrue();
        request.EmployeeName.ShouldBe("Corrected Employee");
        request.RejectionReason.ShouldBe("Incorrect number");
        Should.Throw<BusinessException>(() => request.StartReview()).Code.ShouldBe("CardFlow:ResubmissionRequired");
        request.Correct(owner, "Corrected Again", "003");
        request.Resubmit(owner);
        request.Status.ShouldBe(CardRequestStatus.UnderReview);
        request.NeedsResubmission.ShouldBeFalse();
        request.RejectionReason.ShouldBeNull();
        Should.Throw<BusinessException>(() => request.Resubmit(owner));
    }

    [Fact]
    public void Another_User_Cannot_Correct_Or_Resubmit()
    {
        var owner = Guid.NewGuid();
        var request = Rejected(owner);
        Should.Throw<BusinessException>(() => request.Correct(Guid.NewGuid(), "Other", "002"))
            .Code.ShouldBe("CardFlow:OnlyCreatorCanCorrectOrResubmit");
        request.EmployeeName.ShouldBe("Original");
        request.Status.ShouldBe(CardRequestStatus.Rejected);
        request.Correct(owner, "Updated", "002");
        Should.Throw<BusinessException>(() => request.Resubmit(Guid.NewGuid()));
        request.Status.ShouldBe(CardRequestStatus.Draft);
    }

    [Fact]
    public void Cannot_Correct_An_Ordinary_Draft_Or_Approved_Request()
    {
        var owner = Guid.NewGuid();
        var request = new OwnedRequest(owner);
        Should.Throw<BusinessException>(() => request.Correct(owner, "Updated", "002"));
        Should.Throw<BusinessException>(() => request.Resubmit(owner));
        request.StartReview();
        request.Approve();
        Should.Throw<BusinessException>(() => request.Correct(owner, "Updated", "002"));
        request.Status.ShouldBe(CardRequestStatus.Approved);
    }

    [Fact]
    public void Invalid_Correction_Should_Not_Partially_Update_The_Request()
    {
        var owner = Guid.NewGuid();
        var request = Rejected(owner);
        Should.Throw<ArgumentException>(() => request.Correct(owner, "Updated", " "));
        request.EmployeeName.ShouldBe("Original");
        request.Status.ShouldBe(CardRequestStatus.Rejected);
        request.NeedsResubmission.ShouldBeFalse();
    }

    private static OwnedRequest Rejected(Guid owner)
    {
        var request = new OwnedRequest(owner);
        request.StartReview();
        request.Reject("Incorrect number");
        return request;
    }
}
