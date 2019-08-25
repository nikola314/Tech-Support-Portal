using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{

    public enum AccountRole { Admin, Agent, Client };
    public enum AccountStatus { Active, Inactive };

    public class Account
    {

        public Account()
        {
            this.AgentChannels = new HashSet<Channel>();
            this.Votes = new HashSet<Response>();
        }

        [Key]
        public int AccountId { get; set; }

        [Required]
        [MinLength(2)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        public string LastName { get; set; }

        [Required]
        public string Mail { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [NotMapped]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int Tokens { get; set; }

        [Required]
        public AccountRole Role { get; set; }

        [Required]
        public AccountStatus Status { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        //[InverseProperty("AccountId")]
        public virtual ICollection<Channel> UserChannels { get; set; }

        public virtual ICollection<Channel> AgentChannels { get; set; }
        public virtual ICollection<Response> Votes { get; set; }
    }
}