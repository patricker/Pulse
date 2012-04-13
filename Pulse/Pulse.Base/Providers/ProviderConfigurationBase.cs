using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pulse.Base
{
    public class ProviderConfigurationBase : UserControl
    {
        public bool IsOK = false;

        public ProviderConfigurationBase() : base()
        {

        }

        public virtual void LoadConfiguration(string config){}
        public virtual string SaveConfiguration() { return string.Empty; }
    }
}
