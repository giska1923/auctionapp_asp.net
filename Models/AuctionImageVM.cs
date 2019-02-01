using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Aukcija.Models
{
    public class AuctionImageVM
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        [DisplayName("Starting price")]
        public decimal StartingP { get; set; }
        [DisplayName("Upload File")]
        public string Path { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
}