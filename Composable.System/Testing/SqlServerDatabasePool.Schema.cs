﻿using System;
using System.Collections.Generic;
using Composable.System;
using Composable.System.Transactions;

namespace Composable.Testing
{
    sealed partial class SqlServerDatabasePool
    {
        class Database
        {
            internal int Id { get; }
            internal string Name { get; }
            internal string ConnectionString { get; }
            internal Database(SqlServerDatabasePool pool, int id)
            {
                Id = id;
                Name = $"{ManagerDbName}_{id:0000}";
                ConnectionString = pool.ConnectionStringForDbNamed(Name);
            }
        }

        static readonly HashSet<string> ConnectionStringsWithKnownManagerDb = new HashSet<string>();

        void CreateDatabase(string databaseName)
        {
            var createDatabaseCommand = $@"CREATE DATABASE [{databaseName}]";
            if(!DatabaseRootFolderOverride.IsNullOrWhiteSpace())
            {
                createDatabaseCommand += $@"
ON      ( NAME = {databaseName}_data, FILENAME = '{DatabaseRootFolderOverride}\{databaseName}.mdf') 
LOG ON  ( NAME = {databaseName}_log, FILENAME = '{DatabaseRootFolderOverride}\{databaseName}.ldf');";
            }
            _masterConnection.ExecuteNonQuery(createDatabaseCommand);

            _masterConnection.ExecuteNonQuery($"ALTER DATABASE [{databaseName}] SET RECOVERY SIMPLE;");
            //SafeConsole.WriteLine($"Created: {databaseName}");
        }

        void EnsureManagerDbExistsAndIsAvailable()
        {
            lock(typeof(SqlServerDatabasePool))
            {
                if(ConnectionStringsWithKnownManagerDb.Contains(_masterConnectionString))
                {
                    return;
                }

                try
                {
                    _managerConnection.UseConnection(_ => {});
                }
                catch(Exception exception)
                {
                    Log.Error(exception, "Failed to open manager database. Assuming it either does not exist or was on a temp drive and is now gone. Dropping everything and starting over.");
                    TransactionScopeCe.SupressAmbient(DropAllAndStartOver);
                    TransactionScopeCe.SupressAmbient(() => CreateDatabase(ManagerDbName));
                    TransactionScopeCe.SupressAmbient(() => _managerConnection.ExecuteNonQuery(CreateDbTableSql));
                    ConnectionStringsWithKnownManagerDb.Add(_masterConnectionString);
                }
            }
        }

        static class ManagerTableSchema
        {
            public static readonly string TableName = "Databases";
            public static readonly string Id = nameof(Id);
            public static readonly string IsFree = nameof(IsFree);
            public static readonly string ReservationDate = nameof(ReservationDate);
            public static readonly string ReservationCallStack = nameof(ReservationCallStack);
        }

        static readonly string CreateDbTableSql = $@"
CREATE TABLE [dbo].[{ManagerTableSchema.TableName}](
    [{ManagerTableSchema.Id}] [int] IDENTITY(1,1) NOT NULL,
	[{ManagerTableSchema.IsFree}] [bit] NOT NULL,
    [{ManagerTableSchema.ReservationDate}] [datetime] NOT NULL,
    [{ManagerTableSchema.ReservationCallStack}] [varchar](max) NOT NULL,
 CONSTRAINT [PK_DataBases] PRIMARY KEY CLUSTERED 
(
	[{ManagerTableSchema.Id}] ASC
))
";

        void DropAllAndStartOver()
        {
            _managerConnection.ClearConnectionPool();
            var dbsToDrop = new List<string>();
            _masterConnection.UseCommand(
                action: command =>
                        {
                            command.CommandText = "select name from sysdatabases";
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var dbName = reader.GetString(i: 0);
                                    if (dbName.StartsWith(ManagerDbName))
                                        dbsToDrop.Add(dbName);
                                }
                            }
                        });

            foreach (var db in dbsToDrop)
            {
                var dropCommand = $"drop database [{db}]";
                //SafeConsole.WriteLine(dropCommand);
                try
                {
                    _masterConnection.ExecuteNonQuery(dropCommand);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }
            }

            ConnectionStringsWithKnownManagerDb.Clear();
        }
    }
}