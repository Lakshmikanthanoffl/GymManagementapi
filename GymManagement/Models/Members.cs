using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GymManagement.Models
{
    public class SubscriptionType
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Period { get; set; }
        public int Price { get; set; }
    }

    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public SubscriptionType SubscriptionType { get; set; }

        public string Period { get; set; }
        public int AmountPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public DateTime ValidUntil { get; set; }
    }

}
