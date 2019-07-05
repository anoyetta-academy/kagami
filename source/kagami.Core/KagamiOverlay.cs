using System;
using System.ComponentModel;
using kagami.Helpers;
using kagami.Models;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiOverlay : OverlayBase<KagamiOverlayConfig>
    {
        private KagamiOverlayConfig config;

        public KagamiOverlay(KagamiOverlayConfig config) : base(config, config.Name)
        {
            this.config = config;
            Logger.LogCallback += this.Log;
            this.config.PropertyChanged += this.Config_PropertyChanged;

            this.timer.Interval = this.config.PollingInterval;
        }

        public override void Dispose()
        {
            Logger.LogCallback -= this.Log;
            this.config.PropertyChanged -= this.Config_PropertyChanged;
            base.Dispose();
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(KagamiOverlayConfig.PollingInterval):
                    this.timer.Interval = this.config.PollingInterval;
                    break;
            }
        }

        private static readonly int LongInterval = 3000;
        private volatile bool isUpdating = false;
        private long previousSeq = 0;

        protected override async void Update()
        {
            try
            {
                if (this.isUpdating)
                {
                    return;
                }

                this.isUpdating = true;

                if (!this.Config.IsDesignMode)
                {
                    if (FFXIVPluginHelper.Instance.FFXIVProcess == null)
                    {
                        this.timer.Interval = LongInterval;
                    }
                    else
                    {
                        if (this.timer.Interval == LongInterval)
                        {
                            this.timer.Interval = this.config.PollingInterval;
                        }
                    }
                }
                else
                {
                    this.timer.Interval = LongInterval;
                }

                lock (this)
                {
                    if (!this.Config.IsDesignMode &&
                        this.previousSeq == ActionEchoesModel.Instance.Seq)
                    {
                        return;
                    }

                    this.previousSeq = ActionEchoesModel.Instance.Seq;
                }

                var json = await ActionEchoesModel.Instance.ParseJsonAsync();

                var updateScript =
                    $"var model =\n{ json };\n\n" +
                    "document.dispatchEvent(new CustomEvent('onActionUpdated', { detail: model }));\n";

                this.Overlay?.Renderer?.Browser?.GetMainFrame()?.ExecuteJavaScript(
                    updateScript,
                    null,
                    0);
            }
            catch (Exception ex)
            {
                this.Log(LogLevel.Error, "Update: {0} {1}", this.Name, ex.ToString());
            }
            finally
            {
                this.isUpdating = false;
            }
        }

        public void ClearJsonCache() => this.previousSeq = 0;
    }
}
