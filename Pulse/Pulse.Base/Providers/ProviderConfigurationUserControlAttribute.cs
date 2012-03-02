using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class ProviderConfigurationUserControlAttribute : Attribute
    {
        public Type UserControlType;

        public ProviderConfigurationUserControlAttribute(Type userControlType)
        {
            this.UserControlType = userControlType;
        }
    }
}
