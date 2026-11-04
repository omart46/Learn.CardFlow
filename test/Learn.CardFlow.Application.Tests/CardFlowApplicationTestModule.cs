using Volo.Abp.Modularity;

namespace Learn.CardFlow;

[DependsOn(
    typeof(CardFlowApplicationModule),
    typeof(CardFlowDomainTestModule)
)]
public class CardFlowApplicationTestModule : AbpModule
{

}
