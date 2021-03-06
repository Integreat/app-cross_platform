﻿using Integreat.Localization;
using Integreat.Shared.Data.Loader.Targets;
using Integreat.Shared.Utilities;
using Integreat.Utilities;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Integreat.Shared.Data.Loader
{
    /// <summary>
    /// Class DataLoader
    /// </summary>
    public class DataLoaderProvider
    {
        private const int NoReloadTimeout = 4;
        public readonly DisclaimerDataLoader DisclaimerDataLoader;
        public readonly EventPagesDataLoader EventPagesDataLoader;
        public readonly ExtrasDataLoader ExtrasDataLoader;
        public readonly LanguagesDataLoader LanguagesDataLoader;
        public readonly LocationsDataLoader LocationsDataLoader;
        public readonly PagesDataLoader PagesDataLoader;

        private static readonly ConcurrentDictionary<string, bool> LoaderLocks = new ConcurrentDictionary<string, bool>();

        public DataLoaderProvider(DisclaimerDataLoader disclaimerDataLoader,
            EventPagesDataLoader eventPagesDataLoader,
            ExtrasDataLoader extrasDataLoader,
            LanguagesDataLoader languagesDataLoader,
            LocationsDataLoader locationsDataLoader,
            PagesDataLoader pagesDataLoader)
        {
            DisclaimerDataLoader = disclaimerDataLoader;
            EventPagesDataLoader = eventPagesDataLoader;
            ExtrasDataLoader = extrasDataLoader;
            LanguagesDataLoader = languagesDataLoader;
            LocationsDataLoader = locationsDataLoader;
            PagesDataLoader = pagesDataLoader;
        }


        /// <summary>
        /// Executes the load method and performs thread secure action, as well as caching.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
        /// <param name="caller">The caller.</param>
        /// <param name="loadMethod">The load method.</param>
        /// <param name="errorLogAction">The string that shall receive the error message.</param>
        /// <param name="worker">A action which will be executed, with the loaded data as parameter, after the data has been loaded from the network. (It will not be invoked, when the data is loaded from a cached file)</param>
        /// <param name="persistWorker">A action which will be executed before persisting a list. This is different to the other worker, as this one will also contain cached files, when a merge is being executed.</param>
        /// <param name="finishedAction">A action which will be executed, after data has been successfully loaded.</param>
        /// <returns></returns>
        public static async Task<Collection<T>> ExecuteLoadMethod<T>(bool forceRefresh, IDataLoader caller,
            Func<Task<Collection<T>>> loadMethod, Action<string> errorLogAction,
            Action<Collection<T>> worker = null, Action<Collection<T>> persistWorker = null,
            Action finishedAction = null)
        {
            // lock the file
            await GetLock(caller.FileName);
            // check if a cached version exists
            var cachedFilePath = Constants.DatabaseFilePath + caller.FileName;
            if (File.Exists(cachedFilePath))
            {
                var autoRefresh = !forceRefresh; // this refresh was NOT caused by the user, but automatically
                var timePassed = caller.LastUpdated.AddHours(NoReloadTimeout) >= DateTime.Now; // 4 hours or more have passed since last update
                var notConnected = !CrossConnectivity.Current.IsConnected; // the device is not connected to the Internet
                var refreshDenied = Preferences.WifiOnly && !CrossConnectivity.Current.ConnectionTypes.Contains(ConnectionType.WiFi); // when the app shall only auto refresh to wifi and is not connected to wifi

                // use the cached data, if this is an auto refresh call and the last update is not older than 4 hours
                // OR this is an auto refresh and the refresh is denied through the current connection type and user settings
                // OR the device is simply not connected to the Internet
                if ((autoRefresh && timePassed) || (autoRefresh && refreshDenied) || notConnected)
                {
                    // load cached data
                    await ReleaseLock(caller.FileName);
                    return JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
                }
            }

            // try to load the data from network
            Collection<T> receivedList = null;
            // task that will load the data
            var task = Task.Run(() =>
            {
                try
                {
                    receivedList = loadMethod().Result;
                    worker?.Invoke(receivedList);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error when loading data: " + e);
                    receivedList = null;
                }
            });

            // start the work task and a task which will complete after a timeout simultaneously. If this task will finish first, we use the cached data instead.
            const int timeout = 50000; // 50 seconds timeout
            if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
            {
                // timeout logic
                Debug.WriteLine("Timeout loading data: " + caller.FileName);
                // if a cached version exists, use it instead
                if (File.Exists(cachedFilePath))
                {
                    // load cached data
                    await ReleaseLock(caller.FileName);
                    return JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
                }
                await ReleaseLock(caller.FileName);
                errorLogAction?.Invoke(AppResources.ErrorLoading);
                return new Collection<T>();
            }
            // loading task finished first, check if it failed (received list will be null)
            if (receivedList == null)
            {
                // if a cached version exists, use it instead
                if (File.Exists(cachedFilePath))
                {
                    // load cached data
                    await ReleaseLock(caller.FileName);
                    errorLogAction?.Invoke(AppResources.ErrorInternet);
                    return JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
                }

                // return empty list when it failed
                await ReleaseLock(caller.FileName);
                errorLogAction?.Invoke(AppResources.ErrorLoading);
                return new Collection<T>();
            }

            // cache the file as serialized JSON
            // and there is no id element given, overwrite it (we assume we get the entire list every time). OR there is no cached version present
            if (caller.Id == null || !File.Exists(cachedFilePath) || forceRefresh)
            {
                persistWorker?.Invoke(receivedList);
                WriteFile(cachedFilePath, JsonConvert.SerializeObject(receivedList), caller);
            }
            else
            {
                // otherwise we have to merge the loaded list, with the cached list
                var cachedList = JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
                cachedList.Merge(receivedList, caller.Id);

                persistWorker?.Invoke(cachedList);

                // overwrite the cached data
                WriteFile(cachedFilePath, JsonConvert.SerializeObject(cachedList), caller);

                // return the new merged list
                await ReleaseLock(caller.FileName);
                finishedAction?.Invoke();
                return cachedList;
            }

            // finally, after writing the file return the just loaded list
            await ReleaseLock(caller.FileName);
            finishedAction?.Invoke();
            return receivedList;
        }

        public static async Task<Collection<T>> GetCachedFiles<T>(IDataLoader caller)
        {
            // lock the file
            await GetLock(caller.FileName);
            // check if a cached version exists
            var cachedFilePath = Constants.DatabaseFilePath + caller.FileName;
            if (File.Exists(cachedFilePath))
            {

                // load cached data
                await ReleaseLock(caller.FileName);
                return JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
            }

            // finally, after writing the file return the just loaded list
            await ReleaseLock(caller.FileName);
            // if there is no file saved, return null
            return null;
        }

        public static async Task AddObject<T>(T data, IDataLoader caller)
        {
            //Lock the file
            await GetLock(caller.FileName);
            var cachedFilePath = Constants.DatabaseFilePath + caller.FileName;

            if (!File.Exists(cachedFilePath))
            {
                WriteFile(cachedFilePath, JsonConvert.SerializeObject(data), caller, true);
            }
            else
            {
                // otherwise we have to merge the loaded list, with the cached list
                var cachedList = JsonConvert.DeserializeObject<Collection<T>>(File.ReadAllText(cachedFilePath));
                var collection = new Collection<T> { data };
                cachedList.Merge(collection, caller.Id);

                // overwrite the cached data
                WriteFile(cachedFilePath, JsonConvert.SerializeObject(cachedList), caller);
            }

            await ReleaseLock(caller.FileName);
        }

        public static async Task PersistFiles<T>(Collection<T> data, IDataLoader caller)
        {
            // lock the file
            await GetLock(caller.FileName);
            // check if a cached version exists
            var cachedFilePath = Constants.DatabaseFilePath + caller.FileName;
            try
            {
                WriteFile(cachedFilePath, JsonConvert.SerializeObject(data), caller, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine("PersistFilesError: " + e);
                // ignored
            }
            finally
            {
                // finally, after writing the file return the just loaded list
                await ReleaseLock(caller.FileName);
            }
        }

        private static async Task ReleaseLock(string callerFileName)
        {
            while (!LoaderLocks.TryUpdate(callerFileName, false, true))
                await Task.Delay(200);
        }

        private static async Task GetLock(string callerFileName)
        {
            while (true)
            {
                // try to get the key, if it doesn't exist, add it. Try this until the value is false(is unlocked)
                while (LoaderLocks.GetOrAdd(callerFileName, false))
                {
                    // wait 500ms until the next try
                    await Task.Delay(500);
                }
                if (LoaderLocks.TryUpdate(callerFileName, true, false))
                {
                    // if the method returns true, this thread achieved to update the lock. Therefore we're done and leave the method
                    return;
                }
            }
        }

        private static void WriteFile(string path, string text, IDataLoader caller, bool dontSetUpdateTime = false)
        {
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, text);
            if (!dontSetUpdateTime)
                caller.LastUpdated = DateTime.Now;
        }
    }
}
