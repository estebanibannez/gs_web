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
    
    public partial class Tipo_Contacto_CRM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tipo_Contacto_CRM()
        {
            this.Contactos_Operacionales_CRM = new HashSet<Contactos_Operacionales_CRM>();
        }
    
        public int ID_Tipo_Contacto_CRM { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contactos_Operacionales_CRM> Contactos_Operacionales_CRM { get; set; }
    }
}
