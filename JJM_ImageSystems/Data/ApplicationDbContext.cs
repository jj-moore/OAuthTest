using System;
using System.Collections.Generic;
using System.Text;
using JJM_ImageSystems.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JJM_ImageSystems.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<PowerSchool> PowerSchool { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }
    }
}
