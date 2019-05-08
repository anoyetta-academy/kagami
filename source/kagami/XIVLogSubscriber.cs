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

        private KagamiOverlayConfig Config => KagamiAddon.Current.Config;

        private void OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
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

        private Regex networkAbilityRegex;
        private Regex defeatedRegex;
        private string previousPlayerName;

        private static readonly string ChangedZoneLog = "01:Changed Zone to";
        private static readonly string ChangedPrimaryPlayerLog = "02:Changed primary player";

        private async void StoreLog()
        {
            if (this.Config == null)
            {
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

                var player = SharlayanHelper.Instance.GetCurrentPlayer();
                if (player == null ||
                    string.IsNullOrEmpty(player.Name))
                {
                    interval = LongSleep;
                    return;
                }

                if (this.previousPlayerName != player.Name)
                {
                    this.previousPlayerName = player.Name;

                    // 15:10078E31:Anoyetta Anon:A5:サモン:
                    this.networkAbilityRegex = new Regex(
                        $" 15:[0-9a-fA-F]+:{player.Name}:(?<ActionID>[0-9a-fA-F]+):(?<ActionName>.+):",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    // 19:Anoyetta Anon was defeated by ガルーダ.
                    this.defeatedRegex = new Regex(
                        $" 19:{player.Name} was defeated by",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }

                if (this.networkAbilityRegex == null ||
                    this.defeatedRegex == null)
                {
                    interval = LongSleep;
                    return;
                }

                ActionEchoesModel.Instance.PlayerName = player.Name;
                ActionEchoesModel.Instance.PlayerJob = player.Job.ToString();

                while (this.LogInfoQueue.TryDequeue(out LogLineEventArgs e))
                {
                    var line = e?.logLine?.Trim() ?? string.Empty;

                    if (!line.Contains("] 01:") &&
                        !line.Contains("] 02:") &&
                        !line.Contains("] 15:") &&
                        !line.Contains("] 19:"))
                    {
                        continue;
                    }

                    try
                    {
                        if (line.Contains(ChangedZoneLog) ||
                            line.Contains(ChangedPrimaryPlayerLog) ||
                            this.defeatedRegex.IsMatch(line))
                        {
                            await ActionEchoesModel.Instance.SaveLogAsync();
                            ActionEchoesModel.Instance.Clear();
                            continue;
                        }

                        var match = this.networkAbilityRegex.Match(line);
                        if (!match.Success)
                        {
                            continue;
                        }

                        var echo = new ActionEchoModel();

                        var timestamp = line.Substring(0, 15).TrimEnd();
                        if (DateTime.TryParse(timestamp, out DateTime d))
                        {
                            echo.Timestamp = d;
                        }
                        else
                        {
                            echo.Timestamp = DateTime.Now;
                        }

                        echo.Source = line;
                        echo.Actor = player.Name;

                        var id = match.Groups["ActionID"].ToString();
                        if (uint.TryParse(id, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out uint i))
                        {
                            echo.ID = i;
                        }

                        echo.Name = match.Groups["ActionName"].ToString();

                        var info = SharlayanHelper.Instance.GetActionInfo(echo.ID);
                        if (info != null)
                        {
                            echo.Category = (ActionCategory)Enum.ToObject(typeof(ActionCategory), info.ActionCategory);
                            echo.RecastTime = (float)info.RecastTime;
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
