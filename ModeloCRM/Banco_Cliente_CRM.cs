//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ModeloCRM
{
    using System;
    using System.Collections.Generic;
    
    public partial class Banco_Cliente_CRM
    {
        public int ID_Banco_Cliente { get; set; }
        public Nullable<int> ID_Cliente { get; set; }
        public Nullable<int> ID_Banco { get; set; }
    
        public virtual Cliente_CRM Cliente_CRM { get; set; }
    }
}