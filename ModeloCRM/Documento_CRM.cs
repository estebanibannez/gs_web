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
    
    public partial class Documento_CRM
    {
        public int ID_Documento { get; set; }
        public Nullable<int> ID_Cliente { get; set; }
        public string Ruta { get; set; }
        public string Nombre_Original { get; set; }
        public string Nombre_Asignado { get; set; }
        public string Categoria { get; set; }
        public Nullable<System.DateTime> Fecha_Documento { get; set; }
        public Nullable<int> Estado { get; set; }
    }
}