﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Integreat.Shared.Models;
using Integreat.Shared.Models.Extras;
using Integreat.Shared.Utilities;

namespace Integreat.Shared.Data.Loader.Targets
{
    /// <inheritdoc />
    public class ExtrasDataLoader : IDataLoader
    {
        public const string FileNameConst = "extrasV3";

        /// <inheritdoc />
        public string FileName => FileNameConst;

        public DateTime LastUpdated
        {
            get => Preferences.LastPageUpdateTime<Extra>(_lastLoadedLanguage, _lastLoadedLocation);
            // ReSharper disable once ValueParameterNotUsed
            set => Preferences.SetLastPageUpdateTime<Extra>(_lastLoadedLanguage, _lastLoadedLocation, DateTime.Now);
        }

        /// <inheritdoc />
        public string Id => null;

        private readonly IDataLoadService _dataLoadService;
        private Location _lastLoadedLocation;
        private Language _lastLoadedLanguage;

        public ExtrasDataLoader(IDataLoadService dataLoadService)
        {
            _dataLoadService = dataLoadService;
        }

        /// <summary> Loads the event pages. </summary>
        /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
        /// <param name="forLanguage">Which language to load for.</param>
        /// <param name="forLocation">Which location to load for.</param>
        /// <param name="errorLogAction">The error log action.</param>
        /// <returns>Task to load the event pages.</returns>
        public Task<Collection<Extra>> Load(bool forceRefresh, Language forLanguage, Location forLocation,
            Action<string> errorLogAction = null)
        {
            _lastLoadedLocation = forLocation;
            _lastLoadedLanguage = forLanguage;

            return DataLoaderProvider.ExecuteLoadMethod(forceRefresh, this,
                () => _dataLoadService.GetExtras(forLanguage, forLocation),
                errorLogAction);
        }
    }
}