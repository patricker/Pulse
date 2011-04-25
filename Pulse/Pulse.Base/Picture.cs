using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public class Picture
    {
        public string Id { get; set; } //used as filename when downloading
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
