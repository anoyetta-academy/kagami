using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using kagami.Helpers;
using RainbowMage.OverlayPlugin;

namespace kagami
{
    public class KagamiAddon :
        IOverlayAddon
    {
        public static KagamiAddon Current
        {
            get;
            private set;
        }

        public string ResourcesDirectory
        {
            get;
            private set;
        }

        public KagamiOverlayConfig Config
        {
            get;
            private set;
        }

        public KagamiOverlay Overlay
        {
            get;
            private set;
        }

        public KagamiAddon()
        {
            Current = this;

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

        public IOverlayConfig CreateOverlayConfigInstance(string name) => new KagamiOverlayConfig(name);

        public IOverlay CreateOverlayInstance(IOverlayConfig config)
        {
            this.Config = config as KagamiOverlayConfig;
            this.Initialize();
            this.Overlay = new KagamiOverlay(this.Config);
            return this.Overlay;
        }

        public void Dispose()
        {
            XIVLogSubscriber.Instance.Stop();
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

                // Actionリストを取得しておく
                var a = SharlayanHelper.Instance.GetActionInfo(0);

                await Task.Delay(100);

                XIVLogSubscriber.Instance.Start();
            });
        }
    }
}
