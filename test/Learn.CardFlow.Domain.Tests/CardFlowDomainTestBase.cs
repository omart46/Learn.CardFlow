using Volo.Abp.Modularity;

namespace Learn.CardFlow;

/* Inherit from this class for your domain layer tests. */
public abstract class CardFlowDomainTestBase<TStartupModule> : CardFlowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
