using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Models;
using CMCS.Prototype.Services;

namespace CMCS.Prototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IFileStore _store;
        public ClaimsController(IFileStore store) => _store = store;

        public IActionResult Index()
        {
            // NOTE: If you still want "My Claims" filtered to a single user,
            // this line keeps it filtered to ClaimStore.CurrentLecturer.
            // If you want to see all claims regardless of name, replace this
            // with: var mine = ClaimStore.Claims.OrderByDescending(...).ToList();
            var mine = ClaimStore.Claims
                .Where(c => c.LecturerName == ClaimStore.CurrentLecturer)
                .OrderByDescending(c => c.Year).ThenByDescending(c => c.Month)
                .ToList();

            return View(mine);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ClaimVm
            {
                ClaimId = Guid.NewGuid(),
                // Make the name editable by NOT setting it to CurrentLecturer here.
                // Optionally prefill a placeholder-like value:
                LecturerName = string.Empty,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                HourlyRate = 400,
                Entries = new List<ClaimEntryVm> { new() { Date = DateOnly.FromDateTime(DateTime.Today) } }
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimVm vm, List<IFormFile> files, CancellationToken ct)
        {
            // Basic model checks
            if (string.IsNullOrWhiteSpace(vm.LecturerName))
                ModelState.AddModelError(nameof(vm.LecturerName), "Lecturer name is required.");

            // Normalize/validate entries
            vm.Entries ??= new List<ClaimEntryVm>();
            vm.Entries = vm.Entries
                .Where(e => e is not null && e.Hours > 0 && !string.IsNullOrWhiteSpace(e.Description))
                .ToList();

            if (!vm.Entries.Any())
                ModelState.AddModelError("", "Add at least one entry with hours and description.");

            // Server-side file guards
            var allowed = new[] { ".pdf", ".docx", ".xlsx" };
            foreach (var f in files.Where(f => f?.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    ModelState.AddModelError("", "Only PDF, DOCX, or XLSX files are allowed.");
                if (f.Length > 10 * 1024 * 1024)
                    ModelState.AddModelError("", "Each file must be 10 MB or smaller.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            // Finalize claim WITHOUT forcing the name
            vm.ClaimId = vm.ClaimId == Guid.Empty ? Guid.NewGuid() : vm.ClaimId;
            vm.CreatedOn = DateTime.UtcNow;
            vm.TotalHours = vm.Entries.Sum(e => e.Hours);
            vm.Status = ClaimStatus.Submitted;

            // Save encrypted files
            foreach (var f in files.Where(f => f?.Length > 0))
                await _store.SaveAsync(vm.ClaimId, f, ct);

            ClaimStore.Claims.Add(vm);
            TempData["Message"] = "Claim submitted and files stored securely (encrypted).";
            return RedirectToAction(nameof(Index));
        }
    }
}