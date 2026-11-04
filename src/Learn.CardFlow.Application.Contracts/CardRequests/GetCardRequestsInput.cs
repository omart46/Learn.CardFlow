using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Learn.CardFlow.CardRequests;

public class GetCardRequestsInput : PagedAndSortedResultRequestDto
{
    public bool OnlyMine { get; set; }
    public bool NeedsAttention { get; set; }

    [StringLength(128)]
    public string? Filter { get; set; }

    [EnumDataType(typeof(CardRequestStatus))]
    public CardRequestStatus? Status { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
