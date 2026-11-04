using System;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.CardRequests;

public class EmployeeCardRequestDto : EntityDto<Guid>
{
    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public CardRequestStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public bool IsMine { get; set; }

    public bool NeedsResubmission { get; set; }

    public DateTime CreationTime { get; set; }
}
