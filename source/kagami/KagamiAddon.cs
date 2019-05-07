using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using kagami.Helpers;
using Prism.Mvvm;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiAddon :
        BindableBase,
        IOverlayAddon
    {
        private static readonly Lazy<KagamiAddon> LazyInstance = new Lazy<KagamiAddon>();

        public static KagamiAddon Instance => LazyInstance.Value;

        private string resourcesDirectory;

        public string ResourcesDirectory
        {
            get => this.resourcesDirectory;
            set => this.SetProperty(ref this.resourcesDirectory, value);
        }

        private string updateMessage;

        public string UpdateMessage
        {
            get => this.updateMessage;
            set => this.SetProperty(ref this.updateMessage, value);
        }

        private KagamiOverlayConfig config;

        public KagamiOverlayConfig Config
        {
            get => this.config;
            private set => this.SetProperty(ref this.config, value);
        }

        private KagamiOverlay overlay;

        public KagamiOverlay Overlay
        {
            get => this.overlay;
            private set => this.SetProperty(ref this.overlay, value);
        }

        public KagamiAddon()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

            var asm = Assembly.GetCallingAssembly();
            if (string.IsNullOrEmpty(asm.Location))
            {
                asm = Assembly.GetExecutingAssembly();
            }

            this.ResourcesDirectory = Path.Combine(Path.GetDirectoryName(asm.Location), "resources");
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly tryLoadAssembly(
                string directory,
                string extension)
            {
                var asm = new AssemblyName(args.Name);

                var asmPath = Path.Combine(directory, asm.Name + extension);
                if (File.Exists(asmPath))
                {
                    return Assembly.LoadFrom(asmPath);
                }

                return null;
            }

            var dir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "bin");

            foreach (var directory in new[] { dir })
            {
                var asm = tryLoadAssembly(directory, ".dll");
                if (asm != null)
                {
                    return asm;
                }
            }

            return null;
        }

        public string Name => $"kagami";

        public string Description => throw new NotImplementedException();

        public Type OverlayType => typeof(KagamiOverlay);

        public Type OverlayConfigType => typeof(KagamiOverlayConfig);

        public Type OverlayConfigControlType => typeof(KagamiOverlayConfigPanel);

        public Control CreateOverlayConfigControlInstance(IOverlay overlay) => new KagamiOverlayConfigPanel(overlay as KagamiOverlay);

        public IOverlayConfig CreateOverlayConfigInstance(string name) => this.Config = new KagamiOverlayConfig(name);

        public IOverlay CreateOverlayInstance(IOverlayConfig config)
        {
            this.Overlay = new KagamiOverlay(config as KagamiOverlayConfig);
            this.Initialize();
            return this.Overlay;
        }

        public void Dispose()
        {
            FFXIVPluginHelper.Instance.Stop();
            SharlayanHelper.Instance.Stop();

            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }

        private void Initialize()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);

                FFXIVPluginHelper.Instance.Start();
                SharlayanHelper.Instance.Start();
            });
        }
    }
}
