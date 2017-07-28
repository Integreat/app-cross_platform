﻿using System;
using System.Linq;
using System.Windows.Input;
using Integreat.Shared.ApplicationObjects;
using Integreat.Shared.Data.Loader;
using Integreat.Shared.Models;
using Integreat.Shared.Services;
using Integreat.Shared.Services.Tracking;
using Integreat.Shared.Utilities;
using Integreat.Shared.ViewModels.Resdesign.General;
using Integreat.Utilities;
using localization;
using Xamarin.Forms;

namespace Integreat.Shared.ViewModels.Resdesign.Settings
{
    public class SettingsPageViewModel : BaseContentViewModel
    {
        private string _settingsStatusText;
        private string _cacheSizeText;
        private INavigator _navigator;
        private readonly Func<string, GeneralWebViewPageViewModel> _generalWebViewFactory;
        private string _disclaimerContent; // HTML text for the disclaimer

        /// <summary>
        /// Gets the disclaimer text.
        /// </summary>
        public string DisclaimerText => AppResources.Disclaimer;

        /// <summary>
        /// Gets the clear cache text.
        /// </summary>
        public string ClearCacheText => AppResources.ClearCache;

        /// <summary>
        /// Gets the cache size text.
        /// </summary>
        public string CacheSizeText
        {
            get => _cacheSizeText;
            private set => SetProperty(ref _cacheSizeText, value);
        }

        /// <summary>
        /// Gets the reset settings text.
        /// </summary>
        public string ResetSettingsText => AppResources.ResetSettings;

        /// <summary>
        /// Gets or sets the settings status text, used to give the user feedback that the settings clearance was successful.
        /// </summary>
        public string SettingsStatusText
        {
            get { return _settingsStatusText; }
            set { SetProperty(ref _settingsStatusText, value); }
        }

        public ICommand ClearCacheCommand { get; set; }
        public ICommand ResetSettingsCommand { get; set; }
        public ICommand OpenDisclaimerCommand { get; set; }

        public SettingsPageViewModel(IAnalyticsService analyticsService, INavigator navigator, DataLoaderProvider dataLoaderProvider
            , IViewFactory viewFactory, Func<string, GeneralWebViewPageViewModel> generalWebViewFactory) : base(analyticsService,dataLoaderProvider)
        {
            _navigator = navigator;
            _generalWebViewFactory = generalWebViewFactory;

            Title = AppResources.Settings;
            ClearCacheCommand = new Command(ClearCache);
            ResetSettingsCommand = new Command(ResetSettings);
            OpenDisclaimerCommand = new Command(OpenDisclaimer);
            UpdateCacheSizeText();

            OnRefresh();
        }

        private async void UpdateCacheSizeText()
        {
            CacheSizeText = $"{AppResources.CacheSize} {AppResources.Calculating}";

            // count files async
            var fileSize = await DirectorySize.CalculateDirectorySizeAsync(Constants.CachedFilePath+"/");

            // parse the bytes into an readable string
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            // while the size is dividable by 1024
            while (fileSize >= 1024 && order < sizes.Length - 1)
            {
                // increment the order
                order++;
                // and divide by 1024
                fileSize = fileSize / 1024;
            }

            // set the CachedSizeText with the updated value
            CacheSizeText = $"{AppResources.CacheSize} {fileSize:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Resets the settings.
        /// </summary>
        private void ResetSettings()
        {
            Cache.ClearSettings();
            SettingsStatusText = AppResources.SettingsReseted;
        }

        /// <summary>
        /// Opens the contacts page.
        /// </summary>
        private async void OpenDisclaimer()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(_disclaimerContent)) return;

            var viewModel = _generalWebViewFactory(_disclaimerContent);
            //trigger load content 
            viewModel?.RefreshCommand.Execute(false);
            await _navigator.PushAsync(viewModel, Navigation);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        private void ClearCache()
        {
            Cache.ClearCachedResources();
            Cache.ClearCachedContent();
            UpdateCacheSizeText();
        }

        protected override async void LoadContent(bool forced = false, Language forLanguage = null, Location forLocation = null)
        {
            // load the disclaimer text
            try
            {
                IsBusy = true;

                var pages = await _dataLoaderProvider.DisclaimerDataLoader.Load(true, LastLoadedLanguage, LastLoadedLocation);
                _disclaimerContent = string.Join("<br><br>", pages.Select(x => x.Content));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}