namespace Aukcija.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Token")]
    public partial class Token
    {
        public int Id { get; set; }
        [DisplayName("Default number of auctions")]
        public int? NumN { get; set; }
        [DisplayName("Default auction time")]
        public int? NumD { get; set; }
        [DisplayName("Silver package token amount")]
        public int? NumS { get; set; }
        [DisplayName("Gold package token amount")]
        public int? NumG { get; set; }
        [DisplayName("Platinum package token amount")]
        public int? NumP { get; set; }
        [DisplayName("Active Currency")]
        [StringLength(3)]
        public string ActiveCurrency { get; set; }
        [DisplayName("Token amount in RSD")]
        public decimal? RSD { get; set; }
        [DisplayName("Token amount in USD")]
        public decimal? USD { get; set; }
        [DisplayName("Token amount in EUR")]
        public decimal? EUR { get; set; }
    }
}
