using System.Windows.Forms;
using System.Windows.Forms.Integration;
using kagami.Views;

namespace kagami
{
    public partial class KagamiOverlayConfigPanel : UserControl
    {
        public KagamiOverlayConfigPanel(KagamiOverlay overlay)
        {
            this.InitializeComponent();
            this.Dock = DockStyle.Fill;

            this.Load += (_, __) =>
            {
                var view = new KagamiConfigView();
                view.ViewModel.Config = overlay.Config;

                var elementHost = new ElementHost()
                {
                    Dock = DockStyle.Fill,
                    Child = view
                };

                this.Controls.Add(elementHost);
            };
        }
    }
}
