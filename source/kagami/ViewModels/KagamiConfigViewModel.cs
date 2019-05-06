using System;
using Advanced_Combat_Tracker;
using Prism.Commands;
using Prism.Mvvm;

namespace kagami.ViewModels
{
    public class KagamiConfigViewModel : BindableBase
    {
        private KagamiOverlayConfig config;

        public KagamiOverlayConfig Config
        {
            get => this.config;
            set => this.SetProperty(ref this.config, value);
        }

        private static readonly System.Windows.Forms.OpenFileDialog OpenFileDialog = new System.Windows.Forms.OpenFileDialog()
        {
            RestoreDirectory = true,
            Filter = "All Files (*.*)|*.*",
            InitialDirectory = KagamiAddon.Instance.ResourcesDirectory,
        };

        private DelegateCommand browseUrlCommand;

        public DelegateCommand BrowseUrlCommand =>
            this.browseUrlCommand ?? (this.browseUrlCommand = new DelegateCommand(this.ExecuteBrowseUrlCommand));

        private void ExecuteBrowseUrlCommand()
        {
            var result = OpenFileDialog.ShowDialog(ActGlobals.oFormActMain);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.Config.Url = new Uri(OpenFileDialog.FileName).AbsoluteUri;
            }
        }
    }
}
