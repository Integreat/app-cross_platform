﻿using System.Xml.Serialization;
using Xamarin.Forms;

namespace App1.Models.Extras
{
    public class OfferBase
    {
        //detect click on list list item
        [XmlIgnore]
        public Command OnTapCommand { get; set; }
        // identify if an object is selected
        [XmlIgnore]
        public bool IsSelected { get; set; }
        // detect selection
        [XmlIgnore]
        public Command OnSelectCommand { get; set; }
    }
}
