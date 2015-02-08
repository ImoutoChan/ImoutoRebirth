using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WCFExchageLibrary.Data;

namespace ImoutoViewer.ViewModel
{
    class BindedTagVM : VMBase
    {
        private readonly Dictionary<string, Tuple<SolidColorBrush, SolidColorBrush>> typeBrushes = new Dictionary<string, Tuple<SolidColorBrush, SolidColorBrush>>
        {
                { "Artist", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0xAA, 0x0, 0x0)), new SolidColorBrush(Color.FromRgb(0xC6, 0x66, 0x66))) },
                { "General", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0xFF, 0x76, 0x1C)), new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66))) },
                { "Character", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0x00, 0xAA, 0x00)), new SolidColorBrush(Color.FromRgb(0x66, 0xCC, 0x66))) },
                { "Medium", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0x25, 0x85, 0xBB)), new SolidColorBrush(Color.FromRgb(0x7C, 0xB6, 0xD6))) },
                { "Copyright", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0xAA, 0x0, 0xAA)), new SolidColorBrush(Color.FromRgb(0xCC, 0x66, 0xCC))) },
                { "Meta", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0x5C, 0x00, 0xFF)), new SolidColorBrush(Color.FromRgb(0x9D, 0x66, 0xFF))) },
                { "Studio", new Tuple<SolidColorBrush, SolidColorBrush>(new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0xCD)), new SolidColorBrush(Color.FromRgb(0xFF, 0x93, 0xFF))) },
        };

        private TagsVM _parantVM;
        private BindedTag _modelTag;
        
        public BindedTagVM(BindedTag tag, TagsVM tagsVM)
        {
            _modelTag = tag;
            _parantVM = tagsVM;
        }

        public string Title
        {
            get
            {
                var tag = WebUtility.HtmlDecode(_modelTag.Tag.Title);

                if (_modelTag.Tag.HasValue)
                {
                    return tag + " : " + WebUtility.HtmlDecode(_modelTag.Value);
                }
                else
                {
                    return tag;
                }
            }
        }

        public Brush TypeBrush
        {
            get
            {
                try
                {
                    if (typeBrushes.ContainsKey(_modelTag.Tag.Type.Title))
                    {
                        return typeBrushes[_modelTag.Tag.Type.Title].Item1;
                    }
                    else
                    {
                    return typeBrushes["General"].Item1;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public string Source
        {
            get
            {
                return _modelTag.Source.ToString();
            }
        }
    }
}
