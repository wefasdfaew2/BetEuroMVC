//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BetEuro
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Match
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Match()
        {
            this.Bets = new HashSet<Bet>();
        }
    
        public int Id { get; set; }
        public int HomeId { get; set; }
        public int AwayId { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<int> Factor { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bet> Bets { get; set; }
        public virtual Team AwayTeam { get; set; }
        public virtual Team HomeTeam { get; set; }
        public virtual Score Score { get; set; }
    }
}
