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
        public long Seq { get; private set; } = 0L;

        [JsonIgnore]
        public KagamiOverlayConfig Config => KagamiAddonCore.Current.Config as KagamiOverlayConfig;

        [JsonProperty("player")]
        public string PlayerName { get; set; }

        [JsonProperty("job")]
        public string PlayerJob { get; set; }

        [JsonProperty("encDPS")]
        public double EncDPS { get; set; }

        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }

        [JsonProperty("zone")]
        public string Zone => ActGlobals.oFormActMain?.CurrentZone ?? string.Empty;

        [JsonIgnore]
        public DateTime Time { get; set; }

        [JsonProperty("time")]
        public string TimeText => this.Time.ToString("yyyy-MM-dd HH:mm:ss.fff");

        [JsonProperty("minSkillCycle")]
        public double MinimumSkillCycle { get; set; } = 0;

        [JsonProperty("avgSkillCycle")]
        public double AverageSkillCycle { get; set; } = 0;

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        private readonly List<ActionEchoModel> echoes = new List<ActionEchoModel>(5120);

        [JsonIgnore]
        public IReadOnlyList<ActionEchoModel> Echoes => this.echoes;

        private volatile bool toFile = false;

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

                if (this.toFile)
                {
                    return source;
                }

                var result = (
                    from x in source
                    where
                    x.Timestamp <= this.Time &&
                    x.Timestamp >= this.Time.AddSeconds(this.Config.BufferSizeOfActionEcho * -1)
                    orderby
                    x.Seq descending
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
                this.Seq = 0;
                this.MinimumSkillCycle = 0;
                this.AverageSkillCycle = 0;
            }
        }

        public void Add(ActionEchoModel model)
        {
            lock (this.echoes)
            {
                var lastSkill = this.echoes.LastOrDefault(x =>
                    x.Category == ActionCategory.Weaponskill ||
                    x.Category == ActionCategory.Spell);

                if (lastSkill != null)
                {
                    model.Cycle = Math.Round(
                        (model.ActualTimestamp - lastSkill.ActualTimestamp).TotalSeconds,
                        3);
                }

                this.echoes.Add(model);
                this.Seq++;

                var statistics =
                    from x in this.echoes
                    where
                    (x.Category == ActionCategory.Weaponskill || x.Category == ActionCategory.Spell) &&
                    x.Cycle >= 2.0d &&
                    x.Cycle <= 3.0d
                    select
                    x;

                if (statistics.Any())
                {
                    this.AverageSkillCycle = Math.Round(statistics.Average(x => x.Cycle), 3);
                    this.MinimumSkillCycle = statistics.Min(x => x.Cycle);
                }
            }
        }

        public bool GetEncounterStats()
            => ActGlobals.oFormActMain?.ActiveZone?.ActiveEncounter?.Active ?? false;

        public async Task<string> ParseJsonAsync() => await Task.Run(() =>
        {
            var data = this.Config.IsDesignMode ? CreateDesignModeDataModel() : this;
            var json = string.Empty;

            if (!this.Config.IsDesignMode)
            {
                var encounter = ActGlobals.oFormActMain?.ActiveZone?.ActiveEncounter;
                if (encounter != null)
                {
                    var dpsList = encounter.GetAllies();
                    var dps = dpsList.FirstOrDefault(x =>
                        x.Name == this.PlayerName ||
                        x.Name == "YOU");

                    this.EncDPS = Math.Round(dps?.EncDPS ?? 0);
                    this.Duration = dps?.Duration ?? TimeSpan.Zero;
                    this.IsActive = encounter.Active;
                }
                else
                {
                    this.EncDPS = 0;
                    this.Duration = TimeSpan.Zero;
                    this.IsActive = false;
                }
            }

            lock (this)
            {
                json = JsonConvert.SerializeObject(
                    data,
                    Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        DefaultValueHandling = DefaultValueHandling.Include,
                    });
            }

            return json;
        });

        public async Task SaveLogAsync()
        {
            if (this.echoes.Count < 1)
            {
                return;
            }

            var fileName = string.Empty;

            lock (this)
            {
                var encounter = ActGlobals.oFormActMain?.ActiveZone?.ActiveEncounter;
                var zone = !string.IsNullOrEmpty(encounter?.ZoneName) ?
                    encounter?.ZoneName :
                    this.Zone;
                var title = !string.IsNullOrEmpty(encounter?.Title) ?
                    encounter?.Title :
                    "UNKNOWN";

                fileName =
                    $"{DateTime.Now:yyMMdd_HHmmss}_{this.PlayerName}[{this.PlayerJob}]_{zone}({title}).json";

                // 無効な文字を取り除く
                fileName = string.Concat(fileName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
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

                try
                {
                    this.toFile = true;

                    File.WriteAllText(
                        f,
                        await this.ParseJsonAsync(),
                        new UTF8Encoding(false));
                }
                finally
                {
                    this.toFile = false;
                }
            });
        }

        #region Design mode

        private static ActionEchoesModel CreateDesignModeDataModel()
        {
            var model = new ActionEchoesModel();

            var now = DateTime.Now;

            model.PlayerName = "Anoyetta Anon";
            model.PlayerJob = "SCH";
            model.EncDPS = 3567;
            model.Duration = TimeSpan.FromMinutes(10);
            model.Time = now;
            model.IsActive = true;

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 5),
                Actor = model.PlayerName,
                ID = 168,
                Name = "ミアズマ",
                Category = ActionCategory.Spell,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 4.01),
                Actor = model.PlayerName,
                ID = 178,
                Name = "バイオラ",
                Category = ActionCategory.Spell,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 4),
                Actor = model.PlayerName,
                ID = 9618,
                Name = "エナジードレイン",
                Category = ActionCategory.Ability,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 3.01),
                Actor = model.PlayerName,
                ID = 179,
                Name = "シャドウフレア",
                Category = ActionCategory.Ability,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 3),
                Actor = model.PlayerName,
                ID = 7436,
                Name = "連環計",
                Category = ActionCategory.Ability,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 2.01),
                Actor = model.PlayerName,
                ID = 177,
                Name = "ミアズラ",
                Category = ActionCategory.Spell,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 2),
                Actor = model.PlayerName,
                ID = 9618,
                Name = "エナジードレイン",
                Category = ActionCategory.Ability,
                RecastTime = 2.5f,
            });

            model.Add(new ActionEchoModel()
            {
                Timestamp = now.AddSeconds(-2.5 * 1),
                Actor = model.PlayerName,
                ID = 7435,
                Name = "魔炎法",
                Category = ActionCategory.Spell,
                RecastTime = 2.5f,
            });

            return model;
        }

        #endregion Design mode
    }
}
