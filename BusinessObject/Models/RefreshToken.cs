namespace BusinessObject.Models
{
    public partial class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int? AccountId { get; set; }
        public string? RefreshToken1 { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public virtual Account? Account { get; set; }
    }
}
