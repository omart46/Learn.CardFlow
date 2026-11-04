using Volo.Abp.Settings;

namespace Learn.CardFlow.Settings;

public class CardFlowSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CardFlowSettings.MySetting1));
    }
}
