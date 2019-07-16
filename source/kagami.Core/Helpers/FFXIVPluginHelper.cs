using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using FFXIV_ACT_Plugin.Common.Models;
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

        private ThreadWorker pluginSubscriber;
        private ThreadWorker combatantSubscriber;

        private dynamic plugin;
        private IDataRepository DataRepository { get; set; }
        private IDataSubscription DataSubscription { get; set; }

        public Process FFXIVProcess => this.DataRepository?.GetCurrentFFXIVProcess();

        public string FFXIVPluginLanguage => this.DataRepository?.GetSelectedLanguageID().ToString();

        private static readonly double AttachSubscribeInterval = 3000;
        private static readonly double CombatantSubscribeInterval = 500;

        private KagamiOverlayConfig Config => KagamiAddonCore.Current.Config as KagamiOverlayConfig;

        public void Start()
        {
            this.pluginSubscriber = new ThreadWorker(() =>
            {
                if (ActGlobals.oFormActMain == null)
                {
                    return;
                }

                if (this.plugin != null)
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
            "FFXIV_ACT_Plugin subscriber",
            ThreadPriority.Lowest);

            this.pluginSubscriber.Run();

            this.combatantSubscriber = new ThreadWorker(
                this.DoRefreshCombatant,
                CombatantSubscribeInterval,
                "Combatant subscriber",
                ThreadPriority.Lowest);

            this.combatantSubscriber.Run();
        }

        public void Stop()
        {
            if (this.pluginSubscriber != null)
            {
                this.pluginSubscriber.Abort();
                this.pluginSubscriber = null;
            }

            if (this.combatantSubscriber != null)
            {
                this.combatantSubscriber.Abort();
                this.combatantSubscriber = null;
            }

            this.plugin = null;
            this.DataRepository = null;
            this.DataSubscription = null;
        }

        public Combatant CurrentPlayer { get; private set; }

        public Combatant CurrentTarget { get; private set; }

        public Combatant CurrentFocusTarget { get; private set; }

        private void DoRefreshCombatant()
        {
            if (!this.Config.IsVisible)
            {
                Thread.Sleep(5000);
                return;
            }

            this.CurrentPlayer = this.DataRepository?.GetCombatantList().FirstOrDefault();

            if (this.Config.IsEnableTargetCapture)
            {
                this.CurrentTarget = this.DataRepository?.GetCombatantByOverlayType(OverlayType.Target);
            }
            else
            {
                this.CurrentTarget = null;
            }

            if (this.Config.IsEnableFocusTargetCapture)
            {
                this.CurrentFocusTarget = this.DataRepository?.GetCombatantByOverlayType(OverlayType.FocusTarget);
            }
            else
            {
                this.CurrentFocusTarget = null;
            }
        }
    }
}
