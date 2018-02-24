﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integreat.Shared.Models;
using Integreat.Shared.Utilities;

namespace Integreat.Shared.Data.Loader.Targets
{
    /// <inheritdoc />
    public class DisclaimerDataLoader : IDataLoader
    {
        public const string FileNameConst = "disclaimerV1";
        
        private readonly IDataLoadService _dataLoadService;
        private Location _lastLoadedLocation;
        private Language _lastLoadedLanguage;

        public DisclaimerDataLoader(IDataLoadService dataLoadService)
        {
            _dataLoadService = dataLoadService;
        }

        public string FileName => FileNameConst;
        public DateTime LastUpdated
        {
            get => Preferences.LastPageUpdateTime<Disclaimer>(_lastLoadedLanguage, _lastLoadedLocation);
            // ReSharper disable once ValueParameterNotUsed
            set => Preferences.SetLastPageUpdateTime<Disclaimer>(_lastLoadedLanguage, _lastLoadedLocation, DateTime.Now);
        }

        public string Id => "Id";

        /// <summary> Loads the disclaimer. </summary>
        /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
        /// <param name="forLanguage">Which language to load for.</param>
        /// <param name="forLocation">Which location to load for.</param>
        /// <param name="errorLogAction">The error log action.</param>
        /// <returns>Task to load the disclaimer.</returns>
        public Task<ICollection<Disclaimer>> Load(bool forceRefresh, Language forLanguage, Location forLocation, Action<string> errorLogAction = null)
        {
            _lastLoadedLocation = forLocation;
            _lastLoadedLanguage = forLanguage;

            Action<ICollection<Disclaimer>> worker = pages =>
            {
                foreach (var page in pages)
                {
                    page.PrimaryKey = Page.GenerateKey(page.Id, forLocation, forLanguage);

                    if (!"".Equals(page.ParentJsonId) && page.ParentJsonId != null)
                    {
                        page.ParentId = Page.GenerateKey(page.ParentJsonId, forLocation, forLanguage);
                    }
                }
            };

            // action which will be executed on the merged list of loaded and cached data
            Action<ICollection<Disclaimer>> persistWorker = pages =>
            {
                // remove all pages which status is "trash"
                var itemsToRemove = pages.Where(x => x.Status == "trash").ToList();
                foreach (var page in itemsToRemove)
                {
                    pages.Remove(page);
                }
            };

            return DataLoaderProvider.ExecuteLoadMethod(forceRefresh, this, () => _dataLoadService.GetDisclaimers(
                forLanguage, 
                forLocation, 
                new UpdateTime(LastUpdated.Ticks)), 
                errorLogAction, 
                worker, 
                persistWorker);
        }
    }
}