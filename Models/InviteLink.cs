using System;
using System.ComponentModel.DataAnnotations;

namespace ResponseApp.Models
{
    public class InviteLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }
    }
}