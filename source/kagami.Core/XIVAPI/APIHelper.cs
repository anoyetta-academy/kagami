using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using kagami.Helpers;
using Newtonsoft.Json;

namespace kagami.XIVAPI
{
    public class APIHelper
    {
        #region Lazy Singleton

        private static readonly Lazy<APIHelper> LazyInstance = new Lazy<APIHelper>(() => new APIHelper());

        public static APIHelper Instance => LazyInstance.Value;

        private APIHelper()
        {
        }

        #endregion Lazy Singleton

        public string Language { get; private set; }

        private static readonly string BaseUri = "https://xivapi.com";

        public async Task LoadAsync()
        {
            this.Language = await Task.Run(() =>
            {
                do
                {
                    var lang = FFXIVPluginHelper.Instance.FFXIVPluginLanguage;
                    if (string.IsNullOrEmpty(lang))
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    return FFXIVPluginHelper.Instance.FFXIVPluginLanguage.ToLower() switch
                    {
                        "english" => "en",
                        "japanese" => "ja",
                        "german" => "de",
                        "french" => "fr",
                        "chinese" => "cn",
                        "korean" => "kr",
                        _ => "en",
                    };
                } while (true);
            });

            this.statusDictionaryJa = await this.LoadStatusAsync("ja");

            if (this.Language != "ja")
            {
                this.statusDictionary = await this.LoadStatusAsync(this.Language);
            }

            this.actionDictionary = await this.LoadActionAsync(this.Language);
        }

        private Dictionary<int, Status> statusDictionaryJa;
        private Dictionary<int, Status> statusDictionary;
        private Dictionary<int, Action> actionDictionary;

        public Status GetStatusInfo(
            string name)
        {
            var result = default(Status);

            var ja = this.statusDictionaryJa.FirstOrDefault(x =>
                x.Value.Name == name);

            if (this.statusDictionary == null)
            {
                result = ja.Value;
            }
            else
            {
                result = this.statusDictionary.ContainsKey(ja.Key) ?
                    this.statusDictionary[ja.Key] :
                    null;
            }

            return result;
        }

        private async Task<Dictionary<int, Status>> LoadStatusAsync(
            string language)
        {
            var list = new Dictionary<int, Status>(1024);
            var page = 1;

            do
            {
                var res = await BaseUri
                    .AppendPathSegment("status")
                    .SetQueryParams(new
                    {
                        limit = 3000,
                        language = language,
                        columns = "ID,Name,Description,Icon,Category",
                        page = page,
                    })
                    .GetAsync();

                var result = JsonConvert.DeserializeObject(
                    res.Content.ReadAsStringAsync().Result)
                    as dynamic;

                foreach (var item in result.Results)
                {
                    var entry = new Status()
                    {
                        ID = item.ID,
                        Name = item.Name,
                        Description = item.Description,
                        Category = item.Category,
                        Icon = item.Icon,
                    };

                    list.Add(entry.ID, entry);
                }

                if (result.Pagination.Page >= result.Pagination.PageTotal)
                {
                    break;
                }

                page++;

                Thread.Sleep(1000 / 19);
            } while (true);

            Logger.Info($"xivapi.com/status language={language} loaded.");
            return list;
        }

        public Action GetActionInfo(
            int id)
            => this.actionDictionary != null ?
                (this.actionDictionary.ContainsKey(id) ? this.actionDictionary[id] : null) :
                null;

        private async Task<Dictionary<int, Action>> LoadActionAsync(
            string language)
        {
            var list = new Dictionary<int, Action>(20480);
            var page = 1;

            do
            {
                var res = await BaseUri
                    .AppendPathSegment("action")
                    .SetQueryParams(new
                    {
                        limit = 3000,
                        language = language,
                        columns = "ID,Name,ActionCategory.ID,Icon",
                        page = page,
                    })
                    .GetAsync();

                var result = JsonConvert.DeserializeObject(
                    res.Content.ReadAsStringAsync().Result)
                    as dynamic;

                foreach (var item in result.Results)
                {
                    var name = (string)item.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var category = (string)item.ActionCategory?.ID;
                    if (!int.TryParse(category, out int categoryID))
                    {
                        categoryID = 0;
                    }

                    var entry = new Action()
                    {
                        ID = item.ID,
                        Name = name,
                        ActionCategory = categoryID,
                        Icon = item.Icon,
                    };

                    list.Add(entry.ID, entry);
                }

                if (result.Pagination.Page >= result.Pagination.PageTotal)
                {
                    break;
                }

                page++;

                Thread.Sleep(1000 / 19);
            } while (true);

            Logger.Info($"xivapi.com/action language={language} loaded.");
            return list;
        }
    }
}
