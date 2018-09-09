using System.Configuration;
using Composable.System.Data.SqlClient;

namespace Composable.System.Configuration
{
    ///<summary>Supplies connection strings from the application configuration file.</summary>
    class AppConfigSqlConnectionProviderSource : ISqlConnectionProviderSource
    {
        ///<summary>Returns the connection string with the given name.</summary>
        public Data.SqlClient.ISqlConnectionProvider GetConnectionProvider(string parameterName)
        {
            //var parameter = ConfigurationManager.ConnectionStrings[parameterName];
            //if (parameter == null)
            //{
            //    throw new ConfigurationErrorsException($"ConnectionString with name {parameterName} does not exists");
            //}
            return new SqlServerConnectionProvider("Server=tcp:composable.database.windows.net,1433;Initial Catalog=composable;Persist Security Info=False;User ID=richard;Password=;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}
