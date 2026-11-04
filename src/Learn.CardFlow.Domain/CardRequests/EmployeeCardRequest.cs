using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Learn.CardFlow.CardRequests;

public class EmployeeCardRequest : FullAuditedAggregateRoot<Guid>
{
    public string EmployeeName { get; private set; } = string.Empty;

    public string EmployeeNumber { get; private set; } = string.Empty;

    public CardRequestStatus Status { get; private set; }
    public Guid? ProductionBatchId { get; private set; }

    public string? RejectionReason { get; private set; }

    public bool NeedsResubmission { get; private set; }

    protected EmployeeCardRequest()
    {
    }

    public EmployeeCardRequest(
        Guid id,
        string employeeName,
        string employeeNumber)
        : base(id)
    {
        EmployeeName = employeeName;
        EmployeeNumber = employeeNumber;
        Status = CardRequestStatus.Draft;
    }

    public void StartReview()
    {
        if (NeedsResubmission)
        {
            throw new BusinessException("CardFlow:ResubmissionRequired");
        }
        if (Status != CardRequestStatus.Draft)
        {
            throw new BusinessException(
                "CardFlow:CardRequestMustBeDraftToStartReview"
            );
        }

        Status = CardRequestStatus.UnderReview;
    }

    public void Correct(Guid actorId, string employeeName, string employeeNumber)
    {
        EnsureCreator(actorId);
        if (Status != CardRequestStatus.Rejected && !(Status == CardRequestStatus.Draft && NeedsResubmission))
        {
            throw new BusinessException("CardFlow:OnlyRejectedRequestsCanBeCorrected");
        }

        employeeName = Check.NotNullOrWhiteSpace(employeeName, nameof(employeeName), maxLength: 128).Trim();
        employeeNumber = Check.NotNullOrWhiteSpace(employeeNumber, nameof(employeeNumber), maxLength: 32).Trim();
        EmployeeName = employeeName;
        EmployeeNumber = employeeNumber;
        Status = CardRequestStatus.Draft;
        NeedsResubmission = true;
    }

    public void Resubmit(Guid actorId)
    {
        EnsureCreator(actorId);
        if (Status != CardRequestStatus.Draft || !NeedsResubmission)
        {
            throw new BusinessException("CardFlow:OnlyCorrectedRequestsCanBeResubmitted");
        }

        Status = CardRequestStatus.UnderReview;
        RejectionReason = null;
        NeedsResubmission = false;
    }

    private void EnsureCreator(Guid actorId)
    {
        if (actorId == Guid.Empty || CreatorId != actorId)
        {
            throw new BusinessException("CardFlow:OnlyCreatorCanCorrectOrResubmit");
        }
    }
    public void Approve()
    {
        if (Status != CardRequestStatus.UnderReview)
        {
            throw new BusinessException(
                "CardFlow:CardRequestMustBeUnderReviewToApprove"
            );
        }

        Status = CardRequestStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != CardRequestStatus.UnderReview)
        {
            throw new BusinessException(
                "CardFlow:CardRequestMustBeUnderReviewToReject"
            );
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException("CardFlow:RejectionReasonRequired");
        }

        reason = reason.Trim();
        if (reason.Length > 500)
        {
            throw new BusinessException("CardFlow:RejectionReasonTooLong");
        }

        RejectionReason = reason;
        Status = CardRequestStatus.Rejected;
    }
    public void AssignToBatch(Guid batchId)
    {
        if (batchId == Guid.Empty)
        {
            throw new BusinessException(
                "CardFlow:BatchIdRequired"
            );
        }

        if (Status != CardRequestStatus.Approved)
        {
            throw new BusinessException(
                "CardFlow:OnlyApprovedRequestsCanBeBatched"
            );
        }

        if (ProductionBatchId.HasValue)
        {
            throw new BusinessException(
                "CardFlow:RequestAlreadyAssignedToBatch"
            );
        }

        ProductionBatchId = batchId;
    }
    public void RemoveFromBatch(Guid batchId)
    {
        if (batchId == Guid.Empty)
        {
            throw new BusinessException(
                "CardFlow:BatchIdRequired"
            );
        }

        if (ProductionBatchId != batchId)
        {
            throw new BusinessException(
                "CardFlow:RequestDoesNotBelongToBatch"
            );
        }

        ProductionBatchId = null;
    }
}
