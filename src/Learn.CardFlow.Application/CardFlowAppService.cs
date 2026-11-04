using Learn.CardFlow.Localization;
using Volo.Abp.Application.Services;

namespace Learn.CardFlow;

/* Inherit your application services from this class.
 */
public abstract class CardFlowAppService : ApplicationService
{
    protected CardFlowAppService()
    {
        LocalizationResource = typeof(CardFlowResource);
    }
}
