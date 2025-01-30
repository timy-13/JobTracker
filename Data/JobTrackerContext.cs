using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobTracker.Models;

namespace JobTracker.Data
{
    public class JobTrackerContext : DbContext
    {
        public JobTrackerContext (DbContextOptions<JobTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<JobTracker.Models.Job> Job { get; set; } = default!;
    }
}
