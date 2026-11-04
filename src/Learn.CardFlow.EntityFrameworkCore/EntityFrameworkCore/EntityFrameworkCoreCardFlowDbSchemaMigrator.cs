using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Learn.CardFlow.Data;
using Volo.Abp.DependencyInjection;

namespace Learn.CardFlow.EntityFrameworkCore;

public class EntityFrameworkCoreCardFlowDbSchemaMigrator
    : ICardFlowDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreCardFlowDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the CardFlowDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<CardFlowDbContext>()
            .Database
            .MigrateAsync();
    }
}
