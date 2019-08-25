using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{
    public enum TokenPack { S, G, P }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        public TokenPack TokenPack { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }
    }
}