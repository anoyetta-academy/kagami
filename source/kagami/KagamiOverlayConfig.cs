using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    [Serializable]
    public class KagamiOverlayConfig :
        OverlayConfigBase,
        INotifyPropertyChanged
    {
        public KagamiOverlayConfig(string name)
            : base(name)
        {
            this.SubscribeBasePropertiesChanged();

            this.IsVisible = true;
            this.MaxFrameRate = 30;
        }

        private KagamiOverlayConfig()
            : this(null)
        {
        }

        [XmlIgnore]
        public override Type OverlayType => typeof(KagamiOverlay);

        private void SubscribeBasePropertiesChanged()
        {
            this.VisibleChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.IsVisible));
            this.ClickThruChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.IsClickThru));
            this.UrlChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.Url));
            this.MaxFrameRateChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.MaxFrameRate));
            this.GlobalHotkeyEnabledChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.GlobalHotkeyEnabled));
            this.GlobalHotkeyChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.GlobalHotkey));
            this.GlobalHotkeyModifiersChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.GlobalHotkeyModifiers));
            this.LockChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.IsLocked));
            this.GlobalHotkeyTypeChanged += (x, _) => (x as KagamiOverlayConfig).RaisePropertyChanged(nameof(this.GlobalHotkeyType));
        }

        private int bufferSizeOfActionEcho = 30;

        public int BufferSizeOfActionEcho
        {
            get => this.bufferSizeOfActionEcho;
            set => this.SetProperty(ref this.bufferSizeOfActionEcho, value);
        }

        private int pollingInterval = 50;

        public int PollingInterval
        {
            get => this.pollingInterval;
            set => this.SetProperty(ref this.pollingInterval, value);
        }

        private string logDirectory = Path.GetFullPath(Path.Combine(
            KagamiAddon.Instance.ResourcesDirectory,
            ".."));

        public string LogDirectory
        {
            get => this.logDirectory;
            set => this.SetProperty(ref this.logDirectory, value);
        }

        private bool isGhostMode = false;

        public bool IsGhostMode
        {
            get => this.isGhostMode;
            set => this.SetProperty(ref this.isGhostMode, value);
        }

        private string ghostLogFile = string.Empty;

        public string GhostLogFile
        {
            get => this.ghostLogFile;
            set => this.SetProperty(ref this.ghostLogFile, value);
        }

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(
            [CallerMemberName]string propertyName = null)
            => this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName]string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        #endregion INotifyPropertyChanged
    }
}
