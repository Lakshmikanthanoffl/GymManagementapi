using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string UserEmail { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } // ⚠️ Store hashed passwords in production!

        // ✅ New Fields
        [Required]
        public int GymId { get; set; }

        
        [MaxLength(100)]
        public string GymName { get; set; }

        public DateTime? PaidDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal? AmountPaid { get; set; }
        public bool IsActive { get; set; } = true;

        public List<string> Privileges { get; set; } = new();

        [Column("plan")]
        public string PlanName { get; set; }


        [MaxLength(50)]
        public string SubscriptionPeriod { get; set; } = "Monthly";
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class gymidandname
    {
        public int GymId { get; set; }
        public string GymName { get; set; }
    }
}
