using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pulse.Base
{
    /// <summary>
    /// Interaction logic for ProviderConfigurationBase.xaml
    /// </summary>
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
