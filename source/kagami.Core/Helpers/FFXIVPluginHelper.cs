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

        private void DoRefreshCombatant()
        {
            this.CurrentPlayer = this.DataRepository?.GetCombatantList().FirstOrDefault();
            this.CurrentTarget = this.DataRepository?.GetCombatantByOverlayType(OverlayType.Target);
        }
    }

    public static class CombatantExtensions
    {
        public static Job GetJob(
            this Combatant c)
            => (Job)Enum.ToObject(typeof(Job), c.Job);
    }

    public enum Job
    {
        Unknown = -1,
        ADV = 0,
        GLA = 1,
        PUG = 2,
        MRD = 3,
        LNC = 4,
        ARC = 5,
        CNJ = 6,
        THM = 7,
        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,
        MIN = 16,
        BOT = 17,
        FSH = 18,
        PLD = 19,
        MNK = 20,
        WAR = 21,
        DRG = 22,
        BRD = 23,
        WHM = 24,
        BLM = 25,
        ACN = 26,
        SMN = 27,
        SCH = 28,
        ROG = 29,
        NIN = 30,
        MCH = 31,
        DRK = 32,
        AST = 33,
        SAM = 34,
        RDM = 35,
        BLU = 36,
        GNB = 37,
        DNC = 38,
    }
}
