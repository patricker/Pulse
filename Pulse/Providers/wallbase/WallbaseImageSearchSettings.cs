using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;

namespace wallbase
{
    public class WallbaseImageSearchSettings : Pulse.Base.XmlSerializable<WallbaseImageSearchSettings>
    {
        //search location
        public string SA { get; set; }

        //color, used for color searches (color searching is only available with "Search" type
        public System.Drawing.Color Color { get; set; }

        //Image sizing information
        public string SO { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        //order by
        public string OB { get; set; }
        public string OBD { get; set; }

        public WallbaseImageSearchSettings()
        {
            SA = "search";

            SO = "gteq";
            ImageWidth = (int)SystemParameters.PrimaryScreenWidth;
            ImageHeight = (int)SystemParameters.PrimaryScreenHeight;

            OB = "relevance";
            OBD = "desc";

            Color = System.Drawing.Color.Empty;
        }

        public class SearchArea
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<SearchArea> GetSearchAreas()
            {
                List<SearchArea> sa = new List<SearchArea>();

                sa.Add(new SearchArea() { Name = "Search", Value = "search" });
                sa.Add(new SearchArea() { Name = "Top List", Value = "toplist" });
                sa.Add(new SearchArea() { Name = "Random", Value = "random" });

                return sa;
            }
        }

        public class OrderBy
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<OrderBy> GetOrderByList()
            {
                List<OrderBy> sa = new List<OrderBy>();

                sa.Add(new OrderBy() { Name = "Relevancy", Value = "relevance" });
                sa.Add(new OrderBy() { Name = "Date", Value = "date" });
                sa.Add(new OrderBy() { Name = "Views", Value = "views" });
                sa.Add(new OrderBy() { Name = "Favorites", Value = "favs" });
                sa.Add(new OrderBy() { Name = "Random", Value = "random" });

                return sa;
            }
        }

        public class OrderByDirection
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<OrderByDirection> GetDirectionList()
            {
                List<OrderByDirection> sa = new List<OrderByDirection>();

                sa.Add(new OrderByDirection() { Name = "Descending", Value = "desc" });
                sa.Add(new OrderByDirection() { Name = "Ascending", Value = "asc" });

                return sa;
            }
        }

        public class SizingOption
        {
            public string Name { get; private set; }
            public string Value { get; private set; }

            public static List<SizingOption> GetDirectionList()
            {
                List<SizingOption> sa = new List<SizingOption>();

                sa.Add(new SizingOption() { Name = "Exactly", Value = "eqeq" });
                sa.Add(new SizingOption() { Name = "At Least", Value = "gteq" });

                return sa;
            }
        }
    }
}
