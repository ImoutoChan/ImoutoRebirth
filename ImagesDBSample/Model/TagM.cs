using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class TagM
    {
        #region Constructors

        private TagM(Tag tag)
        {
            DbId = tag.Id;
            Name = tag.Name;
            Type = TagTypeM.TagTypes.FirstOrDefault(x => x.DbId == tag.Type);
        }

        #endregion Constructors

        #region Properties

        public int DbId { get; private set; }

        public string Name { get; private set; }

        public TagTypeM Type { get; private set; }
        
        #endregion Properties

        #region Static members

        static TagM()
        {
            Tags = ImagesDB.GetTags().Select(x => new TagM(x));
        }

        public static IEnumerable<TagM> Tags { get; private set; }

        #endregion Static members
    }

    //todo rename, change type;
}