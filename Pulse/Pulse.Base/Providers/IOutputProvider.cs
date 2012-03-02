using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    /// <summary>
    /// Output providers process pictures as they are activated.
    /// This mostly occurs right now immediately after a picture has been downloaded.
    /// </summary>
    public interface IOutputProvider : IProvider
    {
        /// <summary>
        /// Processes the given picture and performs an action
        /// </summary>
        /// <param name="p">Picture to process</param>
        void ProcessPicture(Picture p);
    }
}
