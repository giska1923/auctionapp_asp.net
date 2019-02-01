namespace Aukcija.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Auction")]
    public partial class Auction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Auction()
        {
            Bids = new HashSet<Bid>();
        }

        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int Duration { get; set; }
        [DisplayName("Starting price")]        
        public decimal StartingP { get; set; }
        [DisplayName("Current price")]
        public decimal CurrentP { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Opened { get; set; }

        public DateTime? Closed { get; set; }

        [StringLength(2)]
        public string Status { get; set; }

        [StringLength(128)]
        public string IdUser { get; set; }

        public int? IdImage { get; set; }

        [StringLength(3)]
        public string Currency { get; set; }

        public virtual AspNetUser AspNetUsers { get; set; }

        public virtual Image Image { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bid> Bids { get; set; }
    }
}
