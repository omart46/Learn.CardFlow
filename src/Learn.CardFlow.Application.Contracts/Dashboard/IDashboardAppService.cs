using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Learn.CardFlow.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
