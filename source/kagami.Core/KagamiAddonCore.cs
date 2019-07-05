using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using kagami.Helpers;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiAddonCore
    {
        public static KagamiAddonCore Current
        {
            get;
            private set;
        }

        public string ResourcesDirectory
        {
            get;
            set;
        }

        public IOverlayConfig Config
        {
            get;
            private set;
        }

        public IOverlay Overlay
        {
            get;
            private set;
        }

        public KagamiAddonCore()
        {
            Current = this;
        }

        public string Name => $"kagami";

        public string Description => "FFXIV skill rotation viewer.";

        public Type OverlayType => typeof(KagamiOverlay);

        public Type OverlayConfigType => typeof(KagamiOverlayConfig);

        public Type OverlayConfigControlType => typeof(KagamiOverlayConfigPanel);

        public Control CreateOverlayConfigControlInstance(IOverlay overlay) => new KagamiOverlayConfigPanel(overlay as KagamiOverlay);

        public IOverlayConfig CreateOverlayConfigInstance(string name) => new KagamiOverlayConfig(name);

        public IOverlay CreateOverlayInstance(IOverlayConfig config)
        {
            this.Config = config;
            this.Initialize();
            this.Overlay = new KagamiOverlay(this.Config as KagamiOverlayConfig);
            return this.Overlay;
        }

        public void Dispose()
        {
            XIVLogSubscriber.Instance.Stop();
            FFXIVPluginHelper.Instance.Stop();
            SharlayanHelper.Instance.Stop();
        }

        private void Initialize()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);

                FFXIVPluginHelper.Instance.Start();
                SharlayanHelper.Instance.Start();

                await Task.Delay(100);

                XIVLogSubscriber.Instance.Start();
            });
        }
    }
}
