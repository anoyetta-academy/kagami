﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Advanced_Combat_Tracker;
using kagami.Helpers;
using kagami.Helpers.Common;
using kagami.Models;

namespace kagami
{
    public class XIVLogSubscriber
    {
        #region Singleton

        private static readonly Lazy<XIVLogSubscriber> LazyInstance = new Lazy<XIVLogSubscriber>(() => new XIVLogSubscriber());

        public static XIVLogSubscriber Instance => LazyInstance.Value;

        private XIVLogSubscriber()
        {
        }

        #endregion Singleton

        private readonly ConcurrentQueue<LogLineEventArgs> LogInfoQueue = new ConcurrentQueue<LogLineEventArgs>();
        private ThreadWorker logSubscriber;

        private KagamiOverlayConfig Config => KagamiAddonCore.Current.Config as KagamiOverlayConfig;

        private void OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            if (this.Config == null ||
                !this.Config.IsVisible)
            {
                return;
            }

            if (logInfo.logLine.Length <= 18)
            {
                return;
            }

            this.LogInfoQueue.Enqueue(logInfo);
        }

        public void Start()
        {
            this.Stop();

            ActGlobals.oFormActMain.OnLogLineRead += this.OnLogLineRead;

            this.logSubscriber = new ThreadWorker(
                this.StoreLog,
                0,
                "XIVLog Subscriber",
                ThreadPriority.BelowNormal);

            this.logSubscriber.Run();
        }

        public async void Stop()
        {
            if (this.logSubscriber != null)
            {
                this.logSubscriber.Abort();
                this.logSubscriber = null;
            }

            await ActionEchoesModel.Instance.SaveLogAsync();

            ActGlobals.oFormActMain.OnLogLineRead -= this.OnLogLineRead;
            this.ClearBuffer();
            ActionEchoesModel.Instance.Clear();
        }

        public void ClearBuffer()
        {
            while (this.LogInfoQueue.TryDequeue(out LogLineEventArgs e)) ;
        }

        private static readonly int LongSleep = 3000;
        private static readonly byte PC = 1;

        private static readonly string MedicatedDummyActionID = "FFFF";
        private static readonly int MedicatedDummyActionIconID = 061165;

        private Regex networkAbilityRegex;
        private Regex medicatedRegex;
        private Regex defeatedRegex;
        private string previousActorName;

        /*
        private static readonly string ChangedZoneLog = "01:Changed Zone to";
        private static readonly string ChangedPrimaryPlayerLog = "02:Changed primary player";
        */

        private volatile string previousActionID = string.Empty;
        private DateTime previousActionTimestamp = DateTime.MinValue;

        private void StoreLog()
        {
            if (this.Config == null ||
                !this.Config.IsVisible)
            {
                if (!this.LogInfoQueue.IsEmpty)
                {
                    while (this.LogInfoQueue.TryDequeue(out LogLineEventArgs e)) ;
                }

                Thread.Sleep(LongSleep);
                return;
            }

            var interval = this.Config.PollingInterval;

            try
            {
                if (this.LogInfoQueue.IsEmpty)
                {
                    return;
                }

                var player = FFXIVPluginHelper.Instance.CurrentPlayer;
                var target = FFXIVPluginHelper.Instance.CurrentTarget;
                var focus = FFXIVPluginHelper.Instance.CurrentFocusTarget;

                if (string.IsNullOrEmpty(player?.Name) &&
                    string.IsNullOrEmpty(target?.Name) &&
                    string.IsNullOrEmpty(focus?.Name))
                {
                    interval = LongSleep;
                    return;
                }

                var actor = player;
                if (focus != null &&
                    focus.type == PC)
                {
                    actor = focus;
                }
                else
                {
                    if (target != null &&
                        target.type == PC)
                    {
                        actor = target;
                    }
                }

                if (this.previousActorName != actor.Name)
                {
                    this.previousActorName = actor.Name;

                    // 15:10078E31:Anoyetta Anon:A5:サモン:
                    // 16:10078E31:Anoyetta Anon:B1:ミアズラ:
                    this.networkAbilityRegex = new Regex(
                        $" (15|16):[0-9a-fA-F]+:{actor.Name}:(?<ActionID>[0-9a-fA-F]+):(?<ActionName>.+?):[0-9a-fA-F]",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    // 1A:100EFDBA:Anoyetta Anon gains the effect of 強化薬
                    this.medicatedRegex = new Regex(
                        $" 1A:[0-9a-fA-F]+:{actor.Name} gains the effect of (?<ActionName>強化薬|Medicated|Stärkung|Médicamenté)",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    // 19:Anoyetta Anon was defeated by ガルーダ.
                    this.defeatedRegex = new Regex(
                        $" 19:{actor.Name} was defeated by",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    ActionEchoesModel.Instance.Clear();
                }

                if (this.networkAbilityRegex == null ||
                    this.medicatedRegex == null ||
                    this.defeatedRegex == null)
                {
                    interval = LongSleep;
                    return;
                }

                ActionEchoesModel.Instance.PlayerName = actor.Name;
                ActionEchoesModel.Instance.PlayerJob = actor.GetJob().ToString();

                while (this.LogInfoQueue.TryDequeue(out LogLineEventArgs e))
                {
                    var line = e?.logLine?.Trim() ?? string.Empty;

                    if (!line.Contains("] 01:") &&
                        !line.Contains("] 02:") &&
                        !line.Contains("] 15:") &&
                        !line.Contains("] 16:") &&
                        !line.Contains("] 19:") &&
                        !line.Contains("] 1A:"))
                    {
                        continue;
                    }

                    try
                    {
                        var isMedicated = false;

                        var match = this.networkAbilityRegex.Match(line);
                        if (!match.Success)
                        {
                            match = this.medicatedRegex.Match(line);
                            if (!match.Success)
                            {
                                continue;
                            }

                            isMedicated = true;
                        }

                        var t = line.Substring(0, 15)
                            .TrimEnd()
                            .Replace("[", string.Empty)
                            .Replace("]", string.Empty);
                        if (!DateTime.TryParse(t, out DateTime timestamp))
                        {
                            timestamp = DateTime.Now;
                        }

                        var actionID = !isMedicated ?
                            match.Groups["ActionID"].ToString() :
                            MedicatedDummyActionID;
                        var actionName = match.Groups["ActionName"].ToString();

                        if (string.Equals(
                            actionID,
                            this.previousActionID,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            if ((timestamp - this.previousActionTimestamp)
                                < TimeSpan.FromMilliseconds(200))
                            {
                                continue;
                            }
                        }

                        this.previousActionID = actionID;
                        this.previousActionTimestamp = timestamp;

                        var echo = new ActionEchoModel();
                        echo.Timestamp = timestamp;
                        echo.ActualTimestamp = DateTime.Now;
                        echo.Source = line;
                        echo.Actor = actor.Name;

                        if (uint.TryParse(actionID, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out uint i))
                        {
                            echo.ID = i;
                        }

                        echo.Name = actionName;

                        if (isMedicated)
                        {
                            echo.Category = ActionCategory.Item;
                            echo.IconCode = MedicatedDummyActionIconID;
                        }
                        else
                        {
                            var info = SharlayanHelper.Instance.GetActionInfo(echo.ID);
                            if (info != null)
                            {
                                echo.IconCode = info.Icon;
                                echo.Category = (ActionCategory)Enum.ToObject(typeof(ActionCategory), info.ActionCategory);
                                echo.RecastTime = (float)info.RecastTime;
                            }
                        }

                        ActionEchoesModel.Instance.Time = echo.Timestamp;

                        ActionEchoesModel.Instance.Add(echo);
                    }
                    finally
                    {
                        Thread.Yield();
                    }
                }

                interval = 0;
            }
            finally
            {
                this.logSubscriber.Interval = interval;
            }
        }
    }
}
