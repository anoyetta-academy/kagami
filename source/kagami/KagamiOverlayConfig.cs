using System;
using System.ComponentModel;
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
            : base(null)
        {
        }

        [XmlIgnore]
        public override Type OverlayType => typeof(KagamiOverlay);

        private void SubscribeBasePropertiesChanged()
        {
            this.VisibleChanged += (_, __) => this.RaisePropertyChanged(nameof(this.IsVisible));
            this.ClickThruChanged += (_, __) => this.RaisePropertyChanged(nameof(this.IsClickThru));
            this.UrlChanged += (_, __) => this.RaisePropertyChanged(nameof(this.Url));
            this.MaxFrameRateChanged += (_, __) => this.RaisePropertyChanged(nameof(this.MaxFrameRate));
            this.GlobalHotkeyEnabledChanged += (_, __) => this.RaisePropertyChanged(nameof(this.GlobalHotkeyEnabled));
            this.GlobalHotkeyChanged += (_, __) => this.RaisePropertyChanged(nameof(this.GlobalHotkey));
            this.GlobalHotkeyModifiersChanged += (_, __) => this.RaisePropertyChanged(nameof(this.GlobalHotkeyModifiers));
            this.LockChanged += (_, __) => this.RaisePropertyChanged(nameof(this.IsLocked));
            this.GlobalHotkeyTypeChanged += (_, __) => this.RaisePropertyChanged(nameof(this.GlobalHotkeyType));
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
