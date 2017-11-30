﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Integreat.Shared.Data.Loader;
using Integreat.Shared.Models;
using Integreat.Shared.Models.Extras.Sprungbrett;
using Integreat.Shared.Services;
using Integreat.Shared.Utilities;
using Integreat.Shared.ViewModels;
using Integreat.Shared.ViewModels.General;
using Xamarin.Forms;

namespace Integreat.Shared
{
    /// <summary>
    /// ViewModel for sprungbrett extra
    /// </summary>
    public class SprungbrettViewModel : BaseContentViewModel
    {
        #region Fields
        private readonly INavigator _navigator;

        private ObservableCollection<SprungbrettJobOffer> _offers;
        private bool _hasNoResults;
        private readonly Func<string, GeneralWebViewPageViewModel> _generalWebViewFactory; // factory generated by AutoFac to resolve a GeneralWebViewPageViewModel instance
        private readonly ISprungbrettParser _parser;

        #endregion

        public SprungbrettViewModel(INavigator navigator,
            DataLoaderProvider dataLoaderProvider,
            Func<string, GeneralWebViewPageViewModel> generalWebViewFactory, 
            ISprungbrettParser parser)
            : base(dataLoaderProvider)
        {
            Title = "Sprungbrett";
            HeaderImage = "sbi_logo";
            _navigator = navigator;
            _generalWebViewFactory = generalWebViewFactory;
            _parser = parser;

            _navigator.HideToolbar(this);
        }

        #region Properties
        public ObservableCollection<SprungbrettJobOffer> Offers
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
        /// The displayed header image on the page
        /// </summary>
        public string HeaderImage { get; }

        #endregion

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

            if (forLocation == null) forLocation = LastLoadedLocation;

            var url = forLocation.SprungbrettUrl;

            if (string.IsNullOrEmpty(url))
            {
                url = "https://www.sprungbrett-intowork.de/ajax/app-search-internships";
            }
            try
            {
                var json = await _parser.FetchJobOffersAsync(url);

                var offers = new ObservableCollection<SprungbrettJobOffer>(json.JobOffers);

                foreach (var jobOffer in offers)
                {
                    jobOffer.OnTapCommand = new Command(OnOfferTapped);
                    jobOffer.OnSelectCommand = new Command(OnSelectionTapped);
                }

                Offers = offers;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                HasNoResults = true;
            }
            finally { IsBusy = false; }

            IsBusy = false;
        }

        /// <summary>
        /// Called when an [offer is tapped]. (By a command)
        /// </summary>
        /// <param name="jobOfferObject">The job offer object.</param>
        private async void OnOfferTapped(object jobOfferObject)
        {
            // try to cast the object, abort if failed
            if (!(jobOfferObject is SprungbrettJobOffer jobOffer)) return;
            jobOffer.IsSelected = true;
            var view = _generalWebViewFactory(jobOffer.Url);
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
            if (!(offerObject is SprungbrettJobOffer offer)) return;
            offer.IsSelected = !offer.IsSelected;
            // push a new general webView page, which will show the URL of the offer
        }
    }
}
