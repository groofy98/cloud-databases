using DAL.Configurations;
using Microsoft.EntityFrameworkCore;
using Models;
using System;

namespace DAL
{
    public class CloudDBContext : DbContext
    {
        public DbSet<House> Houses { get; set; }
        public DbSet<Applicant> Applicants {  get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                Environment.GetEnvironmentVariable("CosmosDBUrl"),
                Environment.GetEnvironmentVariable("ConnectionString"),
                databaseName: Environment.GetEnvironmentVariable("DatabaseName"));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Store");            

            modelBuilder.ApplyConfiguration(new HouseConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicantConfiguration());
            
        }
    }
}
