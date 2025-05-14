using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResponseApp.Data;
using ResponseApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResponseApp.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public List<Course> Courses { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
        public List<ResponseApp.Models.Response> Responses { get; set; } = new();

        [BindProperty(SupportsGet = true)] public int? SelectedCourseId { get; set; }
        [BindProperty(SupportsGet = true)] public int? SelectedAssignmentId { get; set; }
        [BindProperty(SupportsGet = true)] public int? SelectedResponseId { get; set; }

        [BindProperty] public string NewCourseName { get; set; } = "";
        [BindProperty] public string NewCourseDescription { get; set; } = "";

        [BindProperty] public string NewAssignmentTitle { get; set; } = "";
        [BindProperty] public string NewAssignmentDescription { get; set; } = "";
        [BindProperty] public int NewAssignmentPosition { get; set; } = 0;

        [BindProperty] public string NewResponseTitle { get; set; } = "";
        [BindProperty] public string NewResponseText { get; set; } = "";

        public string ResponseText { get; set; }

        public void OnGet()
        {
            LoadData();
        }

        public async Task<IActionResult> OnPostAddCourseAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCourseName))
            {
                TempData["StatusMessage"] = "Course name required.";
                return RedirectToPage();
            }

            var db = GetExternalDb();
            if (db == null) return RedirectToPage();

            db.Courses.Add(new Course
            {
                CourseName = NewCourseName,
                CourseDescription = NewCourseDescription ?? ""
            });
            await db.SaveChangesAsync();

            TempData["StatusMessage"] = "Course added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddAssignmentAsync()
        {
            if (!SelectedCourseId.HasValue || string.IsNullOrWhiteSpace(NewAssignmentTitle))
            {
                TempData["StatusMessage"] = "Course not selected or title is empty.";
                return RedirectToPage(new { SelectedCourseId });
            }

            var db = GetExternalDb();
            if (db == null) return RedirectToPage();

            db.Assignments.Add(new Assignment
            {
                CourseId = SelectedCourseId.Value,
                AssignmentTitle = NewAssignmentTitle,
                AssignmentDescription = NewAssignmentDescription ?? "",
                AssignmentPosition = NewAssignmentPosition
            });
            await db.SaveChangesAsync();

            TempData["StatusMessage"] = "Assignment added.";
            return RedirectToPage(new { SelectedCourseId });
        }

        public async Task<IActionResult> OnPostAddResponseAsync()
        {
            if (!SelectedAssignmentId.HasValue || string.IsNullOrWhiteSpace(NewResponseTitle))
            {
                TempData["StatusMessage"] = "Assignment not selected or title is missing.";
                return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId });
            }

            var db = GetExternalDb();
            if (db == null) return RedirectToPage();

            db.Responses.Add(new ResponseApp.Models.Response
            {
                AssignmentId = SelectedAssignmentId.Value,
                ResponseTitle = NewResponseTitle,
                ResponseText = NewResponseText
            });
            await db.SaveChangesAsync();

            TempData["StatusMessage"] = "Response added.";
            return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId });
        }

        public async Task<IActionResult> OnPostDeleteResponseAsync()
        {
            if (!SelectedResponseId.HasValue)
            {
                TempData["StatusMessage"] = "No response selected.";
                return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId });
            }

            var db = GetExternalDb();
            if (db == null) return RedirectToPage();

            var response = await db.Responses.FindAsync(SelectedResponseId.Value);
            if (response != null)
            {
                db.Responses.Remove(response);
                await db.SaveChangesAsync();
            }

            TempData["StatusMessage"] = "Response deleted.";
            return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId });
        }

        public async Task<IActionResult> OnPostEditResponseAsync()
        {
            if (!SelectedResponseId.HasValue)
            {
                TempData["StatusMessage"] = "No response selected.";
                return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId });
            }

            var db = GetExternalDb();
            if (db == null) return RedirectToPage();

            var response = await db.Responses.FindAsync(SelectedResponseId.Value);
            if (response != null)
            {
                response.ResponseTitle = NewResponseTitle;
                response.ResponseText = NewResponseText;
                await db.SaveChangesAsync();
            }

            TempData["StatusMessage"] = "Response updated.";
            return RedirectToPage(new { SelectedCourseId, SelectedAssignmentId, SelectedResponseId });
        }

        private void LoadData()
        {
            var path = _appDbContext.SystemSettings.AsNoTracking()
                         .FirstOrDefault(s => s.Key == "ExternalDbPath")?.Value;
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return;

            var opts = new DbContextOptionsBuilder<ExternalDbContext>();
            opts.UseSqlite($"Data Source={path}");
            using var db = new ExternalDbContext(opts.Options);

            Courses = db.Courses.ToList();

            if (SelectedCourseId.HasValue)
            {
                Assignments = db.Assignments.Where(a => a.CourseId == SelectedCourseId.Value).ToList();
            }

            if (SelectedAssignmentId.HasValue)
            {
                Responses = db.Responses.Where(r => r.AssignmentId == SelectedAssignmentId.Value).ToList();
            }

            if (SelectedResponseId.HasValue)
            {
                ResponseText = db.Responses.FirstOrDefault(r => r.Id == SelectedResponseId.Value)?.ResponseText ?? "";
            }
        }

        private ExternalDbContext? GetExternalDb()
        {
            var path = _appDbContext.SystemSettings.AsNoTracking()
                         .FirstOrDefault(s => s.Key == "ExternalDbPath")?.Value;
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return null;

            var optionsBuilder = new DbContextOptionsBuilder<ExternalDbContext>();
            optionsBuilder.UseSqlite($"Data Source={path}");
            return new ExternalDbContext(optionsBuilder.Options);
        }
    }
}
