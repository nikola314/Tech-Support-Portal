using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{
    public class Channel
    {

        public Channel()
        {
            this.Agents = new HashSet<Account>();
            this.Questions = new HashSet<Question>();
        }

        [Key]
        public int ChannelId { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedAt { get; set; }
        
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } // Client

        public bool IsOpen { get; set; }

        public virtual ICollection<Account> Agents { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}