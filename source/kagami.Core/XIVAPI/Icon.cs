using System.Text.RegularExpressions;

namespace kagami.XIVAPI
{
    public static class IconHelper
    {
        private static readonly Regex IconUriRegex = new Regex(
            @"/i/(?<dir>{d}+)/(?<code>{d}+)\.png",
            RegexOptions.Compiled);

        public static int GetCode(
            string baseUri)
        {
            if (string.IsNullOrEmpty(baseUri))
            {
                return 0;
            }

            var match = IconUriRegex.Match(baseUri);
            if (!match.Success)
            {
                return 0;
            }

            if (!int.TryParse(match.Groups["code"].Value, out int code))
            {
                return 0;
            }

            return code;
        }

        public static string GetIconUri(
            string baseUri)
            => string.IsNullOrEmpty(baseUri) ?
                string.Empty :
                $"https://xivapi.com{baseUri}";
    }
}
