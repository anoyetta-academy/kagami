using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Advanced_Combat_Tracker;
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

        private dynamic ffxivPlugin;
        private dynamic ffxivPluginConfig;
        private dynamic ffxivPluginLogParse;

        public Process FFXIVProcess => this.ffxivPluginConfig?.Process;

        public string FFXIVPluginLanguage => (this.ffxivPluginLogParse?.Settings?.LanguageID ?? 0) switch
        {
            1 => "English",
            2 => "French",
            3 => "German",
            4 => "Japanese",
            _ => "English",
        };

        private static readonly double AttachSubscribeInterval = 3000;

        public void Start()
        {
            this.attachWorker = new ThreadWorker(() =>
            {
                if (ActGlobals.oFormActMain == null)
                {
                    return;
                }

                if (this.ffxivPlugin == null)
                {
                    this.ffxivPlugin = (
                        from x in ActGlobals.oFormActMain.ActPlugins
                        where
                        x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                        x.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper())
                        select
                        x.pluginObj).FirstOrDefault();
                }

                if (this.ffxivPlugin != null &&
                    this.ffxivPluginConfig == null)
                {
                    var fi = this.ffxivPlugin.GetType().GetField(
                        "_Memory",
                        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    var memory = fi?.GetValue(this.ffxivPlugin);
                    if (memory == null)
                    {
                        return;
                    }

                    fi = memory.GetType().GetField(
                        "_config",
                        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    this.ffxivPluginConfig = fi?.GetValue(memory);

                    Logger.Info("FFXIV_ACT_Plugin.Config attached.");
                }

                if (this.ffxivPlugin != null &&
                    this.ffxivPluginLogParse == null)
                {
                    var fi = this.ffxivPlugin.GetType().GetField(
                        "_LogParse",
                        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

                    this.ffxivPluginLogParse = fi?.GetValue(this.ffxivPlugin);

                    Logger.Info("FFXIV_ACT_Plugin.LogParse attached.");
                }

                if (this.ffxivPlugin != null &&
                    this.ffxivPluginConfig != null &&
                    this.ffxivPluginLogParse != null)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(AttachSubscribeInterval));
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

            this.ffxivPlugin = null;
            this.ffxivPluginConfig = null;
            this.ffxivPluginLogParse = null;
        }
    }
}
