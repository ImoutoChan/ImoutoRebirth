//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImoutoNavigator.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class tagset
    {
        public tagset()
        {
            this.tagset_tag_connection = new HashSet<tagset_tag_connection>();
        }
    
        public int id { get; set; }
        public int id_file { get; set; }
        public System.DateTime added_time { get; set; }
        public bool is_actual { get; set; }
    
        public virtual file file { get; set; }
        public virtual ICollection<tagset_tag_connection> tagset_tag_connection { get; set; }
    }
}
