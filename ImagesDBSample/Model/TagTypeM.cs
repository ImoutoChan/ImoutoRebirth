using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class TagTypeM
    {
        #region Constructors

        private TagTypeM(TagType dbtype)
        {
            DbId = dbtype.Id;
            Name = dbtype.Name;
        }
        
        #endregion Constructors

        #region Properties
        public string Name { get; private set; }

        public int DbId { get; private set; }
        
        #endregion Properties

        #region Static members

        static TagTypeM()
        {
            TagTypes = ImagesDB.GetTagTypes().Select(x => new TagTypeM(x));
        }

        public static IEnumerable<TagTypeM> TagTypes { get; private set; }

        #endregion Static members
    }
}