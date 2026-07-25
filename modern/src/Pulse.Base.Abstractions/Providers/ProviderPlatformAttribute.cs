using System;

namespace Pulse.Base
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ProviderPlatformAttribute : Attribute
    {
        public PlatformID Platform { get; }
        public int MajorVersion { get; }
        public int MinorVersion { get; }

        public ProviderPlatformAttribute(PlatformID platform, int major, int minor)
        {
            Platform = platform;
            MajorVersion = major;
            MinorVersion = minor;
        }
    }
}
