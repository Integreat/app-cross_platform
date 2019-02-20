﻿using Xamarin.Forms;

namespace App1.CustomRenderer
{
    /// <inheritdoc />
    /// <summary>
    /// Special WebView custom control, that allows the viewing of local pdfs on android and windows (iOS works anyway)
    /// </summary>
    public class PdfWebView : WebView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(propertyName: "Source",
            returnType: typeof(string),
            declaringType: typeof(PdfWebView),
            defaultValue: default(string));

        public string Uri
        {
            get => (string)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }
    }
}
