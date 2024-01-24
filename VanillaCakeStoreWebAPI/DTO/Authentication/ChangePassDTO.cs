namespace VanillaCakeStoreWebAPI.DTO.Authentication
{
    public class ChangePassDTO
    {
        public string OldPassword { get; set; } = null!;

        public string NewPassword { get; set; } = null!;
    }
}
