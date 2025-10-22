using System.Security.Cryptography;
using System.Text.Json;

namespace CMCS.Prototype.Services
{
    public sealed class EncryptedFileStore : IFileStore
    {
        private readonly string _uploadsDir;
        private readonly string _metaPath;
        private readonly byte[] _key;

        private sealed class Meta { public List<StoredFile> Files { get; set; } = new(); }
        private Meta _meta;

        public EncryptedFileStore(string appDataRoot, string keyHex)
        {
            _uploadsDir = Path.Combine(appDataRoot, "uploads");
            Directory.CreateDirectory(_uploadsDir);

            _metaPath = Path.Combine(appDataRoot, "metadata.json");
            _meta = File.Exists(_metaPath)
                ? JsonSerializer.Deserialize<Meta>(File.ReadAllText(_metaPath)) ?? new Meta()
                : new Meta();

            _key = Convert.FromHexString(keyHex);
            if (!(_key.Length is 16 or 24 or 32))
                throw new InvalidOperationException("Encryption key must be 16/24/32 bytes (hex).");
        }

        public async Task<StoredFile> SaveAsync(Guid claimId, IFormFile file, CancellationToken ct = default)
        {
            var id = Guid.NewGuid();
            var outPath = Path.Combine(_uploadsDir, id.ToString("N") + ".bin");

            await using (var fs = File.Create(outPath))
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.GenerateIV();

                await fs.WriteAsync(aes.IV, ct); // prefix IV

                await using var crypto = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
                await file.CopyToAsync(crypto, ct);
            }

            var item = new StoredFile
            {
                DocumentId = id,
                ClaimId = claimId,
                OriginalName = Path.GetFileName(file.FileName),
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                SizeKB = (int)Math.Ceiling(file.Length / 1024d),
                UploadedOn = DateTime.UtcNow
            };

            _meta.Files.Add(item);
            File.WriteAllText(_metaPath, JsonSerializer.Serialize(_meta, new JsonSerializerOptions { WriteIndented = true }));
            return item;
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> OpenAsync(Guid documentId, CancellationToken ct = default)
        {
            var meta = _meta.Files.FirstOrDefault(f => f.DocumentId == documentId);
            if (meta is null) return null;

            var path = Path.Combine(_uploadsDir, documentId.ToString("N") + ".bin");
            if (!File.Exists(path)) return null;

            var ms = new MemoryStream();
            await using (var fs = File.OpenRead(path))
            {
                var iv = new byte[16];
                if (await fs.ReadAsync(iv.AsMemory(0, 16), ct) != 16) return null;

                using var aes = Aes.Create();
                aes.Key = _key; aes.IV = iv;

                await using var crypto = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read);
                await crypto.CopyToAsync(ms, ct);
            }
            ms.Position = 0;
            return (ms, meta.ContentType, meta.OriginalName);
        }

        public Task<IReadOnlyList<StoredFile>> ListAsync(Guid claimId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<StoredFile>>(
                _meta.Files.Where(f => f.ClaimId == claimId).OrderByDescending(f => f.UploadedOn).ToList());
    }
}

// Reference: Microsoft Learn (2024) Aes Class – Symmetric Encryption Example.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes
// Used to implement AES encryption and decryption for file storage functionality.