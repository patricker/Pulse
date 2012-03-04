﻿using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Base;
using System.IO;

namespace LocalDirectory
{
    [System.ComponentModel.Description("Local Directory")]
    public class Provider : IInputProvider
    {
        public PictureList GetPictures(PictureSearch ps)
        {
            PictureList pl = new PictureList() { FetchDate = DateTime.Now };

            LocalDirectorySettings lds = string.IsNullOrEmpty(ps.ProviderSearchSettings) ? new LocalDirectorySettings() : LocalDirectorySettings.LoadFromXML(ps.ProviderSearchSettings);

            //get all files in directory and filter for image extensions (jpg/jpeg/png/bmp?)
            List<string> files = new List<string>();

            foreach (string ext in lds.Extensions.Split(new char[] { ';' }))
            {
                files.AddRange(Directory.GetFiles(lds.Directory, "*." + ext, SearchOption.AllDirectories));
            }

            //distinct list (just in case)
            files = files.Distinct().ToList();

            var maxPictureCount = ps.MaxPictureCount > 0 ? ps.MaxPictureCount : int.MaxValue;
            maxPictureCount = Math.Min(files.Count, maxPictureCount);


            //create picture items
            pl.Pictures.AddRange((from c in files
                                 select new Picture()
                                 {
                                     Id = Path.GetFileNameWithoutExtension(c),
                                     Url=c,
                                     LocalPath = c
                                 })
                                 .OrderBy(x=>Guid.NewGuid())
                                 .Take(maxPictureCount));

            return pl;
        }

        public void Activate(object args)
        {

        }

        public void Initialize()
        {

        }

        public void Deactivate(object args)
        {
            
        }
    }
}
