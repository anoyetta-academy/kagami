using System;
using kagami.ViewModels;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace kagami.Models
{
    public class ActionEchoModel : BindableBase
    {
        private static long currentSequence = 0;
        private static readonly object SequenceLocker = new object();

        public ActionEchoModel()
        {
            lock (SequenceLocker)
            {
                if (currentSequence >= long.MaxValue)
                {
                    currentSequence = 0;
                }

                this.Seq = ++currentSequence;
            }
        }

        [JsonProperty("seq")]
        public long Seq { get; private set; }

        private string source;

        [JsonProperty("source")]
        public string Source
        {
            get => this.source;
            set => this.SetProperty(ref this.source, value);
        }

        private DateTime timestamp;

        [JsonIgnore]
        public DateTime Timestamp
        {
            get => this.timestamp;
            set
            {
                if (this.SetProperty(ref this.timestamp, value))
                {
                    this.RaisePropertyChanged(nameof(this.TimestampText));
                }
            }
        }

        [JsonProperty("timestamp")]
        public string TimestampText => this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        private DateTime actualTimestamp;

        [JsonIgnore]
        public DateTime ActualTimestamp
        {
            get => this.actualTimestamp;
            set
            {
                if (this.SetProperty(ref this.actualTimestamp, value))
                {
                    this.RaisePropertyChanged(nameof(this.ActualTimestampText));
                }
            }
        }

        [JsonProperty("actualTimestamp")]
        public string ActualTimestampText => this.ActualTimestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

        private double cycle;

        [JsonProperty("cycle")]
        public double Cycle
        {
            get => this.cycle;
            set => this.SetProperty(ref this.cycle, value);
        }

        private string actor;

        [JsonProperty("actor")]
        public string Actor
        {
            get => this.actor;
            set => this.SetProperty(ref this.actor, value);
        }

        private uint id;

        [JsonProperty("id")]
        public uint ID
        {
            get => this.id;
            set
            {
                if (this.SetProperty(ref this.id, value))
                {
                    this.RaisePropertyChanged(nameof(this.Icon));
                }
            }
        }

        private string name;

        [JsonProperty("name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        [JsonProperty("icon")]
        public string Icon => $"{this.iconCode.ToString(KagamiConfigViewModel.ActionIconCodeFormat)}.png";

        private string iconUri;

        [JsonProperty("iconUri")]
        public string IconUri
        {
            get => this.iconUri;
            set => this.SetProperty(ref this.iconUri, value);
        }

        private int iconCode;

        [JsonIgnore]
        public int IconCode
        {
            get => this.iconCode;
            set => this.SetProperty(ref this.iconCode, value);
        }

        private ActionCategory category;

        [JsonProperty("category")]
        public ActionCategory Category
        {
            get => this.category;
            set => this.SetProperty(ref this.category, value);
        }

        private float recastTime;

        [JsonProperty("recastTime")]
        public float RecastTime
        {
            get => this.recastTime;
            set => this.SetProperty(ref this.recastTime, value);
        }
    }
}
