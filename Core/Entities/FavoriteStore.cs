public class FavoriteStore
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
}