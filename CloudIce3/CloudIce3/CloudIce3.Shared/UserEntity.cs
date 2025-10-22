using Azure;
using Azure.Data.Tables;

namespace CloudIce3.Shared;

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "USER";
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string BlobUrl { get; set; } = "";

    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
}
