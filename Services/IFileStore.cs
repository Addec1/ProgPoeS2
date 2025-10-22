using Microsoft.AspNetCore.Http;

namespace CMCS.Prototype.Services
{
    public sealed class StoredFile
    {
        public Guid DocumentId { get; init; }
        public Guid ClaimId { get; init; }
        public string OriginalName { get; init; } = "";
        public string ContentType { get; init; } = "";
        public int SizeKB { get; init; }
        public DateTime UploadedOn { get; init; }
    }

    public interface IFileStore
    {
        Task<StoredFile> SaveAsync(Guid claimId, IFormFile file, CancellationToken ct = default);
        Task<(Stream Stream, string ContentType, string FileName)?> OpenAsync(Guid documentId, CancellationToken ct = default);
        Task<IReadOnlyList<StoredFile>> ListAsync(Guid claimId, CancellationToken ct = default);
    }
}