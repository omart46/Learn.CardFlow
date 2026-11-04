using Microsoft.Extensions.Localization;
using Learn.CardFlow.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Learn.CardFlow;

[Dependency(ReplaceServices = true)]
public class CardFlowBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<CardFlowResource> _localizer;

    public CardFlowBrandingProvider(IStringLocalizer<CardFlowResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
