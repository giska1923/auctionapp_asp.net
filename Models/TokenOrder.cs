namespace Aukcija.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TokenOrder")]
    public partial class TokenOrder
    {
        [Key]
        public Guid GUID { get; set; }

        [Required]
        [StringLength(128)]
        public string IdUser { get; set; }

        public int TokenNum { get; set; }

        public decimal? PackageP { get; set; }

        [Required]
        [StringLength(2)]
        public string Status { get; set; }

        public virtual AspNetUser AspNetUsers { get; set; }
    }
}
