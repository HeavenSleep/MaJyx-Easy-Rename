using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace MaJyxER
{
    /// <summary>
    /// Indicate file status
    /// </summary>
    public enum FileActionStatus
    {
        PENDING,
        DONE,
        FAILED
    }

    /// <summary>
    /// Handle logic for given file
    /// 
    /// The class should be instancied using the static method Evaluate
    /// </summary>
    public class FileAction
    {

        #region Variables declaration
        public string Source { get; set; }
        public string Destination { get; set; }
        public FileActionStatus Status { get; set; }
        #endregion

        private FileAction(string source, string dest)
        {
            Source = source;
            Destination = dest;
            Status = FileActionStatus.PENDING;
        }

        /// <summary>
        /// Evaluate if a given file match the given rules
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rules"></param>
        /// <returns>FileAction</returns>
        public static FileAction Evaluate(string source, Rules rules)
        {
            // Implement sed like logic for regex
            string ruleMatch = rules.Regex.Split('/')[1];
            string ruleReplace = rules.Regex.Split('/')[2];

            // RightToLeft greatly improve performance
            // and also avoid infinite loop
            Regex regex = new Regex(ruleMatch, RegexOptions.RightToLeft);

            string input = source;
            if (!rules.MatchAllPath)
                input = Path.GetFileName(source);

            if (!regex.IsMatch(input))
                return null;

            string dest = regex.Replace(input, ruleReplace);
            if (!rules.MatchAllPath)
                dest = Path.Join(Path.GetDirectoryName(source), dest);

            return new FileAction(source, dest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FileActionStatus Process()
        {
            if (!Directory.Exists(Path.GetDirectoryName(Destination)))
                Directory.CreateDirectory(Path.GetDirectoryName(Destination));

            try
            {
                File.Move(Source, Destination);

                if (File.Exists(Destination) && !File.Exists(Source))
                    Status = FileActionStatus.DONE;
                else
                    Status = FileActionStatus.FAILED;

            }catch (IOException)
            {
                Status = FileActionStatus.FAILED;
            }

            return Status;
        }
    }
}
