using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RequestAutoV1.Data
{
    class MySQLDatabaseInitializer : MigrateDatabaseToLatestVersion<DataBaseContext, Migrations.Configuration> //Дефалтная заглушка для наследования от нужного класса.
    {
    }
}