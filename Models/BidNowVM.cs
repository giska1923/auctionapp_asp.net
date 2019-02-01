using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Aukcija.Models
{
    public class BidNowVM
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public DateTime Closed { get; set; }
        [DisplayName("Current price")]
        public decimal CurrentP { get; set; }
        public string Currency { get; set; }
        public string Email { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public int offer { get; set; }
    }
}