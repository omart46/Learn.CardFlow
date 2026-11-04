using Volo.Abp.Modularity;

namespace Learn.CardFlow;

public abstract class CardFlowApplicationTestBase<TStartupModule> : CardFlowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
