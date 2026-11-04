using Learn.CardFlow.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Learn.CardFlow.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class CardFlowController : AbpControllerBase
{
    protected CardFlowController()
    {
        LocalizationResource = typeof(CardFlowResource);
    }
}
