using System;
using System.Diagnostics;
using System.Threading;
using kagami.Helpers.Common;
using Sharlayan;
using Sharlayan.Models;
using Sharlayan.Utilities;

namespace kagami.Helpers
{
    public class SharlayanHelper
    {
        #region Singleton

        private static readonly Lazy<SharlayanHelper> LazyInstance = new Lazy<SharlayanHelper>(() => new SharlayanHelper());

        public static SharlayanHelper Instance => LazyInstance.Value;

        private SharlayanHelper()
        {
        }

        #endregion Singleton

        public Sharlayan.Models.XIVDatabase.ActionItem GetActionInfo(uint id) => ActionLookup.GetActionInfo(id);

        public Sharlayan.Core.CurrentPlayer GetCurrentPlayer() => Reader.GetCurrentPlayer()?.CurrentPlayer;

        private ThreadWorker processSubscriber;
        private static readonly double ProcessSubscribeInterval = 3000;

        public void Start()
        {
            lock (this)
            {
                if (this.processSubscriber == null)
                {
                    this.processSubscriber = new ThreadWorker(
                        this.DetectFFXIVProcess,
                        ProcessSubscribeInterval,
                        "Sharlayan Process Subscriber",
                        ThreadPriority.Lowest);
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (this.processSubscriber != null)
                {
                    this.processSubscriber.Abort();
                    this.processSubscriber = null;
                }
            }
        }

        private Process currentFFXIVProcess;
        private string currentFFXIVLanguage;

        private void DetectFFXIVProcess()
        {
            var ffxiv = FFXIVPluginHelper.Instance.FFXIVProcess;
            var ffxivLanguage = FFXIVPluginHelper.Instance.FFXIVPluginLanguage;

            if (ffxiv == null)
            {
                return;
            }

            lock (this)
            {
                if (!MemoryHandler.Instance.IsAttached ||
                    this.currentFFXIVProcess == null ||
                    this.currentFFXIVProcess?.Id != ffxiv?.Id ||
                    this.currentFFXIVLanguage != ffxivLanguage)
                {
                    this.currentFFXIVProcess = ffxiv;
                    this.currentFFXIVLanguage = ffxivLanguage;

                    if (MemoryHandler.Instance.IsAttached)
                    {
                        MemoryHandler.Instance.UnsetProcess();
                    }

                    var model = new ProcessModel
                    {
                        Process = ffxiv,
                        IsWin64 = true
                    };

                    MemoryHandler.Instance.SetProcess(
                        model,
                        ffxivLanguage);

                    Logger.Info("Sharlayan attached.");
                }
            }
        }
    }
}
