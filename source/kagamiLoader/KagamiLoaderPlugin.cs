using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace kagamiLoader
{
    public class KagamiLoaderPlugin : IActPluginV1
    {
        private TabPage pluginPage;
        private Label pluginStatusLabel;
        private string pluginPath;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            this.pluginPage = pluginScreenSpace;
            this.pluginStatusLabel = pluginStatusText;

            (this.pluginPage.Parent as TabControl).TabPages.Remove(pluginScreenSpace);

            foreach (var plugin in ActGlobals.oFormActMain.ActPlugins)
            {
                if (plugin.pluginObj == this)
                {
                    this.pluginPath = plugin.pluginFile.FullName;
                    break;
                }
            }

            var status = "Error.";

            if (!string.IsNullOrEmpty(this.pluginPath))
            {
                var dll = Path.Combine(
                    Path.GetDirectoryName(this.pluginPath),
                    "kagami.dll");

                if (File.Exists(dll))
                {
                    Assembly.LoadFrom(dll);
                    status = "Ready.";
                }
            }

            this.pluginStatusLabel.Text = status;
        }

        public void DeInitPlugin()
        {
            this.pluginStatusLabel.Text = "Exited.";
        }
    }
}
