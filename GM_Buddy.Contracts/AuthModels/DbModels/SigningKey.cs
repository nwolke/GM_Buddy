namespace GM_Buddy.Contracts.AuthModels.DbModels
{
    public record SigningKey
    {
        public int Id { get; set; }
        public required string KeyId { get; set; }
        public required string PrivateKey { get; set; }
        public required string PublicKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
