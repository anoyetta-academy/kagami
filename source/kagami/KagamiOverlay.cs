using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using kagami.Helpers;
using kagami.Models;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiOverlay : OverlayBase<KagamiOverlayConfig>
    {
        public KagamiOverlay(KagamiOverlayConfig config, string name) : base(config, name)
        {
            Logger.LogCallback += this.Log;
            this.Config.PropertyChanged += this.Config_PropertyChanged;

            this.timer.Interval = this.Config.PollingInterval;
        }

        public override Control CreateConfigControl()
            => new KagamiOverlayConfigPanel(this);

        public override void Dispose()
        {
            Logger.LogCallback -= this.Log;
            this.Config.PropertyChanged -= this.Config_PropertyChanged;
            base.Dispose();
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(KagamiOverlayConfig.PollingInterval):
                    this.timer.Interval = this.Config.PollingInterval;
                    break;
            }
        }

        private static readonly int LongInterval = 3000;
        private volatile bool isUpdating = false;
        private long previousSeq = 0;
        private bool previousStats = false;

        public override void Start()
        {
            Task.Run(async () =>
            {
                FFXIVPluginHelper.Instance.Start();
                SharlayanHelper.Instance.Start();

                await Task.Delay(100);

                XIVLogSubscriber.Instance.Start();
            });

            base.Start();
        }

        public override void Stop()
        {
            XIVLogSubscriber.Instance.Stop();
            FFXIVPluginHelper.Instance.Stop();
            SharlayanHelper.Instance.Stop();

            base.Stop();
        }

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
                            this.timer.Interval = this.Config.PollingInterval;
                        }
                    }
                }
                else
                {
                    this.timer.Interval = LongInterval;
                }

                var stats = ActionEchoesModel.Instance.GetEncounterStats();
                var isNeedsSave = false;

                lock (this)
                {
                    if (!this.Config.IsDesignMode &&
                        this.previousSeq == ActionEchoesModel.Instance.Seq &&
                        this.previousStats == stats)
                    {
                        return;
                    }

                    this.previousSeq = ActionEchoesModel.Instance.Seq;

                    if (this.previousStats != stats)
                    {
                        this.previousStats = stats;
                        isNeedsSave = !stats;
                    }
                }

                var json = await ActionEchoesModel.Instance.ParseJsonAsync();

                var updateScript =
                    $"var model =\n{ json };\n\n" +
                    "document.dispatchEvent(new CustomEvent('onActionUpdated', { detail: model }));\n";

                this.Overlay?.Renderer?.ExecuteScript(updateScript);

                if (isNeedsSave)
                {
                    var script = "document.dispatchEvent(new CustomEvent('onEndEncounter', null));\n";
                    this.Overlay?.Renderer?.ExecuteScript(script);

                    await ActionEchoesModel.Instance.SaveLogAsync();
                    ActionEchoesModel.Instance.Clear();
                }
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
