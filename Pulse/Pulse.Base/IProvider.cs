using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public interface IProvider
    {
        void Initialize();
        List<Picture> GetPictures(string search, bool skipLowRes, bool getMaxRes, List<string> filterKeywords);
        int GetResultsCount();
    }
}
