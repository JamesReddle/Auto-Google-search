namespace RequestAutoV1.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Collections.Generic;
    using RequestAutoV1.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RequestAutoV1.Data.DataBaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RequestAutoV1.Data.DataBaseContext context)
        {

            /*if (!context.searches.Any())
            {

            }*/
            /*Ќаполнение базы данных при обновлении модели*/
        }
    }
}
