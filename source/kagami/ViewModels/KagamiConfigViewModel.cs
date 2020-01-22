using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Combat_Tracker;
using kagami.Helpers;
using kagami.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace kagami.ViewModels
{
    public class KagamiConfigViewModel : BindableBase
    {
        private KagamiOverlay overlay;

        public KagamiOverlay Overlay
        {
            get => this.overlay;
            set => this.SetProperty(ref this.overlay, value);
        }

        private KagamiOverlayConfig config;

        public KagamiOverlayConfig Config
        {
            get => this.config;
            set => this.SetProperty(ref this.config, value);
        }

        private static readonly System.Windows.Forms.OpenFileDialog OpenFileDialog = new System.Windows.Forms.OpenFileDialog()
        {
            RestoreDirectory = true,
            Filter = "All files (*.*)|*.*",
            InitialDirectory = Path.Combine(
                KagamiAddon.Instance.ResourcesDirectory,
                "kagami"),
        };

        private static readonly System.Windows.Forms.OpenFileDialog OpenJsonFileDialog = new System.Windows.Forms.OpenFileDialog()
        {
            RestoreDirectory = true,
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
        };

        private static readonly System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

        private DelegateCommand browseUrlCommand;

        public DelegateCommand BrowseUrlCommand =>
            this.browseUrlCommand ?? (this.browseUrlCommand = new DelegateCommand(this.ExecuteBrowseUrlCommand));

        private void ExecuteBrowseUrlCommand()
        {
            var result = OpenFileDialog.ShowDialog(ActGlobals.oFormActMain);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.Config.Url = new Uri(OpenFileDialog.FileName).AbsoluteUri;
            }
        }

        private DelegateCommand browseLogFolderCommand;

        public DelegateCommand BrowseLogFolderCommand =>
            this.browseLogFolderCommand ?? (this.browseLogFolderCommand = new DelegateCommand(this.ExecuteBrowseLogFolderCommand));

        private void ExecuteBrowseLogFolderCommand()
        {
            if (!string.IsNullOrEmpty(this.Config.LogDirectory))
            {
                FolderBrowserDialog.SelectedPath = this.Config.LogDirectory;
            }

            var result = FolderBrowserDialog.ShowDialog(ActGlobals.oFormActMain);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.Config.LogDirectory = FolderBrowserDialog.SelectedPath;
            }
        }

        private DelegateCommand openLogFolderCommand;

        public DelegateCommand OpenLogFolderCommand =>
            this.openLogFolderCommand ?? (this.openLogFolderCommand = new DelegateCommand(this.ExecuteOpenLogFolderCommand));

        private void ExecuteOpenLogFolderCommand()
        {
            if (Directory.Exists(this.Config.LogDirectory))
            {
                Process.Start(this.Config.LogDirectory);
            }
        }

        private DelegateCommand saveLogCommand;

        public DelegateCommand SaveLogCommand =>
            this.saveLogCommand ?? (this.saveLogCommand = new DelegateCommand(this.ExecuteSaveLogCommand));

        private async void ExecuteSaveLogCommand()
        {
            await ActionEchoesModel.Instance.SaveLogAsync();
            SystemSounds.Asterisk.Play();
        }

        private DelegateCommand browseGhostFileCommand;

        public DelegateCommand BrowseGhostFileCommand =>
            this.browseGhostFileCommand ?? (this.browseGhostFileCommand = new DelegateCommand(this.ExecuteBrowseGhostFileCommand));

        private void ExecuteBrowseGhostFileCommand()
        {
            var dir = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.Config.GhostLogFile) &&
                File.Exists(this.Config.GhostLogFile))
            {
                dir = Path.GetDirectoryName(this.Config.GhostLogFile);
            }
            else
            {
                if (!string.IsNullOrEmpty(Config.LogDirectory) &&
                    Directory.Exists(Config.LogDirectory))
                {
                    dir = Config.LogDirectory;
                }
            }

            OpenJsonFileDialog.InitialDirectory = dir;

            var result = OpenJsonFileDialog.ShowDialog(ActGlobals.oFormActMain);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.Config.GhostLogFile = OpenJsonFileDialog.FileName;
            }
        }

        private DelegateCommand openGhostFileCommand;

        public DelegateCommand OpenGhostFileCommand =>
            this.openGhostFileCommand ?? (this.openGhostFileCommand = new DelegateCommand(this.ExecuteOpenGhostFileCommand));

        private void ExecuteOpenGhostFileCommand()
        {
            if (File.Exists(this.Config.GhostLogFile))
            {
                Process.Start(this.Config.GhostLogFile);
            }
        }

        #region Utility commands

        private DelegateCommand copyDataModelCommand;

        public DelegateCommand CopyDataModelCommand =>
            this.copyDataModelCommand ?? (this.copyDataModelCommand = new DelegateCommand(this.ExecuteCopyDataModelCommand));

        private async void ExecuteCopyDataModelCommand()
        {
            var json = await ActionEchoesModel.Instance.ParseJsonAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var updateScript =
                    $"var model =\n{ json };\n\n" +
                    "document.dispatchEvent(new CustomEvent('onOverlayDataUpdate', { detail: model }));\n";

                Clipboard.SetDataObject(updateScript);
                SystemSounds.Asterisk.Play();
            }
        }

        private DelegateCommand reloadOverlayCommand;

        public DelegateCommand ReloadOverlayCommand =>
            this.reloadOverlayCommand ?? (this.reloadOverlayCommand = new DelegateCommand(this.ExecuteReloadOverlayCommand));

        private void ExecuteReloadOverlayCommand()
        {
            this.Overlay.Navigate(this.config.Url);
            this.Overlay.ClearJsonCache();
            SystemSounds.Asterisk.Play();
        }

        private DelegateCommand openDeveloperToolCommand;

        public DelegateCommand OpenDeveloperToolCommand =>
            this.openDeveloperToolCommand ?? (this.openDeveloperToolCommand = new DelegateCommand(this.ExecuteOpenDeveloperToolCommand));

        private void ExecuteOpenDeveloperToolCommand()
        {
            this.Overlay.Overlay.Renderer.showDevTools();
        }

        #endregion Utility commands

        #region Download action icons

        private DelegateCommand downloadActionIconsCommand;

        public DelegateCommand DownloadActionIconsCommand =>
            this.downloadActionIconsCommand ?? (this.downloadActionIconsCommand = new DelegateCommand(this.ExecuteDownloadActionIconsCommand));

        private static readonly uint ActionIconMaxCount = 200000;
        public static readonly string ActionIconCodeFormat = "000000";

        private async void ExecuteDownloadActionIconsCommand()
        {
            await DownloadActionsIconsAsync();
        }

        private async Task DownloadActionsIconsAsync() => await Task.Run(async () =>
        {
            var iconCodeList = new List<int>((int)ActionIconMaxCount);

            for (uint i = 0; i <= ActionIconMaxCount; i++)
            {
                var info = SharlayanHelper.Instance.GetActionInfo(i);
                if (info != null)
                {
                    iconCodeList.Add(info.Icon);
                }

                await Task.Delay(0);
            }

            iconCodeList = iconCodeList.Distinct().ToList();

            Logger.Info($"Download {iconCodeList.Count:N0} action icons.");

            using (var wc = new WebClient())
            {
                var counter = 0;
                foreach (var iconCode in iconCodeList)
                {
                    var directoryCode = (iconCode / 1000) * 1000;

                    var uri = $"https://xivapi.com/i/{formatCode(directoryCode)}/{formatCode(iconCode)}.png";
                    var destination = Path.Combine(
                        KagamiAddon.Instance.ResourcesDirectory,
                        "kagami",
                        "action_icons",
                        $"{formatCode(iconCode)}.png");

                    if (File.Exists(destination))
                    {
                        File.Delete(destination);
                    }

                    await wc.DownloadFileTaskAsync(uri, destination);

                    counter++;

                    if ((counter % 20) == 0)
                    {
                        Logger.Info($"{counter:N0} / {iconCodeList.Count:N0}");
                        await Task.Delay(500);
                    }
                }

                Logger.Info($"{counter:N0} / {iconCodeList.Count:N0}, Download is completed.");
            }

            string formatCode(int code) => code.ToString(ActionIconCodeFormat);
        });

        #endregion Download action icons
    }
}
