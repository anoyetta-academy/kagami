using System.Windows.Forms;
using System.Windows.Forms.Integration;
using kagami.Views;

namespace kagami
{
    public partial class KagamiOverlayConfigPanel : UserControl
    {
        private KagamiOverlay overlay;
        private KagamiOverlayConfig config;

        public KagamiOverlayConfigPanel(KagamiOverlay overlay)
        {
            this.InitializeComponent();
            this.Dock = DockStyle.Fill;

            this.overlay = overlay;
            this.config = overlay.Config;

            this.Load += (_, __) =>
            {
                var view = new KagamiConfigView();
                view.ViewModel.Overlay = this.overlay;
                view.ViewModel.Config = this.config;

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
