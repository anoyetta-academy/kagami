using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using kagami.Helpers.Common;

namespace kagami.Helpers
{
    public class FFXIVPluginHelper
    {
        #region Singleton

        private static readonly Lazy<FFXIVPluginHelper> LazyInstance = new Lazy<FFXIVPluginHelper>(() => new FFXIVPluginHelper());

        public static FFXIVPluginHelper Instance => LazyInstance.Value;

        private FFXIVPluginHelper()
        {
        }

        #endregion Singleton

        private ThreadWorker attachWorker;

        private dynamic plugin;
        private IDataRepository DataRepository { get; set; }
        private IDataSubscription DataSubscription { get; set; }

        public Process FFXIVProcess => this.DataRepository?.GetCurrentFFXIVProcess();

        public string FFXIVPluginLanguage => this.DataRepository?.GetSelectedLanguageID().ToString();

        private static readonly double AttachSubscribeInterval = 3000;

        public void Start()
        {
            this.attachWorker = new ThreadWorker(() =>
            {
                if (ActGlobals.oFormActMain == null)
                {
                    return;
                }

                var ffxivPlugin = (
                    from x in ActGlobals.oFormActMain.ActPlugins
                    where
                    x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                    x.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper())
                    select
                    x.pluginObj).FirstOrDefault();

                if (ffxivPlugin != null)
                {
                    this.plugin = ffxivPlugin;
                    this.DataRepository = this.plugin.DataRepository;
                    this.DataSubscription = this.plugin.DataSubscription;

                    Logger.Info("FFXIV_ACT_Plugin attached.");
                }
            },
            AttachSubscribeInterval,
            "FFXIV_ACT_Plugin Subscriber",
            ThreadPriority.Lowest);

            this.attachWorker.Run();
        }

        public void Stop()
        {
            if (this.attachWorker != null)
            {
                this.attachWorker.Abort();
                this.attachWorker = null;
            }

            this.plugin = null;
            this.DataRepository = null;
            this.DataSubscription = null;
        }
    }
}
