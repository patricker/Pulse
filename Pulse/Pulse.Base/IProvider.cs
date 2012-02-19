using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Base
{
    public interface IProvider
    {
        void Initialize();
        PictureList GetPictures(PictureSearch ps);
    }
}
