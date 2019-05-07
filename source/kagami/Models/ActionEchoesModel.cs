using System;
using System.Collections.Generic;
using System.Linq;
using Advanced_Combat_Tracker;
using Newtonsoft.Json;

namespace kagami.Models
{
    public class ActionEchoesModel
    {
        #region Singleton

        private static readonly Lazy<ActionEchoesModel> LazyInstance = new Lazy<ActionEchoesModel>(() => new ActionEchoesModel());

        public static ActionEchoesModel Instance => LazyInstance.Value;

        private ActionEchoesModel()
        {
        }

        #endregion Singleton

        [JsonIgnore]
        public KagamiOverlayConfig Config => KagamiAddon.Instance.Config;

        [JsonProperty("player")]
        public string PlayerName { get; set; }

        [JsonProperty("job")]
        public string PlayerJob { get; set; }

        [JsonProperty("zone")]
        public string Zone => ActGlobals.oFormActMain?.CurrentZone ?? string.Empty;

        [JsonProperty("time")]
        public DateTime Time { get; private set; }

        private readonly List<ActionEchoModel> echoes = new List<ActionEchoModel>(5120);

        [JsonIgnore]
        public IReadOnlyList<ActionEchoModel> Echoes => this.echoes;

        [JsonProperty("actions")]
        public IEnumerable<ActionEchoModel> Actions
        {
            get
            {
                var source = default(IEnumerable<ActionEchoModel>);
                lock (this.echoes)
                {
                    source = this.echoes.ToArray();
                }

                var result = (
                    from x in source
                    where
                    x.Timestamp <= this.Time &&
                    x.Timestamp >= this.Time.AddSeconds(this.Config.BufferSizeOfActionEcho * -1)
                    select
                    x).ToArray();

                return result;
            }
        }

        public void Clear()
        {
            lock (this.echoes)
            {
                this.echoes.Clear();
            }
        }

        public void Add(ActionEchoModel model)
        {
            lock (this.echoes)
            {
                this.echoes.Add(model);
            }
        }

        public void AddRange(IEnumerable<ActionEchoModel> models)
        {
            lock (this.echoes)
            {
                this.echoes.AddRange(models);
            }
        }

        public string ParseJson()
        {
            return string.Empty;
        }
    }
}
