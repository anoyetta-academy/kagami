using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiAddon :
        IOverlayAddon
    {
        static KagamiAddon()
        {
            AssemblyResolver.Initialize();
        }

        public KagamiAddon()
        {
            this.core = new KagamiAddonCore();

            var asm = Assembly.GetCallingAssembly();
            if (string.IsNullOrEmpty(asm.Location))
            {
                asm = Assembly.GetExecutingAssembly();
            }

            this.core.ResourcesDirectory = Path.Combine(
                Path.GetDirectoryName(asm.Location),
                "resources");
        }

        private dynamic core;

        public string Name => this.core.Name;

        public string Description => this.core.Description;

        public Type OverlayType => this.core.OverlayType;

        public Type OverlayConfigType => this.core.OverlayConfigType;

        public Type OverlayConfigControlType => this.core.OverlayConfigControlType;

        public Control CreateOverlayConfigControlInstance(IOverlay overlay) => this.core.CreateOverlayConfigControlInstance(overlay);

        public IOverlayConfig CreateOverlayConfigInstance(string name) => this.core.CreateOverlayConfigInstance(name);

        public IOverlay CreateOverlayInstance(IOverlayConfig config) => this.core.CreateOverlayInstance(config);

        public void Dispose()
        {
            this.core.Dispose();
            this.core = null;
        }
    }
}
