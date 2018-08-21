using MySql.Data.Entity;
using RequestAutoV1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Web;

namespace RequestAutoV1.Data
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class DataBaseContext : DbContext
    {
        public DataBaseContext() : base("conn")
        {
           Database.SetInitializer(new MySQLDatabaseInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<User>().Property(x => x.Age).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute { }));


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Result> results { get; set; }
        public DbSet<Search> searches { get; set; }
    }

}