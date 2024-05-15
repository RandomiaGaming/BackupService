using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace BackupService
{
    public static class Program
    {
        public static string SourceDrivePath;
        public static string DestinationDrivePath;
        public static readonly string[] CodeExtensions = new string[] { ".c", ".cpp", ".h", ".hpp", ".vcxproj", ".cs", ".csproj", ".sln", ".java", ".py", ".js", ".html", ".css", ".htm", ".rb", ".swift", ".go", ".php", ".r", ".pl", "sh", ".bat", ".sql", ".xml", ".json", ".ts", ".kt", ".dart", ".rs" };
        public static void Main()
        {
            Console.WriteLine("It\'s Backup Time Motha Fuckers...");
            Console.WriteLine();
            Console.WriteLine("PLEASE DO NOT USE SOURCE OR DESTINATION DRIVE DURING BACKUP!!!");
            Console.WriteLine();
            if (IdentifyDrives())
            {
                goto PressAnyKeyToExit;
            }
             Console.WriteLine("Scanning for code outside of git repos...");
             if (ScanUnprotectedCode("", SourceDrivePath, DestinationDrivePath))
             {
                 goto PressAnyKeyToExit;
             }
             Console.WriteLine("Validating all git repos...");
             if (ValidateGitRepos("", SourceDrivePath, DestinationDrivePath))
             {
                 goto PressAnyKeyToExit;
             }
             Console.WriteLine("Pushing all git repos...");
             if (BackupGitRepos("", SourceDrivePath, DestinationDrivePath))
             {
                 goto PressAnyKeyToExit;
             }
             Console.WriteLine("Backing up files...");
             if (Backup("", SourceDrivePath, DestinationDrivePath))
             {
                 goto PressAnyKeyToExit;
             }
            Console.WriteLine("Pruning removed files...");
            if (Prune("", SourceDrivePath, DestinationDrivePath))
            {
                goto PressAnyKeyToExit;
            }
            Console.WriteLine();
            Console.WriteLine("All tasks completed successfully.");
            Console.WriteLine();
        PressAnyKeyToExit:
            Console.WriteLine("Press any key to exit...");
            Stopwatch bufferConsumeStopWatch = Stopwatch.StartNew();
            while (true)
            {
                Console.ReadKey(true);
                if (bufferConsumeStopWatch.ElapsedTicks > 10000000)
                {
                    break;
                }
            }
            Process.GetCurrentProcess().Kill();
        }

        // Approved 1
        // Locates the source and destination drives by their .backup marker files.
        public static bool IdentifyDrives()
        {
            Console.WriteLine("Identifying source and destination drives...");

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (File.Exists("\\\\?\\" + drive.Name + "source.backup"))
                {
                    SourceDrivePath = "\\\\?\\" + drive.Name;
                    break;
                }
            }
            if (SourceDrivePath is null)
            {
                Console.WriteLine("source.backup marker file could not be found.");
                Console.WriteLine();
                return true;
            }
            Console.WriteLine($"Located source drive \"{SourceDrivePath}\".");

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (File.Exists("\\\\?\\" + drive.Name + "destination.backup"))
                {
                    DestinationDrivePath = "\\\\?\\" + drive.Name;
                    break;
                }
            }
            if (DestinationDrivePath is null)
            {
                Console.WriteLine("destination.backup marker file could not be found.");
                Console.WriteLine();
                return true;
            }
            Console.WriteLine($"Located destination drive \"{DestinationDrivePath}\".");

            Console.WriteLine();
            return false;
        }

        // Approved 1
        // Scans through all folders for code which is not in a git repo.
        public static bool ScanUnprotectedCode(string subPath, string source, string destination)
        {
            bool isRepo = false; // Assume a folder is probably not a git repo.
            if (subPath.Length == 0 && Directory.Exists(source + ".git"))
            {
                isRepo = true;
            }
            else if (subPath.Length != 0 && Directory.Exists(source + subPath + "\\.git"))
            {
                isRepo = true;
            }
            if (isRepo)
            {
                return false; // Return early if this is a git repo. No sense searching furthar.
            }

            // Enumerate files in source for code.
            foreach(string sourceFile in Directory.GetFiles(source + subPath))
            {
                string ext = Path.GetExtension(sourceFile);
                bool isCode = false; // Assume files probably aren't code.
                foreach(string codeExtension in CodeExtensions)
                {
                    if(ext == codeExtension)
                    {
                        isCode = true;
                        break;
                    }
                }
                if(isCode && !isRepo)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: File \"{sourceFile}\" is code yet is not in a git repo.");
                    Console.ForegroundColor = ConsoleColor.White;

                    return false; // Don't dig any deeper.
                }
            }

            // Recursively check sub folders.
            foreach (string sourceFolder in Directory.GetDirectories(source + subPath))
            {
                bool scan = true; // Scan sub folders by default.
                if (subPath.Length == 0 && (new DirectoryInfo(sourceFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    scan = false; // Don't scan hidden or system folders in the root of source.
                }
                if (scan)
                {
                    ScanUnprotectedCode(sourceFolder.Substring(source.Length), source, destination);
                }
            }

            return false;
        }

        // Approved 1
        // Scans through all git repos in source for .gitignores, no remote origin.
        public static bool ValidateGitRepos(string subPath, string source, string destination)
        {
            // Run git backup commands if this is a git repo.
            bool isRepo = false; // Assume a folder is probably not a git repo.
            if (subPath.Length == 0 && Directory.Exists(source + ".git"))
            {
                isRepo = true;
            }
            else if (subPath.Length != 0 && Directory.Exists(source + subPath + "\\.git"))
            {
                isRepo = true;
            }
            if (isRepo)
            {
                bool hasIgnore = false; // Assume there isn't a .gitignore.
                if (subPath.Length == 0 && File.Exists(source + ".gitignore"))
                {
                    hasIgnore = true;
                }
                else if (subPath.Length != 0 && File.Exists(source + subPath + "\\.gitignore"))
                {
                    hasIgnore = true;
                }
                if (!hasIgnore)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning: Git repo in folder \"{source + subPath}\" does not have a .gitignore file.");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                bool hasOrigin = false; // Assume there is no remote origin.
                string output = RunGitCommand("remote get-url origin", source + subPath);
                if(output.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: Git repo in folder \"{source + subPath}\" does not have a remote origin.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            // Recursively check sub folders.
            foreach (string sourceFolder in Directory.GetDirectories(source + subPath))
            {
                bool scan = true; // Scan sub folders by default.
                if (subPath.Length == 0 && (new DirectoryInfo(sourceFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    scan = false; // Don't scan hidden or system folders in the root of source.
                }
                if (scan)
                {
                   ValidateGitRepos(sourceFolder.Substring(source.Length), source, destination);
                }
            }

            return false;
        }

        // Approved 1
        // Scans through source drive for git repos and makes sure all changes are commited and pushed to remote origin.
        public static bool BackupGitRepos(string subPath, string source, string destination)
        {
            // Run git backup commands if this is a git repo.
            bool gitCommit = false; // Don't run git commands by default.
            if (subPath.Length == 0 && Directory.Exists(source + ".git"))
            {
                gitCommit = true; // Run git commands if the folder is a git repo.
            }
            else if (subPath.Length != 0 && Directory.Exists(source + subPath + "\\.git"))
            {
                gitCommit = true; // Run git commands if the folder is a git repo.
            }
            if (gitCommit)
            {
                RunGitCommand("rm --cached -r *", source + subPath);
                RunGitCommand("add *", source + subPath);
                RunGitCommand("commit -m\"Backup Commit\"", source + subPath);
                RunGitCommand("push origin --all", source + subPath);
            }

            // Recursively check sub folders.
            foreach (string sourceFolder in Directory.GetDirectories(source + subPath))
            {
                bool scan = true; // Scan sub folders by default.
                if (subPath.Length == 0 && (new DirectoryInfo(sourceFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    scan = false; // Don't scan hidden or system folders in the root of source.
                }
                if (scan)
                {
                    BackupGitRepos(sourceFolder.Substring(source.Length), source, destination);
                }
            }

            return false;
        }

        // Approved 1
        // Backs up files from source to destination which have been modified recursively.
        public static bool Backup(string subPath, string source, string destination)
        {
            // Create the destination directory if it doesn't exist already.
            if (!Directory.Exists(destination + subPath))
            {
                Directory.CreateDirectory(destination + subPath);
            }

            // Back up files in source.
            foreach (string sourceFile in Directory.GetFiles(source + subPath))
            {
                string destinationFile = destination + sourceFile.Substring(source.Length);
                bool backup = false; // Don't backup files by default
                if (!File.Exists(destinationFile))
                {
                    backup = true; // Backup files that do not exist in the destination yet.
                }
                if (new FileInfo(sourceFile).LastWriteTime.Ticks - new FileInfo(destinationFile).LastWriteTime.Ticks > 600000000L)
                {
                    backup = true; // Backup files modified since last backup or within 1 minute of last backup.
                }
                if (sourceFile == source + "source.backup")
                {
                    backup = false; // Don't backup source.backup file.
                }
                if (backup)
                {
                    try
                    {
                        File.Copy(sourceFile, destinationFile, true);
                    }
                    catch
                    {

                    }
                }
            }

            // Recursively backup sub folders of source.
            foreach (string sourceFolder in Directory.GetDirectories(source + subPath))
            {
                bool backup = true; // Backup sub folders by default.
                if (subPath.Length == 0 && (new DirectoryInfo(sourceFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    backup = false; // Don't backup hidden or system folders in the root of source.
                }
                if (backup)
                {
                    Backup(sourceFolder.Substring(source.Length), source, destination);
                }
            }

            return false;
        }

        // Approved 1
        // Prunes files and folders from destination which have been deleted from source recursively.
        public static bool Prune(string subPath, string source, string destination)
        {
            // Prune folders from backup which have been deleted from source.
            foreach (string destinationFolder in Directory.GetDirectories(destination + subPath))
            {
                string sourceFolder = source + destinationFolder.Substring(destination.Length);
                bool prune = !Directory.Exists(sourceFolder); // Prune folders which don't exist in source.
                if (subPath.Length == 0 && (new DirectoryInfo(destinationFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    prune = false; // Don't prune hidden or system folders in the root of destination.
                }
                if (prune)
                {
                    try
                    {
                        Directory.Delete(destinationFolder, true);
                    }
                    catch
                    {

                    }
                }
            }

            // Prune files from destination which have been deleted from source.
            foreach (string destinationFile in Directory.GetFiles(destination + subPath))
            {
                string sourceFile = source + destinationFile.Substring(destination.Length);
                bool prune = !File.Exists(sourceFile); // Prune files which don't exist in source.
                if (destinationFile == destination + "destination.backup")
                {
                    prune = false; // Don't prune the destination.backup file.
                }
                if (prune)
                {
                    try
                    {
                        File.Delete(destinationFile);
                    }
                    catch
                    {

                    }
                }
            }

            // Recursively calls this method on sub folders of the current folder.
            foreach (string destinationFolder in Directory.GetDirectories(destination + subPath))
            {
                bool prune = true; // Prune sub folders by default.
                if (subPath.Length == 0 && (new DirectoryInfo(destinationFolder).Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                {
                    prune = false; // Don't prune hidden or system folders in the root of destination.
                }
                if (prune)
                {
                    Prune(destinationFolder.Substring(source.Length), source, destination);
                }
            }

            return false;
        }

        // Approved 1
        // Runs a cmd command in a certain working directory.
        public static string RunGitCommand(string command, string workingDirectory)
        {
            if (workingDirectory.StartsWith("\\\\?\\"))
            {
                workingDirectory = workingDirectory.Substring("\\\\?\\".Length);
            }
            // Create a process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "git"; // Use cmd.exe to run the command
            processStartInfo.Arguments = $"{command}"; // Pass the command to be executed
            processStartInfo.WorkingDirectory = workingDirectory; // Set the working directory
            processStartInfo.RedirectStandardOutput = true; // Redirect standard output
            processStartInfo.RedirectStandardError = true; // Redirect standard error
            processStartInfo.UseShellExecute = false; // Don't use the shell to execute the command
            processStartInfo.CreateNoWindow = true; // Don't create a window

            // Run process and get output.
            Process process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (error.Length > 0 && !error.StartsWith("Everything up-to-date"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Git error for repo at \"{workingDirectory}\".");
                Console.WriteLine(error);
                Console.ForegroundColor = ConsoleColor.White;
            }

            return output;
        }
    }
}