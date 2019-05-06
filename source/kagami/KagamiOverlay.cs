using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiOverlay : OverlayBase<KagamiOverlayConfig>
    {
        public KagamiOverlay(KagamiOverlayConfig config) : base(config, config.Name)
        {
        }

        protected override void Update()
        {
        }
    }
}
