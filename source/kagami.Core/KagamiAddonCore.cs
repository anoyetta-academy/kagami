namespace kagami
{
    public class KagamiAddonCore
    {
        public static KagamiAddonCore Current { get; private set; }

        public IKagamiOverlayConfig Config { get; set; }

        public string ResourcesDirectory { get; set; }

        public KagamiAddonCore()
        {
            Current = this;
        }
    }
}
