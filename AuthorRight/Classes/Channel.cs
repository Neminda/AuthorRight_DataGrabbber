using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorRightClaim.Classes
{
    class Channel
    {
        public string chnlId { get; set; }
        public string chnDisplayName { get; set; }
        public string chnLanguage { get; set; }
        public string chnIcon { get; set; }
        public string chnUrl { get; set; }

        private static string connString { get; set; }


        public static void populateChannelData(string connString)
        {
            //connString = Utilities.ReadConfigFile("TvGuide");

        }
    }
}
