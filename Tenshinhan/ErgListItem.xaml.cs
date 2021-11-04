using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tenshinhan.DataClass;

namespace Tenshinhan
{
    /// <summary>
    /// ErgListItem.xaml の相互作用ロジック
    /// </summary>
    public partial class ErgListItem : UserControl
    {
        public LocalErgSave erg { get; private set; }

        public ErgListItem(LocalErgSave erg)
        {
            this.erg = erg;
            InitializeComponent();
            TitleText.Content = erg.title;
            MakerText.Content = erg.maker;
            LastUpdateTimeText.Content = $"最終 {erg.lastUpdateTime.ToString()}";
        }
    }
}
