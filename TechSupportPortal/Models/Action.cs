using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{
    public abstract class Action
    {
        [Key]
        public int ActionId { get; set; }

        public string Text { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Author { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Response> Responses { get; set; }
    }
}