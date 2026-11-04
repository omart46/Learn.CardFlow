using Volo.Abp.Modularity;

namespace Learn.CardFlow;

[DependsOn(
    typeof(CardFlowDomainModule),
    typeof(CardFlowTestBaseModule)
)]
public class CardFlowDomainTestModule : AbpModule
{

}
