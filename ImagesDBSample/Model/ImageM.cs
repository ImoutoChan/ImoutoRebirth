using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class ImageM
    {
        #region Constructors

        public ImageM(Image dbimage, IEnumerable<Tag> tags)
        {
            DbId = dbimage.Id;
            Md5 = dbimage.Md5;
            Size = dbimage.Size;
            Path = dbimage.Path;

            Tags = tags.Select(x => TagM.Tags.FirstOrDefault(y => y.DbId == x.Id));
        }

        #endregion Constructors

        #region Properties

        public int DbId { get; private set; }

        public IEnumerable<TagM> Tags { get; private set; }

        public string Md5 { get; private set; }

        public long Size { get; private set; }

        public string Path { get; private set; }

        #endregion Properties
    }

    //todo add/remove tags
}
