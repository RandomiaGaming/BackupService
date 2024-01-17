namespace BackupService
{
    public static class Program
    {
        public static long BackedUpFileCount = 0;
        public static void Main()
        {
            Backup();
        }
        public static void Backup()
        {
            try
            {
                Console.WriteLine("YOOOO its backup time motha fucker!");
                Console.WriteLine();
                BackupFolder("D:\\", "F:\\Backup\\Important Data", "");
                BackupFolder("E:\\", "F:\\Backup\\Media Archive", "");
                Console.WriteLine();
                Console.WriteLine("Backup complete!");
                Console.PressAnyKeyToExit();
            }
            catch (System.Exception ex)
            {
                Console.WriteFatalError(ex);
            }
        }
        private static void BackupFolder(string sourcePath, string destinationPath, string subSourcePath)
        {
            string sourceFolderPath;
            string destinationFolderPath;
            if (subSourcePath is null || subSourcePath.Length is 0)
            {
                sourceFolderPath = sourcePath;
                destinationFolderPath = destinationPath;
            }
            else
            {
                sourceFolderPath = $"{sourcePath}\\{subSourcePath}";
                destinationFolderPath = $"{destinationPath}\\{subSourcePath}";
            }
            try
            {
                if (!System.IO.Directory.Exists(destinationFolderPath))
                {
                    System.IO.Directory.CreateDirectory(destinationFolderPath);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to create destination directory at \"{destinationFolderPath}\" due to error \"{ex.Message}\"");
                return;
            }
            try
            {
                string[] files = System.IO.Directory.GetFiles(sourceFolderPath);
                foreach (string file in files)
                {
                    try
                    {
                        BackupFile(file, destinationFolderPath + "\\" + System.IO.Path.GetFileName(file));
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteError($"Unable to backup file \"{file}\" due to error \"{ex.Message}\"");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to get files within folder \"{sourceFolderPath}\" due to error \"{ex.Message}\"");
            }
            try
            {
                string[] folders = System.IO.Directory.GetDirectories(sourceFolderPath);
                foreach (string folder in folders)
                {
                    try
                    {
                        BackupFolder(sourcePath, destinationPath, folder.Substring(sourcePath.Length));
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteError($"Unable to backup folder \"{folder}\" due to error \"{ex.Message}\"");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to get folders within folder \"{sourceFolderPath}\" due to error \"{ex.Message}\"");
            }
        }
        private static void BackupFile(string sourcePath, string destinationPath)
        {
            System.IO.FileStream sourceFileStream;
            try
            {
                sourceFileStream = System.IO.File.Open(sourcePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None);
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to open file stream for \"{sourcePath}\" due to error \"{ex.Message}\"");
                return;
            }
            System.IO.FileStream destinationFileStream;
            try
            {
                destinationFileStream = System.IO.File.Open(destinationPath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write, System.IO.FileShare.None);
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to open file stream for \"{sourcePath}\" due to error \"{ex.Message}\"");
                return;
            }
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead = sourceFileStream.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0)
                {
                    destinationFileStream.Write(buffer, 0, bytesRead);
                    bytesRead = sourceFileStream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteError($"Unable to copy bytes from \"{sourcePath}\" to \"{destinationPath}\" due to error \"{ex.Message}\"");
            }
            finally
            {
                try
                {
                    sourceFileStream.Close();
                }
                catch (System.Exception ex)
                {
                    Console.WriteWarning($"Unable to close file stream for \"{sourcePath}\" due to error \"{ex.Message}\"");
                }
                try
                {
                    destinationFileStream.Close();
                }
                catch (System.Exception ex)
                {
                    Console.WriteWarning($"Unable to close file stream for \"{destinationPath}\" due to error \"{ex.Message}\"");
                }
            }

            BackedUpFileCount++;
            System.Console.Title = $"Backed up {BackedUpFileCount} files";
        }
    }
}