using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models
{
    public class SubscriptionType
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Period { get; set; }
        public int Price { get; set; }
    }

    [Table("Members")]
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }

        public SubscriptionType SubscriptionType { get; set; }

        public string Period { get; set; }
        public int AmountPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public DateTime ValidUntil { get; set; }

        // ✅ New fields for gym
        [Required]
        public int GymId { get; set; }

        
        [MaxLength(100)]
        public string GymName { get; set; }
    }
}
