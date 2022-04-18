using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaJyxER
{
    /// <summary>
    /// Define Rules used to search and replace files and folder
    /// </summary>
    public class Rules
    {
        public string Source { get; set; }
        public string Regex { get; set; }
        public bool MatchAllPath { get; set; }
        public bool Recursive { get; set; }
        public bool CleanFolders { get; set; }

        /// <summary>
        /// Validate that the current rules are valide
        /// </summary>
        /// <returns>bool</returns>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(Source) || !Directory.Exists(Source))
                return false;

            if (string.IsNullOrEmpty(Source) || Regex.Split('/').Length < 3)
                return false;

            return true;
        }
    }
}
