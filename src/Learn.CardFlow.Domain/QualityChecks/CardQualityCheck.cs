using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Learn.CardFlow.QualityChecks;

public class CardQualityCheck : FullAuditedAggregateRoot<Guid>
{
    public Guid EmployeeCardRequestId { get; private set; }

    public Guid ProductionBatchId { get; private set; }

    public CardQualityStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    protected CardQualityCheck()
    {
    }

    public CardQualityCheck(Guid id, Guid employeeCardRequestId, Guid productionBatchId)
        : base(id)
    {
        if (employeeCardRequestId == Guid.Empty || productionBatchId == Guid.Empty)
        {
            throw new BusinessException("CardFlow:QualityCheckReferencesRequired");
        }

        EmployeeCardRequestId = employeeCardRequestId;
        ProductionBatchId = productionBatchId;
        Status = CardQualityStatus.Pending;
    }

    public void Pass()
    {
        EnsurePending();
        Status = CardQualityStatus.Passed;
    }

    public void Fail(string reason)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException("CardFlow:QualityFailureReasonRequired");
        }

        reason = reason.Trim();
        if (reason.Length > 500)
        {
            throw new BusinessException("CardFlow:QualityFailureReasonTooLong");
        }

        FailureReason = reason;
        Status = CardQualityStatus.Failed;
    }

    private void EnsurePending()
    {
        if (Status != CardQualityStatus.Pending)
        {
            throw new BusinessException("CardFlow:QualityCheckAlreadyCompleted");
        }
    }
}
