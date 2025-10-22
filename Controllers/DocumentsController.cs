using Microsoft.AspNetCore.Mvc;
using CMCS.Prototype.Services;

namespace CMCS.Prototype.Controllers
{
    [Route("documents")]
    public class DocumentsController : Controller
    {
        private readonly IFileStore _store;
        public DocumentsController(IFileStore store) => _store = store;

        [HttpGet("download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty) return NotFound();
            var opened = await _store.OpenAsync(id, ct);
            if (opened is null) return NotFound();
            var (stream, contentType, fileName) = opened.Value;
            return File(stream, contentType, fileName);
        }
    }
}
