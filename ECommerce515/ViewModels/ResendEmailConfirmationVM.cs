using System.ComponentModel.DataAnnotations;

namespace ECommerce515.ViewModels
{
    public class ResendEmailConfirmationVM
    {
        public int Id { get; set; }
        [Required]
        public string EmailORUserName { get; set; } = string.Empty;
    }
}
