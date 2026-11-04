using System.Threading.Tasks;

namespace Learn.CardFlow.Data;

public interface ICardFlowDbSchemaMigrator
{
    Task MigrateAsync();
}
