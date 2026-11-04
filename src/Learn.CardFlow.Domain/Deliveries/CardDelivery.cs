using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Learn.CardFlow.Deliveries;

public class CardDelivery : FullAuditedAggregateRoot<Guid>
{
    public Guid EmployeeCardRequestId { get; private set; }
    public Guid CardQualityCheckId { get; private set; }
    public string RecipientName { get; private set; } = string.Empty;
    public DateTime DeliveredAt { get; private set; }
    public Guid DeliveredById { get; private set; }

    protected CardDelivery() { }

    public CardDelivery(Guid id, Guid requestId, Guid qualityCheckId,
        string recipientName, DateTime deliveredAt, Guid deliveredById) : base(id)
    {
        if (requestId == Guid.Empty || qualityCheckId == Guid.Empty || deliveredById == Guid.Empty)
            throw new BusinessException("CardFlow:DeliveryReferencesRequired");
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new BusinessException("CardFlow:RecipientNameRequired");
        recipientName = recipientName.Trim();
        if (recipientName.Length > 128)
            throw new BusinessException("CardFlow:RecipientNameTooLong");

        EmployeeCardRequestId = requestId;
        CardQualityCheckId = qualityCheckId;
        RecipientName = recipientName;
        DeliveredAt = deliveredAt;
        DeliveredById = deliveredById;
    }
}
