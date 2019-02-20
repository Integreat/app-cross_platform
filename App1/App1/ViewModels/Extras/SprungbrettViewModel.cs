﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App1.Data.Loader;
using App1.Models;
using App1.Models.Extras.Sprungbrett;
using App1.Navigator;
using App1.Utilities;
using App1.ViewModels.General;
using Integreat.Localization;
using Xamarin.Forms;

namespace App1.ViewModels.Extras
{
    /// <summary>
    /// ViewModel for sprungbrett extra
    /// </summary>
    public class SprungbrettViewModel : BaseContentViewModel
    {
        #region Fields
        private readonly INavigator _navigator;

        private ObservableCollection<ListItemViewModel<SprungbrettJobOffer>> _offers;
        private readonly string _url;
        private bool _hasNoResults;
        private readonly Func<string, GeneralWebViewPageViewModel> _generalWebViewFactory; // factory generated by AutoFac to resolve a GeneralWebViewPageViewModel instance
        private readonly IParser _parser;

        #endregion

        public SprungbrettViewModel(INavigator navigator,
            DataLoaderProvider dataLoaderProvider,
            string url,
            Func<string, GeneralWebViewPageViewModel> generalWebViewFactory, 
            IParser parser)
            : base(dataLoaderProvider)
        {
            Title = "Sprungbrett";
            HeaderImage = "sbi_logo";
            _navigator = navigator;
            _generalWebViewFactory = generalWebViewFactory;
            _parser = parser;

            _url = url;
            _navigator.HideToolbar(this);
        }

        #region Properties
        public ObservableCollection<ListItemViewModel<SprungbrettJobOffer>> Offers
        {
            get => _offers;
            private set => SetProperty(ref _offers, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has no results for the given location or not.
        /// </summary>
        public bool HasNoResults
        {
            get => _hasNoResults;
            set => SetProperty(ref _hasNoResults, value);
        }

        /// <summary>
        /// Gets the has no results label.
        /// </summary>
        public string HasNoResultsLabel => AppResources.HasNoResults;

        /// <summary>
        /// The displayed header image on the page
        /// </summary>
        public string HeaderImage { get; }

        #endregion

        //we need this because it won't refresh during the first appearience
        public override void OnAppearing()
        {
            this.RefreshCommand.Execute(null);
            base.OnAppearing();
        }

        protected override async void LoadContent(bool forced = false, Language forLanguage = null, Location forLocation = null)
        {
            Offers?.Clear();
            // wait until this resource is free
            await Task.Run(() =>
            {
                while (IsBusy) { /*empty body*/ }
            });
            IsBusy = true;
            HasNoResults = true;

            try
            {
                var json = await _parser.FetchAsync<SprungbrettRootObject>(_url);

                var offerItems = new ObservableCollection<ListItemViewModel<SprungbrettJobOffer>>(
                    json.JobOffers.Select(x => new ListItemViewModel<SprungbrettJobOffer>()
                    {
                        ListItemModel = x,
                        OnTapCommand = new Command(OnOfferTapped),
                        OnSelectCommand = new Command(OnSelectionTapped)
                    }));

                Offers = offerItems;
                if (Offers.Count > 0)
                    HasNoResults = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                HasNoResults = true;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// Called when an [offer is tapped]. (By a command)
        /// </summary>
        /// <param name="jobOfferObject">The job offer object.</param>
        private async void OnOfferTapped(object jobOfferObject)
        {
            // try to cast the object, abort if failed
            if (!(jobOfferObject is ListItemViewModel<SprungbrettJobOffer> jobOffer)) return;
            jobOffer.IsSelected = true;
            var view = _generalWebViewFactory(jobOffer.ListItemModel.Url);
            view.Title = "Sprungbrett";
            // push a new general webView page, which will show the URL of the offer
            await _navigator.PushAsync(view, Navigation);
        }

        /// <summary>
        /// Called when an [select button] from an offer is tapped. (By a command)
        /// </summary>
        /// <param name="offerObject">The career offer object.</param>
        private void OnSelectionTapped(object offerObject)
        {
            // try to cast the object, abort if failed
            if (!(offerObject is ListItemViewModel<SprungbrettJobOffer> offer)) return;
            offer.IsSelected = !offer.IsSelected;
            // push a new general webView page, which will show the URL of the offer
        }
    }
}
