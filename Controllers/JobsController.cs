using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobTracker.Data;
using JobTracker.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;


namespace JobTracker.Controllers
{
    [Authorize]
    public class JobsController : Controller
    {
        private readonly JobTrackerContext _context;
        private readonly ILogger<JobsController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public JobsController(JobTrackerContext context, ILogger<JobsController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Jobs
        public async Task<IActionResult> Index(string companySearchString, string descriptionSearchString)
        {
            if (_context.Job == null)
            {
                _logger.LogError("Entity set 'JobTrackerContext.Job' is null.");
                return Problem("Entity set 'JobTrackerContext.Job' is null.");
            }

            var jobs = from m in _context.Job
                       where m.UserId == _userManager.GetUserId(User)
                       select m;

            if (!String.IsNullOrEmpty(companySearchString))
            {
                _logger.LogInformation("Filtering jobs by company: {companySearchString}", companySearchString);
                jobs = jobs.Where(s => s.Company!.ToUpper().Contains(companySearchString.ToUpper()));
            }

            if (!String.IsNullOrEmpty(descriptionSearchString))
            {
                _logger.LogInformation("Filtering jobs by description: {descriptionSearchString}", descriptionSearchString);
                jobs = jobs.Where(s => s.Description!.ToUpper().Contains(descriptionSearchString.ToUpper()));
            }

            var jobList = await jobs.ToListAsync();
            _logger.LogInformation("Fetched {jobCount} jobs.", jobList.Count());
            return View(jobList);
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogError("Job id not found.");
                return NotFound();
            }

            var job = await _context.Job
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                _logger.LogError("Job with id {id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Fetched job details for job with id {id}.", id);
            return View(job);
        }

        // GET: Jobs/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Entered Create method.");
            return View();
        }

        // POST: Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Company,Description,Link,ApplicationDate,Status")] Job job)
        {
            _logger.LogInformation("Entered Create POST method with job: {job}", job);

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                job.UserId = userId;
                ModelState.Remove("UserId");
                _logger.LogInformation("Job UserId set: {userId}", userId);
            }
            else
            {
                _logger.LogWarning("User Id can not be found");
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Job with id {id} created successfully.", job.Id);
                return RedirectToAction(nameof(Index));          
            }


            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Model Error: " + error.ErrorMessage);
            }

            _logger.LogWarning("Model state is invalid for job: {job}", job);
            return View(job);
        }

        // GET: Jobs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("Entered Edit method with id: {id}", id);

            if (id == null)
            {
                _logger.LogWarning("Job id not found.");
                return NotFound();
            }

            var job = await _context.Job.FindAsync(id);
            if (job == null)
            {
                _logger.LogWarning("Job with id {id} not found.", id);
                return NotFound();
            }
            return View(job);
        }

        // POST: Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Company,Description,Link,ApplicationDate,Status")] Job job)
        {
            _logger.LogInformation("Entered Edit POST method for job with id: {id}", id);

            if (id != job.Id)
            {
                _logger.LogWarning("Job id mismatch: {id} does not match {jobId}.", id, job.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    _logger.LogInformation("Job with id {id} updated successfully.", job.Id);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!JobExists(job.Id))
                    {
                        _logger.LogError("Job with id {id} does not exist.", job.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Error updating job with id {id}.", job.Id);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid for job with id: {id}", job.Id);
            return View(job);
        }

        // GET: Jobs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            _logger.LogInformation("Entered Delete method with id: {id}", id);

            if (id == null)
            {
                _logger.LogError("Job id not found.");
                return NotFound();
            }

            var job = await _context.Job
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                _logger.LogWarning("Job with id {id} not found.", id);
                return NotFound();
            }

            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Entered DeleteConfirmed method with id: {id}", id);

            var job = await _context.Job.FindAsync(id);
            if (job != null)
            {
                _context.Job.Remove(job);
                _logger.LogInformation("Job with id {id} deleted successfully.", id);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            _logger.LogInformation("Checking if job with id {id} exists.", id);
            return _context.Job.Any(e => e.Id == id);
        }
    }
}
