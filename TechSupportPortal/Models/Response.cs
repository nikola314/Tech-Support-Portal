using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{
    public class Response : Action
    {
        public Response()
        {
            this.Voters = new HashSet<Account>();
        }

        public int ActionRespToId { get; set; }
        [ForeignKey("ActionRespToId")]
        public virtual Action RespondingTo { get; set; }

        public int Upvotes { get; set; }

        public int Downvotes { get; set; }

        public virtual ICollection<Account> Voters { get; set; }
    }
}