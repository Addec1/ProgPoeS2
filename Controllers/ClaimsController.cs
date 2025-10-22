using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Models;
using CMCS.Prototype.Services;

namespace CMCS.Prototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IFileStore _store;
        public ClaimsController(IFileStore store) => _store = store;

        // ✅ Show ALL claims (not just CurrentLecturer)
        public IActionResult Index()
        {
            var claims = ClaimStore.Claims
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .ThenByDescending(c => c.CreatedOn)
                .ToList();

            return View(claims);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ClaimVm
            {
                ClaimId = Guid.NewGuid(),
                LecturerName = string.Empty, // editable
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
            if (string.IsNullOrWhiteSpace(vm.LecturerName))
                ModelState.AddModelError(nameof(vm.LecturerName), "Lecturer name is required.");

            vm.Entries ??= new List<ClaimEntryVm>();
            vm.Entries = vm.Entries
                .Where(e => e is not null && e.Hours > 0 && !string.IsNullOrWhiteSpace(e.Description))
                .ToList();

            if (!vm.Entries.Any())
                ModelState.AddModelError("", "Add at least one entry with hours and description.");

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

            // ✅ Ensure totals are calculated and metadata set
            vm.ClaimId = vm.ClaimId == Guid.Empty ? Guid.NewGuid() : vm.ClaimId;
            vm.CreatedOn = DateTime.UtcNow;
            vm.TotalHours = vm.Entries.Sum(e => e.Hours);
            vm.Status = ClaimStatus.Submitted;

            foreach (var f in files.Where(f => f?.Length > 0))
                await _store.SaveAsync(vm.ClaimId, f, ct);

            ClaimStore.Claims.Add(vm);
            TempData["Message"] = "Claim submitted and files stored securely (encrypted).";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var claim = ClaimStore.Find(id);
            if (claim is null) return NotFound();
            ViewBag.Files = await _store.ListAsync(id, ct);
            return View(claim);
        }
    }
}