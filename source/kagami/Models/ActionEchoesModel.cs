using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<string> ParseJsonAsync() => await Task.Run(() =>
        {
            var json = JsonConvert.SerializeObject(
                this,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                });

            return json;
        });

        private int takeCount;

        public async Task SaveLogAsync()
        {
            if (this.echoes.Count < 1)
            {
                return;
            }

            var fileName = string.Empty;

            lock (this)
            {
                this.takeCount++;
                fileName =
                    $"{DateTime.Now:yyyy-MM-dd_HHmmss}.{this.PlayerName}[{this.PlayerJob}].{this.Zone}.{this.takeCount}.json";
            }

            if (string.IsNullOrEmpty(this.Config.LogDirectory))
            {
                return;
            }

            await Task.Run(async () =>
            {
                if (!Directory.Exists(this.Config.LogDirectory))
                {
                    Directory.CreateDirectory(this.Config.LogDirectory);
                }

                var f = Path.Combine(this.Config.LogDirectory, fileName);
                File.WriteAllText(
                    f,
                    await this.ParseJsonAsync(),
                    new UTF8Encoding(false));
            });
        }
    }
}
