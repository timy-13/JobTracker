using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobTracker.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace JobTracker.Data
{
    public class JobTrackerContext : IdentityDbContext<IdentityUser>
    {
        public JobTrackerContext (DbContextOptions<JobTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<JobTracker.Models.Job> Job { get; set; } = default!;
    }
}
