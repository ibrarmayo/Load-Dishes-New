using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using SwJsonDictionary = System.Collections.Generic.Dictionary<string, object>;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwSelfUpdate
    {
        #region --- Constants ---

        private const string WISDOM_UPDATE_CONFIG_URL = "https://assets.mobilegamestats.com/docs/self-update-config-v1.json";
        
        internal const long TOTAL_SECONDS_IN_ONE_HOUR = 60 * 60;
        internal const long RECURRING_CHECK_UPDATES_DURATION_SECONDS = TOTAL_SECONDS_IN_ONE_HOUR;
        private const int NONE_CODE = -1;
        private const long TOTAL_SECONDS_IN_ONE_DAY = 24 * TOTAL_SECONDS_IN_ONE_HOUR;
        private const long LOGIN_TO_CHECK_UPDATES_ALERT_DURATION_SECONDS = TOTAL_SECONDS_IN_ONE_DAY;

        #endregion


        #region --- Members ---

        private static SwSelfUpdateConfiguration _selfUpdateConfiguration;

        #endregion


        #region --- Properties ---

        private static string WisdomBackUpFolderPath
        {
            get { return (Application.temporaryCachePath + "/sw-previous-version-files").Replace(" ", "-"); }
        }

        private static string PendingUnityPackagesToImportFolderPath
        {
            get { return (Application.temporaryCachePath + "/sw-pending-unity-packages-to-import").Replace(" ", "-"); }
        }

        private static string GameId
        {
            get
            {
                var iosGameId = SwEditorUtils.SwSettings.iosGameId;
                var androidGameId = SwEditorUtils.SwSettings.androidGameId;
                string gameId;

                
                if (!string.IsNullOrEmpty(iosGameId) && !string.IsNullOrEmpty(androidGameId))
                {
                    // Both not empty - choose according to the current target.
                    gameId = SwUtils.IsIosTarget() ? iosGameId : androidGameId;
                }
                else
                {
                    // At least one of them is empty - try to see the other available
                    gameId = !string.IsNullOrEmpty(iosGameId) ? iosGameId : androidGameId;
                }
                

                return gameId;
            }
        }

        private static bool IsDownloadProcessInProgress { get; set; }
        private static bool IsUpdateCheckInProgress { get; set; }

        #endregion


        #region --- Public Methods ---

        public static void OnPreCompilation()
        {
            EditorPrefs.SetString(SwEditorConstants.SwKeys.TEMP_AUTH_TOKEN, SwAccountUtils.AccountToken);

            if (IsDownloadProcessInProgress)
            {
                SwSelfUpdateWindow.CloseWindow();
                SwEditorAlerts.AlertError(SwEditorConstants.UI.WISDOM_UPDATE_FAILED_DUE_TO_COMPILATION, (long) SwErrors.ESelfUpdate.CompilationInterference, SwEditorConstants.UI.ButtonTitle.OK);
            }
        }

        /// <summary>
        /// This method initiates the self-update process, if needed.
        /// In case weh updated is needed, the flows divided into two cases:
        /// A. Stage update - an easy update, Wisdom will simply download the package and will import it.
        ///    This update should be easier because there's no version changes, Wisdom won't expect any folder structure changes, components removal, refactors, etc.
        /// B. Version update (or version + stage update) - a larger update, Wisdom will (a) restore its current version files (b) delete all these files (c) import.
        ///    In case (a) fails - it won't continue to (b). In case (b) / (c) fails - Wisdom will restore itself and revert to previous working version.
        /// </summary>
        /// <param name="isInitiatedByUser"></param>
        public static async void CheckForUpdates(bool isInitiatedByUser = false)
        {
            var shouldUpdate = await ShouldUpdate(isInitiatedByUser);

            if (shouldUpdate)
            {
                SwEditorLogger.Log("Wisdom should be updated");
                ShowUpdateConfirmation();
            }
            
            IsUpdateCheckInProgress = false;
        }

        public static void OnPostCompilation()
        {
            TryAlertWisdomUpdateFinished();
            CheckForUpdates();
        }

        #endregion


        #region --- Private Methods ---

        private static (int, int) ErroneousUpdateDetails { get { return (NONE_CODE, NONE_CODE); } }

        private static async Task<bool> ShouldUpdate(bool isInitiatedByUser)
        {
            var currentTimestampSeconds = DateTime.Now.SwTimestampSeconds();        
            if (ShouldQuitUpdateLocalChecksAndTryAlert(isInitiatedByUser, currentTimestampSeconds)) return false;
            if (await ShouldQuitUpdateRemoteChecksAndTryAlert(isInitiatedByUser, currentTimestampSeconds)) return false;

            return true;
        }

        /// <summary>
        ///     Checks if at this moment Wisdom detects traces of self-update, and checks if
        ///     the current stage and / or version matches the updated ones, that was persisted during update process.
        /// </summary>
        private static void TryAlertWisdomUpdateFinished()
        {
            var updateInfoJsonString = EditorPrefs.GetString(SwEditorConstants.SwKeys.IMPORTED_WISDOM_UPDATE_INFO, SwEditorConstants.EMPTY_STRINGIFIED_JSON);
            var updateInfoJsonDictionary = updateInfoJsonString.SwToJsonDictionary();
            int.TryParse(updateInfoJsonDictionary.SwSafelyGet(SwEditorConstants.SwKeys.UPDATED_SDK_STAGE_NUMBER, NONE_CODE.ToString()).ToString(), out var updatedStageNumber);
            int.TryParse(updateInfoJsonDictionary.SwSafelyGet(SwEditorConstants.SwKeys.UPDATED_SDK_VERSION_ID, NONE_CODE.ToString()).ToString(), out var updatedVersionId);

            var didCompleteUpdate = updatedStageNumber == SwStageUtils.CurrentStage.sdkStage || updatedVersionId == SwConstants.SdkVersionId;

            if (didCompleteUpdate)
            {
                SwEditorCoroutines.StartEditorCoroutine(OnPostUpdate());
            }
            
            HandlePackageContentFiles();
        }

        private static void PersistUpdateDetailsBeforeUpdate(int updatedStageNumber, long updatedVersionId)
        {
            var updateInfoJsonString = new SwJsonDictionary
            {
                [SwEditorConstants.SwKeys.UPDATED_SDK_STAGE_NUMBER] = updatedStageNumber,
                [SwEditorConstants.SwKeys.UPDATED_SDK_VERSION_ID] = updatedVersionId
            }.SwToJsonString();

            EditorPrefs.SetString(SwEditorConstants.SwKeys.IMPORTED_WISDOM_UPDATE_INFO, updateInfoJsonString);
        }

        private static void HandlePackageContentFiles()
        {
            var previousFolderPathHidden = SwEditorUtils.SwUpdateMetadataFolderPath(true);
            var updatedFolderPath = SwEditorUtils.SwUpdateMetadataExposedFolderPath.Replace($"./{SwEditorConstants.ASSETS}", SwEditorConstants.ASSETS);

            if (!Directory.Exists(updatedFolderPath))
            {
                SwEditorLogger.Log($"Couldn't find previous hidden folder - `{previousFolderPathHidden}` does not exists");
            }
            else
            {
                SwFileUtils.DeleteDirectory(previousFolderPathHidden);
                
                if (!SwFileUtils.TryMoveOrRenameFolder(updatedFolderPath, previousFolderPathHidden))
                {
                    SwEditorLogger.LogError($"Failed to replace previous hidden folder `{previousFolderPathHidden}` with `{updatedFolderPath}`");
                }

                // Specifically delete the new .meta file
                SwFileUtils.TryMoveOrRenameFile(updatedFolderPath + SwFileUtils.META_FILE_EXTENSION, previousFolderPathHidden + SwFileUtils.META_FILE_EXTENSION);

                var metaOfUpdatedFolder = new FileInfo(updatedFolderPath + SwFileUtils.META_FILE_EXTENSION);
                // Cleanup
                metaOfUpdatedFolder.SwTryDelete();
            }
        }

        private static string GetCurrentWisdomPackageContentFilePath()
        {
            var packageContentFolderPath = SwEditorUtils.SwUpdateMetadataFolderPath(true);
            var wisdomPackageContentFilePath = Path.Combine(packageContentFolderPath, SwEditorUtils.GetWisdomPackageContentFileName());

            if (!File.Exists(wisdomPackageContentFilePath))
            {
                SwEditorLogger.LogError("File not found! Expected to find package contents file in: " + packageContentFolderPath);
                wisdomPackageContentFilePath = string.Empty;
            }

            return wisdomPackageContentFilePath;
        }

        private static async Task<(string, int, string)> DownloadUpdatePackageWithCheckSum(string remoteUrl, string localDestinationFilePath)
        {
            string downloadedFilePath = null;
            var downloadUpdatePackageErrorCode = 0;
            string checksum = null;

            try
            {
                // Promise style...
                await Task.WhenAll(new List<Task>
                {
                    Task.Run(async () =>
                    {
                        var (downloadedFile, errorCode) = await DownloadUpdatePackage(remoteUrl, localDestinationFilePath);
                        downloadedFilePath = downloadedFile;
                        downloadUpdatePackageErrorCode = errorCode;
                    }),
                    Task.Run(async () =>
                    {
                        var wisdomPackageApi = remoteUrl.Replace(SwPlatformCommunication.URLs.DOWNLOAD_WISDOM_PACKAGE, SwPlatformCommunication.URLs.WISDOM_PACKAGE_MANIFEST);
                        var (response, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(wisdomPackageApi, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary());
                        var wisdomPackageManifest = response.SwToJsonDictionary();
                        checksum = wisdomPackageManifest.SwSafelyGet(SwEditorUtils.Keys.CHECKSUM, string.Empty).ToString();
                    }),
                });
            }
            catch
            {
                //
            }

            return (downloadedFilePath, downloadUpdatePackageErrorCode, checksum);
        }

        private static async Task<(string, int)> DownloadUpdatePackage(string fromRemoteUrl, string toLocalFilePath)
        {
            var (_, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(fromRemoteUrl, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary());

            var fileTempUrl = string.Empty;

            if (error.IsValid && 302 == error.ResponseCode)
            {
                // Got a redirect response: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.httpresponseheaders.location?view=net-6.0#system-net-http-headers-httpresponseheaders-location
                fileTempUrl = httpResponseMessage.Headers.Location.ToString();
            }

            if (string.IsNullOrEmpty(fileTempUrl)) return (null, error.ResponseCode);
            
            IsDownloadProcessInProgress = true;

            var (downloadedFilePath, errorCode) = await SwNetworkHelper.DownloadFileAsync(new Uri(fileTempUrl), toLocalFilePath, (percentagesDownloaded, totalBytesDownloaded, onCancel) =>
            {
                if (IsDownloadProcessInProgress)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(SwEditorConstants.UI.SUPERSONIC_WISDOM_SDK, SwEditorConstants.UI.DownloadingUpdatePackage(SwUtils.GenerateFileSizeString(totalBytesDownloaded), (percentagesDownloaded * 100).ToString("0")), percentagesDownloaded))
                    {
                        IsDownloadProcessInProgress = false;

                        SwEditorUtils.RunOnMainThread(() =>
                        {
                            onCancel?.Invoke();
                            SwSelfUpdateWindow.CloseWindow();
                            EditorUtility.ClearProgressBar();
                        });
                    }
                }
            });

            IsDownloadProcessInProgress = false;

            SwEditorUtils.RunOnMainThread(EditorUtility.ClearProgressBar, 100);

            return (downloadedFilePath, errorCode);
        }

        private static async Task<string> UpdateWisdomFromPackage(string unityPackageFilePath, int updatedStageNumber, long updatedVersionId)
        {
            var steps = new[] {UiMessages.SW_UPDATE_BACKUP_STEP, UiMessages.SW_UPDATE_DELETION_STEP, UiMessages.SW_UPDATE_IMPORT_STEP}.ToList();
            float totalSteps = steps.Count;
            float currentStep = 0;

            var updateSteps = new Action(() =>
            {
                SwEditorUtils.RunOnMainThread(() =>
                {
                    currentStep += 1;
                    // a floating-point value between 0.0 and 1.0, indicating the progress.
                    var progressStep = currentStep / totalSteps;
                    var progressDescription = steps.FirstOrDefault();
                    steps.RemoveAt(0);
                    EditorUtility.DisplayProgressBar(SwEditorConstants.UI.SUPERSONIC_WISDOM_SDK, progressDescription, progressStep);

                    // For giving the user the time to read each message
                    Thread.Sleep(1000);
                });
            });

            string errorMessage = null;
            var didImport = false;
            string backUpFolderPath = null;

            try
            {
                if (_selfUpdateConfiguration.ShouldUpdateVersion)
                {
                    updateSteps();
                    backUpFolderPath = BackupCurrentVersionFiles();

                    if (string.IsNullOrEmpty(backUpFolderPath))
                    {
                        const string message = SwEditorConstants.ErrorMessages.FAILED_TO_BACKUP;
                        SwEditorLogger.LogError(message);
                        
                        return message;
                    }

                    updateSteps();
                    
                    if (!DeleteCurrentVersionFiles())
                    {
                        const string message = SwEditorConstants.ErrorMessages.FAILED_TO_DELETE_CURRENT_VERSION;
                        
                        SwEditorLogger.LogError(message);
                        
                        return message;
                    }

                    updateSteps();
                    EditorUtility.ClearProgressBar();
                }
                else if (_selfUpdateConfiguration.ShouldUpdateStage)
                {
                    SwEditorLogger.Log("No need to update version, updating only stage.");
                }

                PersistUpdateDetailsBeforeUpdate(updatedStageNumber, updatedVersionId);
                SwEditorLogger.Log($"Importing Unity package file from path: {unityPackageFilePath}");
                didImport = await SwEditorUtils.ImportPackage(unityPackageFilePath, true);
                
                // NOTE: The code below will run only in Unity Editors below 2022. Because in Unity 2022, right after import and after a successful compilation, the process gets killed and replaced.
                
                if (didImport)
                {
                    if (_selfUpdateConfiguration.ShouldVerifyUpdate)
                    {
                        // Verify version update by checking existing files vs. the expected file from package content.
                        if (!VerifySuccessfulUpdate())
                        {
                            errorMessage = SwEditorConstants.ErrorMessages.FAILED_TO_VERIFY_SUCCESSFUL_UPDATE;
                            didImport = false;
                            SwInfra.Logger.LogError(EWisdomLogType.SelfUpdate, errorMessage + ". Package path: " + unityPackageFilePath);
                        }
                    }
                    else
                    {
                        SwEditorLogger.Log("Skipping update verification.");
                    }
                    
                    HandlePackageContentFiles();
                }
                else
                {
                    errorMessage = SwEditorConstants.ErrorMessages.FAILED_TO_IMPORT_PACKAGE;
                    SwInfra.Logger.LogError(EWisdomLogType.SelfUpdate, errorMessage + "package path: " + unityPackageFilePath);
                }
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError("Importing SW update package failed! Exception:\n" + e);
            }
            finally
            {
                if (didImport)
                {
                    SwFileUtils.DeleteDirectory(WisdomBackUpFolderPath);
                }
                else if (_selfUpdateConfiguration.ShouldUpdateVersion)
                {
                    if (!string.IsNullOrEmpty(backUpFolderPath))
                    {
                        SwEditorLogger.Log("Restoring due to Wisdom update failure... ");
                        if (RestorePreviousVersionFiles(backUpFolderPath))
                        {
                            SwEditorLogger.Log("... restored.");   
                        }
                        else
                        {
                            SwEditorLogger.LogError("... failed restore!");
                        }
                    }
                }

                SwFileUtils.DeleteFileAtPath(unityPackageFilePath);

                EditorUtility.ClearProgressBar();
            }

            return errorMessage;
        }

        private static string CurrentStageUrl()
        {
            if (string.IsNullOrEmpty(GameId)) return string.Empty;

            var queryString = SwUtils.SerializeToQueryString(new SwJsonDictionary
            {
                [QueryParamKeys.ID] = GameId
            });

            return $"{SwPlatformCommunication.URLs.CURRENT_STAGE_API}?{queryString}";
        }

        private static async Task<(int, long)> FetchVersionDetailsForTitle(bool isInitiatedByUser)
        {
            var currentStageUrl = CurrentStageUrl();

            if (string.IsNullOrEmpty(currentStageUrl))
            {
                return ErroneousUpdateDetails;
            }

            var (responseString, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(currentStageUrl, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary());

            if (error.IsValid)
            {
                if (isInitiatedByUser)
                {
                    SwEditorLogger.LogError($"Error: {error},\nHTTP response: {httpResponseMessage}");
                    SwEditorAlerts.AlertFailedToCheckCurrentStage(error.ErrorMessage);
                }

                return ErroneousUpdateDetails;
            }

            var responseDictionary = SwJsonParser.DeserializeToDictionary(responseString);
            var errorCode = (int) (responseDictionary.SwSafelyGet("errorCode", null) ?? NONE_CODE);

            if (errorCode >= 0)
            {
                var errorMessage = responseDictionary.SwSafelyGet("errorMessage", string.Empty).ToString();

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    SwEditorLogger.LogError(errorMessage);
                }

                return ErroneousUpdateDetails;
            }

            var stageNumberString = responseDictionary.SwSafelyGet("stage", NONE_CODE.ToString()).ToString();
            var latestVersionString = responseDictionary.SwSafelyGet("latest-version", NONE_CODE.ToString()).ToString();
            var latestStableVersionId = SwUtils.ComputeVersionId(latestVersionString);

            if (latestStableVersionId == 0)
            {
                long.TryParse(latestVersionString, NumberStyles.Any, CultureInfo.InvariantCulture, out latestStableVersionId);
            }
                
            if (!int.TryParse(stageNumberString, NumberStyles.Any, CultureInfo.InvariantCulture, out var stageNumber))
            {
                stageNumber = NONE_CODE;
                SwEditorLogger.LogError("Failed to parse an integer from stage number string: " + stageNumberString);
            }

            return (stageNumber, latestStableVersionId);
        }

        private static async Task<SwSelfUpdateRawConfiguration> FetchUpdateConfig()
        {
            SwSelfUpdateRawConfiguration selfUpdateConfiguration = null;

            try
            {
                var (jsonString, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(WISDOM_UPDATE_CONFIG_URL, null, null);

                if (string.IsNullOrEmpty(jsonString) && error.IsValid)
                {
                    SwEditorLogger.LogError(error.ErrorMessage);
                }
                else
                {
                    selfUpdateConfiguration = JsonConvert.DeserializeObject<SwSelfUpdateRawConfiguration>(jsonString ?? SwEditorConstants.EMPTY_STRINGIFIED_JSON);
                }
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            return selfUpdateConfiguration;
        }

        private static string GenerateLocalFilePath(int stageNumber, string versionToDownload)
        {
            if (!Directory.Exists(PendingUnityPackagesToImportFolderPath))
            {
                Directory.CreateDirectory(PendingUnityPackagesToImportFolderPath);
            }

            return PendingUnityPackagesToImportFolderPath + "/SupersonicWisdomSDK_" + versionToDownload + "_stage_" + stageNumber + SwFileUtils.UNITY_PACKAGE_FILE_EXTENSION;
        }

        private static string GenerateUnityPackageRemoteUrl(int stageToDownload, long requestedVersionId)
        {
            var apiPath = string.Empty;

            if (requestedVersionId <= 0)
            {
                SwEditorLogger.LogError("The param 'requestedVersionId' doesn't exist, using current running version as fallback.");
                requestedVersionId = SwConstants.SdkVersionId;
            }

            try
            {
                var versionToDownload = SwUtils.ComputeVersionString(requestedVersionId);
                var fileSuffix = string.Empty;

                if (SwStageUtils.MAX_STAGE_NUMBER != stageToDownload)
                {
                    fileSuffix = "_Stage" + stageToDownload;
                }

                var path = versionToDownload + "/SupersonicWisdomSDK_" + versionToDownload + fileSuffix + SwFileUtils.UNITY_PACKAGE_FILE_EXTENSION;
                apiPath = SwPlatformCommunication.URLs.DOWNLOAD_WISDOM_PACKAGE + $"?{QueryParamKeys.ID}={GameId}&path={path}&{QueryParamKeys.SDK_VERSION_ID}={requestedVersionId}";
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            return apiPath;
        }

        private static IEnumerator OnPostUpdate()
        {
            yield return new WaitForSeconds(2);

            try
            {
                SwFileUtils.DeleteDirectory(PendingUnityPackagesToImportFolderPath);
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            EditorPrefs.DeleteKey(SwEditorConstants.SwKeys.IMPORTED_WISDOM_UPDATE_INFO);

            if (SwEditorAlerts.AlertUpdateSuccess())
            {
                SwEditorUtils.OpenSettings();
            }
        }

        private static async Task<bool> ShouldQuitUpdateRemoteChecksAndTryAlert(bool isInitiatedByUser, long currentTimestampSeconds)
        {
            var lastCheckupTimestampString = EditorPrefs.GetString(SwEditorConstants.SwKeys.LAST_WISDOM_UPDATE_CHECKUP_TIMESTAMP, "0");
            long.TryParse(lastCheckupTimestampString, out var lastCheckupTimestamp);
            if (!isInitiatedByUser && currentTimestampSeconds - lastCheckupTimestamp < RECURRING_CHECK_UPDATES_DURATION_SECONDS) return true;
            
            EditorPrefs.SetString(SwEditorConstants.SwKeys.LAST_WISDOM_UPDATE_CHECKUP_TIMESTAMP, currentTimestampSeconds.ToString());

            SwEditorLogger.Log("Checking for updates...");
            var (updatedStage, updatedVersionId) = await FetchVersionDetailsForTitle(isInitiatedByUser);
            var rawUpdateConfiguration = await FetchUpdateConfig();
            _selfUpdateConfiguration = new SwSelfUpdateConfiguration(rawUpdateConfiguration, updatedStage, updatedVersionId);
            var shouldUpdate = _selfUpdateConfiguration.ShouldUpdate;

            if (!shouldUpdate)
            {
                SwEditorLogger.Log("Can't / no need to update.");

                if (isInitiatedByUser)
                {
                    if (NONE_CODE == updatedStage || NONE_CODE == updatedVersionId)
                    {
                        SwEditorAlerts.AlertUpdateCheckFailed();
                    }
                    else
                    {
                        SwEditorAlerts.AlertNoUpdateAvailable();
                    }
                }
            }

            return !shouldUpdate;
        }
        
        private static bool ShouldQuitUpdateLocalChecksAndTryAlert(bool isInitiatedByUser, long currentTimestampSeconds)
        {
            if (IsUpdateCheckInProgress || SwSelfUpdateWindow.GetIfPresented() != null)
            {
                if (isInitiatedByUser)
                {
                    SwEditorUtils.RunOnMainThread(() =>
                    {
                        if (SwEditorAlerts.AlertUpdateInProgress())
                        {
                            SwSelfUpdateWindow.GetIfPresented()?.Focus();
                        }
                    });
                }

                return true;
            }
            
            IsUpdateCheckInProgress = true;

            if (SwEditorUtils.SwSettings == null)
            {
                return true;
            }

            if (!SwAccountUtils.IsLoggedIn)
            {
                var shouldShowPopup = isInitiatedByUser;

                if (!shouldShowPopup)
                {
                    var lastLoginAlertTimestampString = EditorPrefs.GetString(SwEditorConstants.SwKeys.LAST_LOGIN_ALERT_TIMESTAMP, "0");

                    long.TryParse(lastLoginAlertTimestampString, out var lastLoginAlertTimestampSeconds);
                    shouldShowPopup = currentTimestampSeconds - lastLoginAlertTimestampSeconds > LOGIN_TO_CHECK_UPDATES_ALERT_DURATION_SECONDS;
                    
                    if (shouldShowPopup)
                    {
                        EditorPrefs.SetString(SwEditorConstants.SwKeys.LAST_LOGIN_ALERT_TIMESTAMP, currentTimestampSeconds.ToString());
                    }
                }

                if (shouldShowPopup)
                {
                    if (SwEditorAlerts.Alert(SwEditorConstants.UI.CANT_CHECK_UPDATES, SwEditorConstants.UI.ButtonTitle.LOGIN_NOW, SwEditorConstants.UI.ButtonTitle.CANCEL))
                    {
                        SwAccountUtils.GoToLoginTab();
                    }
                }

                return true;
            }

            if ((SwAccountUtils.TitlesList?.Count ?? 0) == 0)
            {
                if (isInitiatedByUser)
                {
                    if (SwEditorAlerts.AlertUpdateCheckFailedDueToEmptyGamesList())
                    {
                        SwAccountUtils.GoToLoginTab();
                    }
                }

                return true;
            }
            
            return false;
        }

        private static async void OnUserClickedUpdate(SwSelfUpdateWindow updateWindow)
        {
            var updatedStage = _selfUpdateConfiguration.UpdatedStage;
            var updatedVersionId = _selfUpdateConfiguration.UpdatedVersionId;
            updateWindow.IsUpdateButtonEnabled = false;
            var didUpdate = await BeginSelfUpdate(updatedStage, updatedVersionId);
            
            if (didUpdate)
            {
                // This block should never run as it will be interrupted at some point.
                // The replaced and compiled code (from the imported package) will take it from there.
            }

            updateWindow.IsUpdateButtonEnabled = !didUpdate;
        }

        private static void ShowUpdateConfirmation()
        {
            var currentVersionId = SwSelfUpdateConfiguration.CurrentVersionId;
            var currentStage = SwSelfUpdateConfiguration.CurrentStage;
            var updatedVersionId = _selfUpdateConfiguration.UpdatedVersionId;
            var updatedStage = _selfUpdateConfiguration.UpdatedStage;

            var shouldUpdateStage = _selfUpdateConfiguration.ShouldUpdateStage;
            var shouldUpdateVersion = _selfUpdateConfiguration.ShouldUpdateVersion;

            const string integrationGuideUrlKey = "integrationGuideUrl";
            const string changeLogUrlKey = "changeLogsUrl";

            var stageKey = $"{currentStage}-{updatedStage}";
            var stageUpdateConfiguration = _selfUpdateConfiguration.StageUpdate;
            stageUpdateConfiguration = new SwJsonDictionary(SwJsonParser.Deserialize(stageUpdateConfiguration.SwSafelyGet(stageKey, null)?.ToString()) as Dictionary<string, object>);
            var bothConfiguration = _selfUpdateConfiguration.StageAndVersionUpdate;
            var versionUpdateConfiguration = _selfUpdateConfiguration.VersionUpdate;

            var messageTitle = (string) stageUpdateConfiguration.SwSafelyGet("messageTitle", null);
            var updateButtonTitle = (string) stageUpdateConfiguration.SwSafelyGet("updateButtonTitle", null);
            var updateButtonTip = (string) stageUpdateConfiguration.SwSafelyGet("updateButtonTip", null);
            var changeLogUrl = (string) versionUpdateConfiguration.SwSafelyGet(changeLogUrlKey, SwEditorConstants.DEFAULT_CHANGE_LOG_URL);
            var releaseNotesLinkTitle = (string) versionUpdateConfiguration.SwSafelyGet("releaseNotesLinkTitle", "Release notes");

            // In addition, we noticed there is a new and updated SDK for you
            var integrationGuideButtonTitle = (string) stageUpdateConfiguration.SwSafelyGet("integrationGuideButtonTitle", "Integration Guide");
            var integrationGuideButtonTip = (string) stageUpdateConfiguration.SwSafelyGet("integrationGuideButtonTip", "Link to Wisdom integration guide");
            var integrationGuideUrl = (string) stageUpdateConfiguration.SwSafelyGet(integrationGuideUrlKey, SwEditorConstants.DEFAULT_STAGE_INTEGRATION_GUIDE_URL);
            SwEditorUtils.SwAccountData.IntegrationGuideUrl = integrationGuideUrl;

            var additionalDescription = (string) bothConfiguration.SwSafelyGet("additionalDescription", string.Empty);
            string defaultMessageBody;
            SwJsonDictionary messageBodyConfig;

            if (shouldUpdateStage && shouldUpdateVersion) // Both
            {
                defaultMessageBody = $"Good News!\nYour game, {SwSelfUpdateWindow.APP_NAME_PLACEHOLDER}, has advanced to the next level.\nIn addition, we noticed there is a new and updated SDK for you.\n\nUpgrade your Wisdom package from version {SwSelfUpdateWindow.CURRENT_VERSION_PLACEHOLDER} to {SwSelfUpdateWindow.UPDATED_VERSION_PLACEHOLDER}.{SwSelfUpdateWindow.CHANGE_LOGS_LINK_PLACEHOLDER}\nAdd functionalities needed to keep progression using the integration guide.";
                messageBodyConfig = bothConfiguration;
            }
            else if (shouldUpdateVersion) // `shouldUpdateVersion` only
            {
                integrationGuideUrl = string.Empty;
                defaultMessageBody = $"We noticed there is a new and updated SDK for you.\nUpgrade your Wisdom package from version {SwSelfUpdateWindow.CURRENT_VERSION_PLACEHOLDER} to {SwSelfUpdateWindow.UPDATED_VERSION_PLACEHOLDER}.{SwSelfUpdateWindow.CHANGE_LOGS_LINK_PLACEHOLDER}";
                messageBodyConfig = versionUpdateConfiguration;
            }
            else // `shouldUpdateStage` only
            {
                changeLogUrl = string.Empty; // clear "change logs" link
                defaultMessageBody = $"Good News!\nYour game, {SwSelfUpdateWindow.APP_NAME_PLACEHOLDER}, has advanced to the next level.\nUpgrade your Wisdom package to add functionalities needed to keep progressing.";
                messageBodyConfig = stageUpdateConfiguration;
            }

            var messageBody = messageBodyConfig.SwSafelyGet("messageBody", defaultMessageBody) as string ?? string.Empty;

            var remindLaterButtonTitle = (string) versionUpdateConfiguration.SwSafelyGet("remindLaterButtonTitle", "Remind me later");
            var remindLaterButtonTip = (string) versionUpdateConfiguration.SwSafelyGet("remindLaterButtonTip", "We will let you know again after " + SwSelfUpdateWindow.CHECK_INTERVAL_HOURS_PLACEHOLDER);

            var currentVersion = SwUtils.ComputeVersionString(currentVersionId);
            var updatedVersion = SwUtils.ComputeVersionString(updatedVersionId);

            SwSelfUpdateWindow.ShowNew(updateButtonTitle, updateButtonTip, messageTitle, messageBody, integrationGuideButtonTitle, integrationGuideButtonTip, additionalDescription, integrationGuideUrl, changeLogUrl, releaseNotesLinkTitle, remindLaterButtonTitle, remindLaterButtonTip, currentVersion, updatedVersion, (selfUpdateWindow, didUserAgree) =>
            {
                if (didUserAgree)
                {
                    OnUserClickedUpdate(selfUpdateWindow);
                }
            });
        }

        private static async Task<bool> BeginSelfUpdate(int updatedStageNumber, long requestedVersionId)
        {
            var remoteUrl = GenerateUnityPackageRemoteUrl(updatedStageNumber, requestedVersionId);

            if ((remoteUrl?.Length ?? 0) == 0)
            {
                SwEditorAlerts.AlertError(SwEditorConstants.UI.FAILED_TO_GENERATE_REMOTE_URL, (int) SwErrors.ESelfUpdate.InvalidUnityPackageRemoteUrl, SwEditorConstants.UI.ButtonTitle.OK);

                return false;
            }

            var localDestinationFilePath = GenerateLocalFilePath(updatedStageNumber, SwUtils.ComputeVersionString(requestedVersionId));
            var (downloadedFilePath, downloadUpdatePackageErrorCode, checksum) = await DownloadUpdatePackageWithCheckSum(remoteUrl, localDestinationFilePath);

            var didUpdate = false;

            if ((downloadedFilePath?.Length ?? 0) > 0)
            {
                var md5 = SwEditorUtils.Md5(downloadedFilePath);

                if (md5 == checksum)
                {
                    Selection.activeObject = null; // Deselecting our settings object in the inspector before import. Motivation: The settings inspector is not refreshed properly in case of stage update while our settings is focused in the inspector.
                    var errorMessage = await UpdateWisdomFromPackage(localDestinationFilePath, updatedStageNumber, requestedVersionId);

                    didUpdate = errorMessage == null;
                    
                    if (!didUpdate)
                    {
                        if (SwEditorAlerts.AlertFailedToUpdateWisdomPackage(errorMessage))
                        {
                            SwSelfUpdateWindow.GetIfPresented()?.Focus();
                            InternalEditorUtility.RepaintAllViews();
                        }
                    }
                }
                else
                {
                    SwEditorLogger.LogError($"Failed to download: Checksum doesn't match (expected: {checksum}, actual: {md5})");
                    SwEditorAlerts.AlertFailedToDownloadPackage((int) SwErrors.ESelfUpdate.DownloadChecksumMismatch);
                }
            }
            else
            {
                if (downloadUpdatePackageErrorCode != 0 && downloadUpdatePackageErrorCode != (int) SwErrors.ESelfUpdate.DownloadUpdatePackageCanceled)
                {
                    SwEditorLogger.LogError($"Failed to download stage update Unity package file to path: {localDestinationFilePath}. Error code: {downloadUpdatePackageErrorCode}");

                    SwEditorAlerts.AlertFailedToDownloadPackage(downloadUpdatePackageErrorCode);
                }
            }

            return didUpdate;
        }

        private static bool VerifySuccessfulUpdate()
        {
            SwEditorLogger.Log($"{nameof(SwSelfUpdate)} | {nameof(VerifySuccessfulUpdate)} | Verifying...");
            
            var expectedFiles = GetCurrentPackageContentFileNames();
            var allProjectFiles = SwFileUtils.GetFolderContent(Application.dataPath, true).ToList().ConvertAll(e => e.Replace(Application.dataPath, SwEditorConstants.ASSETS)).SwToHashSet();

            // Cleanup all
            expectedFiles.RemoveWhere(e => Path.GetFileName(e).StartsWith(SwFileUtils.HIDDEN_FILE_PREFIX));
            allProjectFiles.RemoveWhere(e => Path.GetFileName(e).StartsWith(SwFileUtils.HIDDEN_FILE_PREFIX));

            // Sometimes the list may contain "Unity hidden files and folder names"
            expectedFiles.RemoveWhere(e => e.EndsWith(SwFileUtils.UNITY_IGNORED_FILE_SUFFIX));
            allProjectFiles.RemoveWhere(e => e.EndsWith(SwFileUtils.UNITY_IGNORED_FILE_SUFFIX));

            // Sometimes there are expected meta files that still were not created at this pont
            expectedFiles.RemoveWhere(e => e.EndsWith(SwFileUtils.META_FILE_EXTENSION));
            allProjectFiles.RemoveWhere(e => e.EndsWith(SwFileUtils.META_FILE_EXTENSION));

            // Turns out this could also happen
            expectedFiles.RemoveWhere(string.IsNullOrEmpty);
            allProjectFiles.RemoveWhere(string.IsNullOrEmpty);

            expectedFiles.RemoveWhere(e => e.Contains(SwEditorUtils.SW_UPDATE_METADATA_FOLDER_NAME));
            
            SwInfra.Logger.Log(EWisdomLogType.SelfUpdate, $"expectedFiles: {expectedFiles.SwToString()}");
            SwInfra.Logger.Log(EWisdomLogType.SelfUpdate, $"allProjectFiles: {allProjectFiles.SwToString()}");
            expectedFiles.RemoveWhere(e => allProjectFiles.Contains(e));
            
            bool isValid = 0 == expectedFiles.Count;

            if (!isValid)
            {
                SwEditorLogger.LogError($"SW self-update verification failed, {expectedFiles.Count} expected files not found: {expectedFiles.SwToString(",\n")}");
            }
            
            SwEditorLogger.Log($"{nameof(SwSelfUpdate)} | {nameof(VerifySuccessfulUpdate)} | Is Wisdom package valid? => {isValid}");

            return isValid;
        }

        private static bool RestorePreviousVersionFiles(string backUpFolderPath)
        {
            if (string.IsNullOrEmpty(backUpFolderPath)) return false;
            if (!Directory.Exists(backUpFolderPath)) return false;

            SwEditorLogger.Log("Deleting current version files...");
            if (DeleteCurrentVersionFiles())
            {
                SwEditorLogger.Log("... deleted.");
            }
            else
            {
                SwEditorLogger.LogError("... failed to delete all current files!");
            }

            var allBackUpFiles = SwFileUtils.GetFolderContent(backUpFolderPath, true);
            var didRestoreAll = true;

            foreach (var backUpFile in allBackUpFiles)
            {
                var relativePath = backUpFile.Replace(backUpFolderPath, string.Empty);

                var destinationPath = string.Empty;
                var assetsFolder = $"/{SwEditorConstants.ASSETS}/";

                if (relativePath.StartsWith(assetsFolder))
                {
                    relativePath = relativePath.Remove(0, assetsFolder.Length);
                    destinationPath = Path.Combine(Application.dataPath, relativePath);
                }
                else
                {
                    destinationPath = Path.Combine(Application.dataPath, relativePath);
                }

                var didCreate = SwFileUtils.CreateDirectory(Path.GetDirectoryName(destinationPath));

                if (!didCreate)
                {
                    didRestoreAll = false;

                    continue;
                }

                if (backUpFile.EndsWith(SwFileUtils.UNITY_IGNORED_FILE_SUFFIX) || destinationPath.EndsWith(SwFileUtils.UNITY_IGNORED_FILE_SUFFIX)) continue;

                didRestoreAll &= SwFileUtils.CopyFile(backUpFile, destinationPath, true, true);
            }

            return didRestoreAll;
        }

        private static string BackupCurrentVersionFiles()
        {
            var packageContentFileNames = GetCurrentPackageContentFileNames();
            var backUpFolderPath = WisdomBackUpFolderPath + "/" + SwConstants.SdkVersion;

            if (!SwFileUtils.DeleteDirectory(backUpFolderPath))
            {
                SwEditorLogger.LogError("Failed to delete previous backup folder: " + backUpFolderPath);
                return string.Empty;
            }

            var didCreate = SwFileUtils.CreateDirectory(backUpFolderPath);

            if (!didCreate)
            {
                SwEditorLogger.LogError("Failed to create backup folder: " + backUpFolderPath);
                return string.Empty;
            }

            SwEditorLogger.Log($"Backing up current package contents to: {backUpFolderPath}");
            var allCopied = true;

            // Copy all current package content
            foreach (var filePath in packageContentFileNames)
            {
                if (string.IsNullOrEmpty(filePath)) continue;
                if (filePath.StartsWith(SwFileUtils.HIDDEN_FILE_PREFIX) || filePath.EndsWith(SwFileUtils.UNITY_IGNORED_FILE_SUFFIX)) continue;

                var fileInfo = new FileInfo(filePath);

                if (!fileInfo.Exists) continue;

                var destinationPath = Path.Combine(backUpFolderPath, filePath.Replace(Application.dataPath, string.Empty));

                try
                {
                    var destinationFolder = Path.GetDirectoryName(destinationPath);
                    didCreate = SwFileUtils.CreateDirectory(destinationFolder);
                }
                catch (Exception e)
                {
                    didCreate = false;
                    SwEditorLogger.LogError(e);
                }

                if (!didCreate)
                {
                    SwEditorLogger.LogError("Failed to create backup destination folder: " + destinationPath);
                    return string.Empty;
                }

                // Clean all previous files list
                if (!SwFileUtils.CopyFile(filePath, destinationPath, true))
                {
                    allCopied = false;

                    break;
                }
            }

            return allCopied ? backUpFolderPath : string.Empty;
        }

        private static HashSet<string> GetCurrentPackageContentFileNames()
        {
            var wisdomFilesContentFilePath = GetCurrentWisdomPackageContentFilePath();

            if (string.IsNullOrEmpty(wisdomFilesContentFilePath)) return new HashSet<string>();

            SwInfra.Logger.Log(EWisdomLogType.SelfUpdate, $"Found package content file: {wisdomFilesContentFilePath.SwToString()}");
            var currentFilesList = File.ReadAllText(wisdomFilesContentFilePath).SwSplit("\n").SwToHashSet();
            currentFilesList.Add(wisdomFilesContentFilePath.Replace($".{SwFileUtils.DirectorySeparatorChar}Assets", "Assets"));

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Fix path separator in Windows Editor
                currentFilesList = currentFilesList.ToList().ConvertAll(item => item.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)).SwToHashSet();
            }
            
            return currentFilesList;
        }

        private static bool DeleteCurrentVersionFiles()
        {
            // Delete all files from "previous files list".
            return SwFileUtils.DeleteFilesAtPaths(GetCurrentPackageContentFileNames().ToArray());
        }

        #endregion


        #region --- Inner Classes ---

        // Note: These keys are used for two different APIs: 'current-stage', 'download-wisdom-package'
        private static class QueryParamKeys
        {
            #region --- Constants ---

            internal const string ID = "id";
            internal const string SDK_VERSION_ID = "sdkVersionId";

            #endregion
        }

        private static class UiMessages
        {
            #region --- Constants ---

            public const string SW_UPDATE_BACKUP_STEP = "backing up files";
            public const string SW_UPDATE_DELETION_STEP = "deleting current version files";
            public const string SW_UPDATE_IMPORT_STEP = "importing...";

            #endregion
        }

        #endregion
    }
}