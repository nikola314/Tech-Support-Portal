using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechSupportPortal.Models
{

    public class Pack
    {
        [Key]
        public int PackId { get; set; }

        public TokenPack packName;

        public int Amount { get; set; }

        public int Price { get; set; }
    }
}