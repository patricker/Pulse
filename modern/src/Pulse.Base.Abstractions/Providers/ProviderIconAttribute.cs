using System;

namespace Pulse.Base.Providers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ProviderIconAttribute : Attribute
    {
        // For cross-platform, icon loading is optional and not using System.Drawing
        public Type? ResourceType { get; }
        public string? ResourceName { get; }

        public ProviderIconAttribute(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }

        // Legacy compat - returns null in Abstractions, Windows version overrides
        public object? ProviderIcon => null;
    }
}
