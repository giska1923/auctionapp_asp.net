namespace Aukcija.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Bid")]
    public partial class Bid
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string IdUser { get; set; }

        public Guid GUIDAuction { get; set; }

        public int TokenNum { get; set; }

        public DateTime Timestamp { get; set; }

        public virtual AspNetUser AspNetUsers { get; set; }

        public virtual Auction Auction { get; set; }
    }
}
