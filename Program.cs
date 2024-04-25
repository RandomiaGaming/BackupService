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
            /*Console.WriteLine("Backing up files...");
            if (Backup("", SourceDrivePath, DestinationDrivePath))
            {
                goto PressAnyKeyToExit;
            }
            Console.WriteLine("Pruning removed files...");
            if (Prune("", SourceDrivePath, DestinationDrivePath))
            {
                goto PressAnyKeyToExit;
            }*/
            Console.WriteLine("Pushing all git repos...");
            if (BackupGitRepos("", SourceDrivePath, DestinationDrivePath))
            {
                goto PressAnyKeyToExit;
            }
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

        // Scans through source drive for git repos and makes sure all changes are commited and pushed to remote origin.
        public static bool BackupGitRepos(string subPath, string source, string destination)
        {
            // Run git backup commands if this is a git repo.
            bool gitCommit = false; // Don't run git commands by default.
            if(subPath.Length == 0 && Directory.Exists(source + ".git"))
            {
                gitCommit = true; // Run git commands if the folder is a git repo.
            }
            else if (subPath.Length != 0 && Directory.Exists(source + subPath + "\\.git"))
            {
                gitCommit = true; // Run git commands if the folder is a git repo.
            }
            if (gitCommit)
            {
                string out1 = RunCommand("git rm --cached -r *", source + subPath);
                string out2 = RunCommand("git add *", source + subPath);
                string out3 = RunCommand("git commit -m\"Backup Commit\"", source + subPath);
                string out4 = RunCommand("git push origin --all", source + subPath);
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
                    Attempt(() => { Directory.Delete(destinationFolder, true); });
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
                    Attempt(() => { File.Delete(destinationFile); });
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
        // Backs up files from source to destination which have been modified recursively.
        public static bool Backup(string subPath, string source, string destination)
        {
            // Create the destination directory if it doesn't exist already.
            if (!Directory.Exists(destination + subPath))
            {
                Attempt(() => { Directory.CreateDirectory(destination + subPath); });
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
                    Attempt(() => { File.Copy(sourceFile, destinationFile, true); });
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
        // Runs a cmd command in a certain working directory.
        public static string RunCommand(string command, string workingDirectory)
        {
            if (workingDirectory.StartsWith("\\\\?\\"))
            {
                workingDirectory = workingDirectory.Substring("\\\\?\\".Length);
            }
            // Create a process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe"; // Use cmd.exe to run the command
            processStartInfo.Arguments = $"/c {command}"; // Pass the command to be executed
            processStartInfo.WorkingDirectory = workingDirectory; // Set the working directory
            processStartInfo.RedirectStandardOutput = true; // Redirect standard output
            processStartInfo.RedirectStandardError = true; // Redirect standard error
            processStartInfo.UseShellExecute = false; // Don't use the shell to execute the command
//            processStartInfo.CreateNoWindow = true; // Don't create a window

            // Run process and get output.
            Process process = Process.Start(processStartInfo);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(process.StandardError.ReadToEnd());
            Console.ForegroundColor = ConsoleColor.White;
            process.WaitForExit();

            return "";
        }

        public static void Attempt(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}