using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Models;
using CMCS.Prototype.Services;

namespace CMCS.Prototype.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IFileStore _store;
        public ManagerController(IFileStore store) => _store = store;

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var claims = ClaimStore.Claims
                .Where(c => c.Status == ClaimStatus.Verified)
                .OrderByDescending(c => c.Year).ThenByDescending(c => c.Month).ToList();

            var files = new Dictionary<Guid, IReadOnlyList<StoredFile>>();
            foreach (var c in claims) files[c.ClaimId] = await _store.ListAsync(c.ClaimId, ct);
            ViewBag.Files = files;

            return View(claims);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Approve(Guid id)
        {
            var c = ClaimStore.Find(id);
            if (c is null) return NotFound();
            if (c.Status == ClaimStatus.Verified) c.Status = ClaimStatus.Approved;
            TempData["Message"] = "Claim approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Reject(Guid id)
        {
            var c = ClaimStore.Find(id);
            if (c is null) return NotFound();
            if (c.Status is ClaimStatus.Verified or ClaimStatus.Approved) c.Status = ClaimStatus.Rejected;
            TempData["Message"] = "Claim rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
