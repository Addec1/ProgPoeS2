using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Models;
using System.Linq;
using System.Reflection;

namespace CMCS.Prototype.Controllers
{
    public class ApprovalsController : Controller
    {
        private static List<ClaimVm> Claims()
        {
            var f = typeof(ClaimsController).GetField("_claims", BindingFlags.NonPublic | BindingFlags.Static);
            return (List<ClaimVm>)(f?.GetValue(null) ?? new List<ClaimVm>());
        }

        public IActionResult Index()
        {
            // In the prototype we treat this page as "staff-only" (no auth wired yet)
            var model = Claims()
                .OrderByDescending(c => c.Year).ThenByDescending(c => c.Month)
                .ToList();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Verify(Guid id)
        {
            var c = Claims().FirstOrDefault(x => x.ClaimId == id);
            if (c is null) return NotFound();
            c.Status = ClaimStatus.Verified;
            TempData["Message"] = "Claim verified.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Approve(Guid id)
        {
            var c = Claims().FirstOrDefault(x => x.ClaimId == id);
            if (c is null) return NotFound();
            c.Status = ClaimStatus.Approved;
            TempData["Message"] = "Claim approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Reject(Guid id)
        {
            var c = Claims().FirstOrDefault(x => x.ClaimId == id);
            if (c is null) return NotFound();
            c.Status = ClaimStatus.Rejected;
            TempData["Message"] = "Claim rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
