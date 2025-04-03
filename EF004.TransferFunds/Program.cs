using EF004.TransferFunds;
using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using System;

using var session = CreateSession();
using var transaction = session.BeginTransaction();

var idFrom = 3;
var idTo = 2;
var amountToTransfer = 1000;

try 
{
    var walletFrom = session.Get<Wallet>(idFrom);
    var walletTo = session.Get<Wallet>(idTo);

    if (walletFrom == null || walletTo == null)
    {
        throw new Exception("One or both wallets not found");
    }

    if (walletFrom.Balance < amountToTransfer)
    {
        throw new Exception("Insufficient funds for transfer");
    }

    Console.WriteLine($"Starting transfer of ${amountToTransfer} from wallet {idFrom} to wallet {idTo}");
    Console.WriteLine($"Initial balance - From Wallet: ${walletFrom.Balance}, To Wallet: ${walletTo.Balance}");

    walletFrom.Balance -= amountToTransfer;
    walletTo.Balance += amountToTransfer;

    session.Update(walletFrom);
    session.Update(walletTo);

    transaction.Commit();

    Console.WriteLine($"Transfer completed successfully!");
    Console.WriteLine($"Final balance - From Wallet: ${walletFrom.Balance}, To Wallet: ${walletTo.Balance}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: Transfer failed!");
    Console.WriteLine($"Reason: {ex.Message}");
    transaction.Rollback();
}
finally
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

ISession CreateSession()
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    var constr = config.GetSection("constr").Value;

    var mapper = new ModelMapper();

    // list all of type mappings from assembly

    mapper.AddMappings(typeof(Wallet).Assembly.ExportedTypes);

    // Compile class mapping
    HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

    // optional
    // Console.WriteLine(domainMapping.AsString());

    // allow the application to specify propertties and mapping documents
    // to be used when creating

    var hbConfig = new Configuration();

    // settings from app to nhibernate 
    hbConfig.DataBaseIntegration(c =>
    {
        // strategy to interact with provider
        c.Driver<MicrosoftDataSqlClientDriver>();

        // dialect nhibernate uses to build syntaxt to rdbms
        c.Dialect<MsSql2012Dialect>();

        // connection string
        c.ConnectionString = constr;

        // log sql statement to console
        // c.LogSqlInConsole = true;

        // format logged sql statement
        // c.LogFormattedSql = true; 
    });

    // add mapping to nhiberate configuration
    hbConfig.AddMapping(domainMapping);

    // instantiate a new IsessionFactory (use properties, settings and mapping)
    var sessionFactory = hbConfig.BuildSessionFactory();

    var session = sessionFactory.OpenSession();

    return session;
}