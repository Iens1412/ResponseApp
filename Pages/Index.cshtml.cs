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

        public string ResponseText { get; set; } = "";

        public void OnGet() => LoadData();

        public IActionResult OnGetPartial()
        {
            LoadData();
            return Partial("_DashboardContent", this);
        }

        public async Task<IActionResult> OnPostAddCourseAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCourseName)) return BadRequest("Course name required.");

            var db = GetExternalDb();
            if (db == null) return BadRequest("Database missing.");

            db.Courses.Add(new Course
            {
                CourseName = NewCourseName,
                CourseDescription = NewCourseDescription ?? ""
            });
            await db.SaveChangesAsync();

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostEditCourseAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedCourseId.HasValue) return BadRequest();

            var course = await db.Courses.FindAsync(SelectedCourseId.Value);
            if (course != null)
            {
                course.CourseName = NewCourseName;
                course.CourseDescription = NewCourseDescription ?? "";
                await db.SaveChangesAsync();
            }

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostDeleteCourseAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedCourseId.HasValue) return BadRequest();

            var assignments = db.Assignments.Where(a => a.CourseId == SelectedCourseId.Value).ToList();
            foreach (var assignment in assignments)
            {
                var responses = db.Responses.Where(r => r.AssignmentId == assignment.Id).ToList();
                db.Responses.RemoveRange(responses);
            }
            await db.SaveChangesAsync();
            db.Assignments.RemoveRange(assignments);

            var course = await db.Courses.FindAsync(SelectedCourseId.Value);
            if (course != null) db.Courses.Remove(course);

            await db.SaveChangesAsync();
            SelectedCourseId = null;

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostAddAssignmentAsync()
        {
            if (!SelectedCourseId.HasValue || string.IsNullOrWhiteSpace(NewAssignmentTitle)) return BadRequest();

            var db = GetExternalDb();
            if (db == null) return BadRequest();

            db.Assignments.Add(new Assignment
            {
                CourseId = SelectedCourseId.Value,
                AssignmentTitle = NewAssignmentTitle,
                AssignmentDescription = NewAssignmentDescription ?? "",
                AssignmentPosition = NewAssignmentPosition
            });
            await db.SaveChangesAsync();

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostEditAssignmentAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedAssignmentId.HasValue) return BadRequest();

            var assignment = await db.Assignments.FindAsync(SelectedAssignmentId.Value);
            if (assignment != null)
            {
                assignment.AssignmentTitle = NewAssignmentTitle;
                assignment.AssignmentDescription = NewAssignmentDescription ?? "";
                assignment.AssignmentPosition = NewAssignmentPosition;
                await db.SaveChangesAsync();
            }

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostDeleteAssignmentAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedAssignmentId.HasValue) return BadRequest();

            var responses = db.Responses.Where(r => r.AssignmentId == SelectedAssignmentId.Value).ToList();
            db.Responses.RemoveRange(responses);
            await db.SaveChangesAsync();

            var assignment = await db.Assignments.FindAsync(SelectedAssignmentId.Value);
            if (assignment != null) db.Assignments.Remove(assignment);
            await db.SaveChangesAsync();

            SelectedAssignmentId = null;
            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostAddResponseAsync()
        {
            if (!SelectedAssignmentId.HasValue || string.IsNullOrWhiteSpace(NewResponseTitle)) return BadRequest();

            var db = GetExternalDb();
            if (db == null) return BadRequest();

            db.Responses.Add(new ResponseApp.Models.Response
            {
                AssignmentId = SelectedAssignmentId.Value,
                ResponseTitle = NewResponseTitle,
                ResponseText = NewResponseText
            });
            await db.SaveChangesAsync();

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostEditResponseAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedResponseId.HasValue) return BadRequest();

            var response = await db.Responses.FindAsync(SelectedResponseId.Value);
            if (response != null)
            {
                response.ResponseTitle = NewResponseTitle;
                response.ResponseText = NewResponseText;
                await db.SaveChangesAsync();
            }

            return await ReloadPartialAsync();
        }

        public async Task<IActionResult> OnPostDeleteResponseAsync()
        {
            var db = GetExternalDb();
            if (db == null || !SelectedResponseId.HasValue) return BadRequest();

            var response = await db.Responses.FindAsync(SelectedResponseId.Value);
            if (response != null) db.Responses.Remove(response);
            await db.SaveChangesAsync();

            SelectedResponseId = null;
            return await ReloadPartialAsync();
        }

        private async Task<PartialViewResult> ReloadPartialAsync()
        {
            LoadData();
            return Partial("_DashboardContent", this);
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
                Assignments = db.Assignments.Where(a => a.CourseId == SelectedCourseId.Value).ToList();

            if (SelectedAssignmentId.HasValue)
                Responses = db.Responses.Where(r => r.AssignmentId == SelectedAssignmentId.Value).ToList();

            if (SelectedResponseId.HasValue)
                ResponseText = db.Responses.FirstOrDefault(r => r.Id == SelectedResponseId.Value)?.ResponseText ?? "";
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