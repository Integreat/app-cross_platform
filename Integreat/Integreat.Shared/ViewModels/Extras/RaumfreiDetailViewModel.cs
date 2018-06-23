﻿using Integreat.Shared.Models;
using Integreat.Shared.Models.Extras.Raumfrei;
using Integreat.Shared.ViewFactory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Integreat.Shared.ViewModels
{
    public class RaumfreiDetailViewModel : BaseViewModel
    {
        public RaumfreiOffer Offer { get; }

        public RaumfreiDetailViewModel(RaumfreiOffer offer) : base()
        {
            Offer = offer;
            Title = "Mietangebot";
            HeaderImage = "raumfrei_logo";
        }

        public string HeaderImage { get; set; }

    }
}