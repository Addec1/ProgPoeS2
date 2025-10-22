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

// Reference: Microsoft Learn (2023) C# Properties and expression-bodied members.
// Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
// Assisted in defining calculated properties such as TotalAmount.
