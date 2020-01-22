using System.Windows.Controls;
using kagami.ViewModels;

namespace kagami.Views
{
    /// <summary>
    /// KagamiConfigView.xaml の相互作用ロジック
    /// </summary>
    public partial class KagamiConfigView : UserControl
    {
        public KagamiConfigView()
        {
            this.InitializeComponent();
        }

        public KagamiConfigViewModel ViewModel => this.DataContext as KagamiConfigViewModel;
    }
}
