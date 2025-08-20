using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Plan { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public int GymId { get; set; }

        [MaxLength(100)]
        public string GymName { get; set; }

        public string Screenshot { get; set; }
    }
    public class PaymentRequest
    {
        public string UserName { get; set; }
        public string Plan { get; set; }
        public decimal Price { get; set; }
        public DateTime PaymentDate { get; set; }
        public int GymId { get; set; }
        public string GymName { get; set; }
    }
}
