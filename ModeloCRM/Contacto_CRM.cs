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
    
    public partial class Contacto_CRM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contacto_CRM()
        {
            this.Contacto_Cliente_CRM = new HashSet<Contacto_Cliente_CRM>();
        }
    
        public int ID_Contacto { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rut { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contacto_Cliente_CRM> Contacto_Cliente_CRM { get; set; }
    }
}
