namespace VanillaCakeStoreWebAPI.DTO.Authentication
{
    public class ClaimDTO
    {
        public string? CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public int AccountId { get; set; }
        public int Role { get; set; }
        public string Email { get; set; } = null!;
    }
}
