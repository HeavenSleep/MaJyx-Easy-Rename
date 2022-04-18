using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaJyxER
{
    /// <summary>
    /// Class that implement the logic to search and replaces files asynchronously
    /// </summary>
    public class FilesManager
    {
        #region Event declaration
        public delegate void MatchFoundEvent(object sender, FileAction fileAction);
        public event MatchFoundEvent MatchFound;

        public delegate void SearchFinishedEvent(object sender, int count);
        public event SearchFinishedEvent SearchFinished;

        public delegate void ApplyDoneEvent(object sender, int count);
        public event ApplyDoneEvent ApplyDone;

        public delegate void UpdateCurrentFileEvent(object sender, string filename);
        public event UpdateCurrentFileEvent OnCurrentFileChange;

        public delegate void LogMessageEvent(object sender, string message);
        public event LogMessageEvent OnMessageReceive;

        public delegate void ProcessActionEvent(object sender, FileAction fileAction);
        public event ProcessActionEvent OnProcessActionDone;
        #endregion

        #region Variables declaration
        public List<FileAction> FilesMatched;
        private List<string> FoldersToClean;
        #endregion

        public FilesManager()
        {
            FilesMatched = new List<FileAction>();
            FoldersToClean = new List<string>();
        }

        /// <summary>
        /// Start new search according to given rules
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="path"></param>
        /// <param name="isSubSearch"></param>
        /// <returns>int (files found count)</returns>
        public async Task<int> Search(Rules rules, string path, bool isSubSearch = false)
        {
            string[] dirFiles = Directory.GetFiles(path);
            string[] subDirs = Directory.GetDirectories(path);

            int filesScanned = dirFiles.Length;

            if (rules.Recursive)
                OnMessageReceive?.Invoke(this, $"[INFO] Searching folder {path} ({dirFiles.Length} files, {subDirs.Length} subdirectories)...");
            else
                OnMessageReceive?.Invoke(this, $"[INFO] Searching folder {path} ({dirFiles.Length} files)...");

            if (rules.Recursive)
            {
                foreach (string subDir in subDirs)
                {
                    // Recursively search sub-folders and wait for the result
                    filesScanned += await Search(rules, subDir, true);
                }
            }

            foreach (string file in dirFiles)
            {
                OnCurrentFileChange?.Invoke(this, file);
                FileAction fileAction = FileAction.Evaluate(file, rules);
                if (fileAction != null)
                {
                    FilesMatched.Add(fileAction);
                    MatchFound?.Invoke(this, fileAction);

                    // Register folder for potential clean needed
                    if (Path.GetDirectoryName(fileAction.Destination) != path)
                        FoldersToClean.Add(path);
                }
            }

            // We end at the main call which should be the root folder of the search
            if (!isSubSearch)
                SearchFinished?.Invoke(this, filesScanned);

            return filesScanned;
        }

        /// <summary>
        /// Apply to all or any given files to be processed
        /// </summary>
        /// <param name="fileActions"></param>
        /// <returns>int (files processed count)</returns>
        public async Task<int> Apply(List<FileAction> fileActions = null)
        {
            if (fileActions != null)
            {
                foreach(FileAction action in fileActions)
                {
                    // Check if files already processed
                    if (action.Status == FileActionStatus.PENDING)
                    {
                        action.Process();
                        OnProcessActionDone(this, action);
                    }
                }

                ApplyDone?.Invoke(this, fileActions.Count);
                return fileActions.Count;
            } else
            {
                // Apply on all known files
                return await Apply(FilesMatched);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="path"></param>
        /// <returns>int (folders deleted count)</returns>
        public async Task<int> Clean()
        {
            int foldersDeleted = 0;
            foreach (string path in FoldersToClean)
            {
                if (Directory.GetFiles(path).Length > 0 || Directory.GetDirectories(path).Length > 0)
                    continue;

                try
                {
                    Directory.Delete(path);
                    foldersDeleted++;
                    OnMessageReceive?.Invoke(this, $"[INFO] Folder {path} deleted because empty");
                }
                catch (IOException ex)
                {
                    OnMessageReceive?.Invoke(this, $"[ERROR] Couldn't delete folder {path}:\n{ex}");
                }
            }

            return foldersDeleted;
        }

    }
}
