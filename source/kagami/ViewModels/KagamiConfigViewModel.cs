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

        private DelegateCommand browseUrlCommand;

        public DelegateCommand BrowseUrlCommand =>
            this.browseUrlCommand ?? (this.browseUrlCommand = new DelegateCommand(this.ExecuteBrowseUrlCommand));

        private void ExecuteBrowseUrlCommand()
        {
        }
    }
}
