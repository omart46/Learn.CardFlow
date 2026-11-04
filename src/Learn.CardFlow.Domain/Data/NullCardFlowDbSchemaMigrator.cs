using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Learn.CardFlow.Data;

/* This is used if database provider does't define
 * ICardFlowDbSchemaMigrator implementation.
 */
public class NullCardFlowDbSchemaMigrator : ICardFlowDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
