using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Services;

namespace CMCS.Prototype.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly IFileStore _store;
        public DocumentsController(IFileStore store) => _store = store;

        [HttpGet("/documents/download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id, CancellationToken ct)
        {
            var opened = await _store.OpenAsync(id, ct);
            if (opened is null) return NotFound();
            var (stream, contentType, fileName) = opened.Value;
            return File(stream, contentType, fileName);
        }
    }
}
