//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImagesDBLibrary.Database.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Source
    {
        public Source()
        {
            this.ImagesInSources = new HashSet<ImagesInSource>();
            this.Collections = new HashSet<Collection>();
        }
    
        public int Id { get; set; }
        public string Path { get; set; }
    
        public virtual ICollection<ImagesInSource> ImagesInSources { get; set; }
        public virtual ICollection<Collection> Collections { get; set; }
    }
}
