using ImagesDBLibrary.Database.Model;
using System;
using System.Linq;
namespace ImagesDBLibrary.Model
{
    public class TagInfoM
    {
        #region Constructors

        public TagInfoM(TagsInImage dbTagEntry)
        {
            AddedDate = dbTagEntry.AddedTime;
            Tag = TagM.Tags.First(x => x.DbId == dbTagEntry.TagFK);
            IsUserAdded = dbTagEntry.UserAdded;
            Value = dbTagEntry.Value;
        }

        #endregion //Constructors

        #region Properties

        public DateTime AddedDate { get; private set; }

        public TagM Tag { get; private set; }

        public bool IsUserAdded { get; private set; }

        public string Value { get; private set; }
        
        #endregion //Properties
    }
}
