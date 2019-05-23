#if false
using System;
using System.Globalization;
using Prism.Mvvm;

namespace kagami.Models
{
    public class NetworkAbilityModel : BindableBase
    {
        public static readonly int Code = 21;

        public static string HexCode => Code.ToString("X2");

        private string log;

        public string Log
        {
            get => this.log;
            set => this.SetProperty(ref this.log, value);
        }

        private DateTime timestamp;

        public DateTime PropertyName
        {
            get => this.timestamp;
            set => this.SetProperty(ref this.timestamp, value);
        }

        private DateTime logTimestamp;

        public DateTime LogTimestamp
        {
            get => this.logTimestamp;
            set => this.SetProperty(ref this.logTimestamp, value);
        }

        private string actor;

        public string Actor
        {
            get => this.actor;
            set => this.SetProperty(ref this.actor, value);
        }

        private uint actionID;

        public uint ActionID
        {
            get => this.actionID;
            set => this.SetProperty(ref this.actionID, value);
        }

        private string action;

        public string Action
        {
            get => this.action;
            set => this.SetProperty(ref this.action, value);
        }

        public void AnalyzeLog()
        {
            if (string.IsNullOrEmpty(this.log))
            {
                return;
            }

            var values = this.log.Split(':');

            this.Actor = values.Length > 2 ? values[2] : string.Empty;

            var idText = values.Length > 3 ? values[3] : string.Empty;
            if (uint.TryParse(idText, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out uint id))
            {
                this.ActionID = id;
            }

            this.Action = values.Length > 4 ? values[4] : string.Empty;
        }
    }
}
#endif
