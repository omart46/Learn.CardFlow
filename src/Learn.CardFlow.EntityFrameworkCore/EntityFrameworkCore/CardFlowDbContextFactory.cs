using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Learn.CardFlow.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class CardFlowDbContextFactory : IDesignTimeDbContextFactory<CardFlowDbContext>
{
    public CardFlowDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        CardFlowEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<CardFlowDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new CardFlowDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Learn.CardFlow.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.secrets.json", optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
