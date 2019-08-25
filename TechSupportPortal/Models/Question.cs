using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{
    public enum SortTypes
    {
        Time, Votes
    }

    public class Question:Action
    {
        public Question()
        {
            this.Channels = new HashSet<Channel>();
        }

        public string Title { get; set; }

        public byte[] Image { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LastLockedAt { get; set; }

        public virtual ICollection<Channel> Channels { get; set; }
    }
}