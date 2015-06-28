using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Imouto.Utils;
using Imouto.WCFExchageLibrary.Data;

namespace ImoutoNavigator.ViewModel
{
    class BindedTagVM : VMBase
    {
        private readonly BindedTag _bindedTagM;

        public BindedTagVM(BindedTag bindedTagModel)
        {
            this._bindedTagM = bindedTagModel;
        }

        public SearchType SearchType
        {
            get
            {
                return this._bindedTagM.SearchType;
            }
            set
            {
                this._bindedTagM.SearchType = value;
                OnPropertyChanged(() => this.SearchType);
            }
        }

        public Tag Tag => this._bindedTagM.Tag;

        public Brush TypeBrush => new SolidColorBrush(Tag.Type.Color.ToColor());

        public string Synonyms => String.Join(", ", Tag.SynonymsCollection);

        public string Value
        {
            get
            {
                return this._bindedTagM.Value;
            }
            set
            {
                this._bindedTagM.Value = value;
                OnPropertyChanged(() => this.Value);
            }
        }

        public BindedTag Model => this._bindedTagM;
    }
}
