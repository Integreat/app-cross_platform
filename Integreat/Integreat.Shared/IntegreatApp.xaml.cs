﻿using System.Security;
using Autofac;
using Integreat.ApplicationObject;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using System;
using System.Linq;
using Integreat.Shared.Models;
using Integreat.Utilities;
using System.Threading.Tasks;
using Integreat.Shared.Utilities;
using System.Diagnostics;
using Autofac.Core;
using Integreat.Shared.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Integreat.Shared
{
    [SecurityCritical]
    public partial class IntegreatApp : Application
    {
        private AppSetup _app;

        [SecurityCritical]
        public IntegreatApp(ContainerBuilder builder)
        {
            InitializeComponent();
            this._app = new AppSetup(this, builder);
            _app.Run();
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            string appDomain = Constants.IntegreatReleaseUrl;
            if (!uri.ToString().ToLower().StartsWith(appDomain, StringComparison.Ordinal)||this._app==null)
                return;
            /*
            string[] segments = uri.Segments.Where(s => s != "/").ToArray().Select(s => s.Trim(new Char[] { '/' })).ToArray();

            //regensburg/de/site
            String locationShortname = !segments[0].IsNullOrEmpty() ? segments[0] : String.Empty;
            String languageShortname = !segments[1].IsNullOrEmpty() ? segments[1] : String.Empty;

            //get location
            if (locationShortname.IsNullOrEmpty()||locationShortname == "event")
                base.OnAppLinkRequestReceived(uri);
            
            ShortnameParser shortnameparser = new ShortnameParser();
            Location location = Task.Run(() => shortnameparser.getLocation(locationShortname)).Result;

            //get language
            Language language = null;
            if(!languageShortname.IsNullOrEmpty()){
                language = Task.Run(() => shortnameparser.getLanguage(languageShortname, location)).Result;
            }

            Debug.WriteLine(location.Name);

            //webapp url to cms url
        */
            DeepLinkService deeplinkservice = (DeepLinkService)this._app.Container.Resolve<IDeepLinkService>();
            if (deeplinkservice == null)
                return;
            deeplinkservice.Url = uri;
            try{
                deeplinkservice.Navigate();
            }catch(Exception e){
                Debug.WriteLine(e.Message);
            }
            base.OnAppLinkRequestReceived(uri);
        }
    }
}
