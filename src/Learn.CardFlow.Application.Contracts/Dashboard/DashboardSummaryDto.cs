using System;

namespace Learn.CardFlow.Dashboard;

public class DashboardSummaryDto
{
    public DateTime GeneratedAt { get; set; }

    public CardRequestsSummaryDto? CardRequests { get; set; }

    public ProductionSummaryDto? Production { get; set; }

    public QualitySummaryDto? Quality { get; set; }

    public DeliverySummaryDto? Delivery { get; set; }
}

public class CardRequestsSummaryDto
{
    public long Draft { get; set; }
    public long UnderReview { get; set; }
    public long Approved { get; set; }
    public long Rejected { get; set; }
}

public class ProductionSummaryDto
{
    public long Draft { get; set; }
    public long InProduction { get; set; }
    public long Completed { get; set; }
}

public class QualitySummaryDto
{
    public long Pending { get; set; }
    public long Passed { get; set; }
    public long Failed { get; set; }
}

public class DeliverySummaryDto
{
    public long ReadyForDelivery { get; set; }
    public long Delivered { get; set; }
}
