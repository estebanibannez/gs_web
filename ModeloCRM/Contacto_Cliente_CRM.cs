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
    
    public partial class Contacto_Cliente_CRM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contacto_Cliente_CRM()
        {
            this.Contactos_Operacionales_CRM = new HashSet<Contactos_Operacionales_CRM>();
        }
    
        public int ID_Contacto_Cliente { get; set; }
        public Nullable<int> ID_Cliente { get; set; }
        public Nullable<int> ID_Contacto { get; set; }
        public Nullable<int> Cargo { get; set; }
        public Nullable<bool> Contactado { get; set; }
        public string Telefono_Personal { get; set; }
        public string Telefono_Trabajo { get; set; }
        public string Fax { get; set; }
        public Nullable<System.DateTime> Fecha_Inicio_Contacto { get; set; }
        public string email { get; set; }
        public string Referencia_Contacto { get; set; }
        public Nullable<System.DateTime> Fecha_Mod { get; set; }
        public string Usuario_Mod { get; set; }
        public string Estado { get; set; }
    
        public virtual Cliente_CRM Cliente_CRM { get; set; }
        public virtual Contacto_CRM Contacto_CRM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contactos_Operacionales_CRM> Contactos_Operacionales_CRM { get; set; }
    }
}
