using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiOverlay : OverlayBase<KagamiOverlayConfig>
    {
        public KagamiOverlay(KagamiOverlayConfig config) : base(config, config.Name)
        {
            // Loggerにコールバックを仕込む
            Logger.LogCallback += this.Log;
        }

        protected override void Update()
        {
        }
    }
}
