using Learn.CardFlow.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Learn.CardFlow.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CardFlowEntityFrameworkCoreModule),
    typeof(CardFlowApplicationContractsModule)
)]
public class CardFlowDbMigratorModule : AbpModule
{
}
