using System.IO;
using System.Net;
using System.Reflection;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiAddon :
        IOverlayAddonV2
    {
        static KagamiAddon()
        {
            CosturaUtility.Initialize();
            AssemblyResolver.Initialize();
        }

        public static KagamiAddon Instance { get; private set; }

        public KagamiAddon()
        {
            Instance = this;
        }

        public string Name => $"kagami";

        public string Description => "FFXIV skill rotation viewer.";

        public string ResourcesDirectory { get; private set; }

        public void Init()
        {
            ServicePointManager.DefaultConnectionLimit = 32;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            this.ResourcesDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "resources");

            Registry.RegisterOverlay<KagamiOverlay>();
        }
    }
}
