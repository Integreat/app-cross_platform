﻿using Android.Content;
using Integreat.Droid.CustomRenderer;
using Integreat.Shared.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(PdfWebView), typeof(PdfWebViewRenderer))]
namespace Integreat.Droid.CustomRenderer
{
    /// <summary>
    /// This Class is used to render PDF views on android devices
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Platform.Android.WebViewRenderer" />
    /// <inheritdoc />
    public class PdfWebViewRenderer : ZoomingWebViewRenderer
    {
        public PdfWebViewRenderer(Context context) : base(context)
        { }
        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            // resolve the control as the pdfWebView
            if (e.NewElement == null) return;
            if (!(Element is PdfWebView pdfWebView)) return;

            Control.Settings.AllowFileAccess = true;
            Control.Settings.AllowFileAccessFromFileURLs = true;
            Control.Settings.AllowUniversalAccessFromFileURLs = true;

            // if the target is an online pdf, use the google docs pdf viewer (there is also a online version of PDF.js, however it does not easily support cross-domain urls)
            if (pdfWebView.Uri.StartsWith("http"))
            {
                var target = $"https://docs.google.com/gview?embedded=true&url={pdfWebView.Uri}";
                Control.LoadUrl(target);
                Control.Reload();
            }
            else
            {
                // otherwise (local pdf) use the local pdf viewer (PDF.js) instead
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                Control.LoadUrl($"file:///android_asset/web/viewer.html?file={pdfWebView.Uri}");
                Control.Reload();
            }
        }
    }
}