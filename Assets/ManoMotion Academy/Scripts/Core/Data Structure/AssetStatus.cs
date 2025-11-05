using System;

namespace ManoMotion
{
    [Serializable]
    public struct AssetStatus
    {
        public LicenseAnswer licenseAnswer;
        public float version;
    };

    public enum LicenseAnswer
    {
        LICENSE_OK = 30,
        LICENSE_INTERNET_REQUIRED = 41
    }
}