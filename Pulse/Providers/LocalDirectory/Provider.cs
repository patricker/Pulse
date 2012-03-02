using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;

namespace LocalDirectory
{
    [System.ComponentModel.Description("Local Directory")]
    public class Provider : IInputProvider
    {
        public void Activate(object args)
        {
            
        }

        public void Initialize()
        {
            
        }

        public PictureList GetPictures(PictureSearch ps)
        {
            throw new NotImplementedException();
        }

        public void Deactivate(object args)
        {
            
        }
    }
}
