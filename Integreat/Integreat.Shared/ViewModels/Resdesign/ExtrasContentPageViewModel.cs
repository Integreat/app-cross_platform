﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Integreat.Shared.Data.Loader;
using Integreat.Shared.Models;
using Integreat.Shared.Services;
using Integreat.Shared.Services.Persistence;
using Integreat.Shared.Services.Tracking;
using Integreat.Shared.Utilities;
using Integreat.Shared.ViewModels.Resdesign.General;
using Xamarin.Forms;
using Localization;

namespace Integreat.Shared.ViewModels.Resdesign {
    public class ExtrasContentPageViewModel : BaseContentViewModel {
        private ObservableCollection<ExtraAppEntry> _extras;
        private INavigator _navigator;
        private String _noteInternetText;
        private BaseContentViewModel _activeViewModel;
        private Func<string, bool, GeneralWebViewPageViewModel> _generalWebViewFactory;
        private ICommand _itemTappedCommand;

        #region Fields

        #endregion

        #region Properties

        public ObservableCollection<ExtraAppEntry> Extras {
            get { return _extras; }
            private set { SetProperty(ref _extras, value); }
        }

        public ICommand ItemTappedCommand
        {
            get { return _itemTappedCommand; }
            set { SetProperty(ref _itemTappedCommand, value); }
        }

        #endregion


        public ExtrasContentPageViewModel(IAnalyticsService analytics, INavigator navigator, DataLoaderProvider dataLoaderProvider
            , Func<Careers4RefugeesViewModel> careers4RefugeesFactory
            , Func<SprungbrettViewModel> sprungbrettFactory
            , Func<string, bool, GeneralWebViewPageViewModel> generalWebViewFactory)
            : base(analytics, dataLoaderProvider) {
            NoteInternetText = AppResources.NoteInternet;
            Title = AppResources.Extras;
            _navigator = navigator;
            _generalWebViewFactory = generalWebViewFactory;
            ItemTappedCommand = new Command(InvokeOnTap);

            Extras = new ObservableCollection<ExtraAppEntry>();
            Extras.Add(new ExtraAppEntry { Thumbnail = "careers4refugees_de_icon.jpg", ViewModelFactory = careers4RefugeesFactory, Title = "Careers 4 Refugees", OnTapCommand = new Command(OnExtraTap) });
            Extras.Add(new ExtraAppEntry { Thumbnail = "sbi_integreat_quadratisch_farbe.jpg", ViewModelFactory = sprungbrettFactory, Title = "Sprungbrett", OnTapCommand = new Command(OnExtraTap) });
            Extras.Add(new ExtraAppEntry {Thumbnail = "serloabc.jpg", Title = "Serlo ABC", ViewModelFactory = null, OnTapCommand = new Command(OnSerloTapped)});

        }

        public string NoteInternetText
        {
            get { return _noteInternetText; }
            set { SetProperty(ref _noteInternetText, value); }
        }

        private void InvokeOnTap(object obj)
        {
            var extraAppEntry = obj as ExtraAppEntry;
            extraAppEntry?.OnTapCommand?.Execute(obj);
        }

        private async void OnSerloTapped(object obj)
        {
            // push a new general webView page, which will show the URL of the offer
            await _navigator.PushAsync(_generalWebViewFactory("https://abc.serlo.org/try/#1", false), Navigation);
        }

        private async void OnExtraTap(object obj) {
            var asExtraAppEntry = obj as ExtraAppEntry;
            if (asExtraAppEntry == null) return;

            // push page on stack
            var vm = asExtraAppEntry.ViewModelFactory() as BaseContentViewModel;
            _activeViewModel = vm;
            _activeViewModel?.RefreshCommand.Execute(false);
            await _navigator.PushAsync(vm, Navigation);
        }

        protected override void LoadContent(bool forced = false, Language forLanguage = null, Location forLocation = null) {
            _activeViewModel?.RefreshCommand.Execute(forced);
        }
    }
}
