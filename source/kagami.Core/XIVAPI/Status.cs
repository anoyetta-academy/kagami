namespace kagami.XIVAPI
{
    public class Status
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }
        public string Icon { get; set; }

        private int? iconCode;
        public int IconCode => (this.iconCode ??= IconHelper.GetCode(this.Icon)).Value;

        public string IconUri => IconHelper.GetIconUri(this.Icon);
    }
}
