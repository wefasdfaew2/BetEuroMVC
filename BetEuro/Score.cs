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
    
    public partial class Score
    {
        public int Id { get; set; }
        public Nullable<int> HomeScore { get; set; }
        public Nullable<int> AwayScore { get; set; }
        public Nullable<int> Result { get; set; }
    
        public virtual Match Match { get; set; }
    }
}