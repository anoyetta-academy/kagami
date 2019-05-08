using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using Advanced_Combat_Tracker;
using kagami.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace kagami.ViewModels
{
    public class KagamiConfigViewModel : BindableBase
    {
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
                KagamiAddon.Current.ResourcesDirectory,
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
            KagamiAddon.Current.Overlay.Navigate(this.config.Url);
            SystemSounds.Asterisk.Play();
        }

        private DelegateCommand openDeveloperToolCommand;

        public DelegateCommand OpenDeveloperToolCommand =>
            this.openDeveloperToolCommand ?? (this.openDeveloperToolCommand = new DelegateCommand(this.ExecuteOpenDeveloperToolCommand));

        private void ExecuteOpenDeveloperToolCommand()
        {
            KagamiAddon.Current.Overlay.Overlay.Renderer.showDevTools();
        }

        #endregion Utility commands
    }
}
