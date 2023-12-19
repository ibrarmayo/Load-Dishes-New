using System;
using System.IO;

namespace SupersonicWisdomSDK
{
    public static class SwConstants
    {
        #region --- Members ---
        
        public const string SORTABLE_DATE_STRING_FORMAT = "yyyy'-'MM'-'dd";
        public const string GameObjectName = "SupersonicWisdom";
        public const int DefaultRequestTimeout = 10;
        public const string Feature = "";
        public const long FeatureVersion = 0;
        public const string BuildNumber = "7906";
        public const string GitCommit = "a6886fb";

        public const string SdkVersion = "7.5.6";
        public const string SettingsResourcePath = "SupersonicWisdom/Settings";
        public const string ExtractedResourcesDirName = "Extracted";
        public const string CrashlyticsDependenciesFilePath = "Firebase/Editor/";
        public const string CrashlyticsDependenciesFileName = "CrashlyticsDependencies.xml";
        public const string FirebaseVersionTextFileName = "FirebaseUnityWrapperVersion";
        public const string IronsourceEditorFolder = "IronSource/Editor/";
        public const string IronsourceAdapterVersionsCacheFilename = "IronSourceAdapterVersions";

        private const string KNOWLEDGE_CENTER_ARTICLE_URL = "https://support.supersonic.com/hc/en-us/articles/";
        public const string REWARDED_VIDEO_HELP_URL = KNOWLEDGE_CENTER_ARTICLE_URL + "9945166026525-Wisdom-SDK-Integration-Guide-Stage-2";
        
        public static readonly long SdkVersionId = SwUtils.ComputeVersionId(SdkVersion);

        public const string APP_ICON_RESOURCE_NAME = "AppIcon";
        public static readonly string AppIconResourcesPath = Path.Combine("Extracted", APP_ICON_RESOURCE_NAME);
        
        #endregion
    }
}
