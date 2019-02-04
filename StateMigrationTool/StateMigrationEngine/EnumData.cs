using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace StateMigrationBackend.StateMigrationEngine
{
    
    class EnumData
    {
        CountModel model = new CountModel();

        private CountModel GetCount(DirectoryInfo source)
        {
            int count = 0;     
            
            foreach(var fileInfo in source.GetFiles())
            {
                if (fileInfo.Exists && ((File.GetAttributes(fileInfo.FullName) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint))
                {
                    count++;
                    model.FileCount++;
                }
            }

            foreach (var childDirectoryInfo in source.GetDirectories())
            {

                if (childDirectoryInfo.Exists && (childDirectoryInfo.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    model.DirCount++;
                    count += GetCount(childDirectoryInfo).FileCount;
                }

            }
            
            return model;
        }

        internal async Task<CountModel> GetCountAsync(DirectoryInfo source)
        {
           return await Task.Run(() => GetCount(source));
        }
                
    }
    
    class CountModel
    {
        public int FileCount { get; set; } = 0;
        public int DirCount { get; set; } = 0;
    }

    [SuppressUnmanagedCodeSecurity]
    class SafeNativeMethods
    {
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const int MAX_PATH = 260;


        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            internal uint dwLowDateTime;
            internal uint dwHighDateTime;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            internal FileAttributes dwFileAttributes;
            internal FILETIME ftCreationTime;
            internal FILETIME ftLastAccessTime;
            internal FILETIME ftLastWriteTime;
            internal int nFileSizeHigh;
            internal int nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternate;

        }


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        // Assume dirName passed in is already prefixed with \\?\

        private static List<string> FilesAndDirectoryList(string dirName)
        {
            List<string> results = new List<string>();

            WIN32_FIND_DATA findData;

            IntPtr findHandle = FindFirstFile(dirName + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;
                do
                {
                    string currentFileName = findData.cFileName;

                    if ((((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0))
                    {

                        if (currentFileName != "." && currentFileName != "..")
                        {
                            List<string> childResults = FilesAndDirectoryList(Path.Combine(dirName, currentFileName));
                            results.AddRange(childResults);
                            results.Add(Path.Combine(dirName, currentFileName));
                        }

                    }
                    else
                    {
                        results.Add(Path.Combine(dirName, currentFileName));
                    }

                    found = FindNextFile(findHandle, out findData);
                }

                while (found);
            }

            FindClose(findHandle);
            return results;
        }

        public static int FilesAndDirectoryListCount(string dirName)
        {
            int count = 0;

            WIN32_FIND_DATA findData;

            IntPtr findHandle = FindFirstFile(dirName + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;
                do
                {
                    string currentFileName = findData.cFileName;

                    if ((((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0))
                    {

                        if (currentFileName != "." && currentFileName != "..")
                        {
                            int childResults = FilesAndDirectoryListCount(Path.Combine(dirName, currentFileName));

                            count += childResults;
                            count += 1;
                        }

                    }

                    else
                    {
                        count += 1;
                    }

                    found = FindNextFile(findHandle, out findData);
                }

                while (found);
            }

            FindClose(findHandle);
            return count;


        }

        public static async Task<int> FilesAndDirectoryListCountAsync(string dirName)
        {
            return await Task.Run<int>(() => FilesAndDirectoryListCount(dirName));
        }

        public static async Task<List<string>> FilesAndDirectoryListAsync(string dirName)
        {
            return await Task.Run<List<string>>(() => FilesAndDirectoryListAsync(dirName));
        }

    }


}
