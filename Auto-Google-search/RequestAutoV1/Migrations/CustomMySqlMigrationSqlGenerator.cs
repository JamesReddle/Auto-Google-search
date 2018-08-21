using MySql.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Linq;
using System.Web;

namespace RequestAutoV1.Migrations
{
    public class CustomMySqlMigrationSqlGenerator : MySqlMigrationSqlGenerator
    {
        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            IEnumerable<MigrationStatement> statements = base.Generate(migrationOperations, providerManifestToken);
            foreach (MigrationStatement statement in statements)
            {
                if (!statement.Sql.EndsWith(";"))
                {
                    statement.Sql = statement.Sql.TrimEnd() + ";";
                }
            }
            return statements;
        }
    }
}