﻿using Integreat.Shared.Services;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Integreat.Shared.ViewModels
{
    public class NavigationViewModel : BaseViewModel
    {
        public ObservableCollection<PageViewModel> Pages { get; internal set; }
        private readonly INavigator _navigator;
        private readonly Func<DisclaimerViewModel> _disclaimerFactory;

        public string Thumbnail => "http://vmkrcmar21.informatik.tu-muenchen.de/wordpress/augsburg/wp-content/uploads/sites/2/2015/11/cropped-Augsburg.jpg";

        public NavigationViewModel(INavigator navigator, Func<DisclaimerViewModel> disclaimerFactory)
        {
            Console.WriteLine("NavigationViewModel initialized");
            Title = "Navigation";
            _navigator = navigator;
            Pages = new ObservableCollection<PageViewModel>();
            _disclaimerFactory = disclaimerFactory;
        }

        private Command _openDisclaimerCommand;

        public Command OpenDisclaimerCommand => _openDisclaimerCommand ??
                                                (_openDisclaimerCommand = new Command(OnOpenDisclaimerClicked));

        private async void OnOpenDisclaimerClicked()
        {
            await _navigator.PushModalAsync(_disclaimerFactory());
        }
    }
}
