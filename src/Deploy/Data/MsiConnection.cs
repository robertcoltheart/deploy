using System;
using System.ComponentModel;
using Deploy.Win32;

namespace Deploy.Data
{
    internal class MsiConnection : IDisposable
    {
        private const int CreateMode = 3;
        private const int MsiVersion = 500;
        private const int Properties = 2;

        private readonly IntPtr _handle;

        public MsiConnection(string filename)
        {
            ThrowOnFailure(SafeNativeMethods.MsiOpenDatabase(filename, new IntPtr(CreateMode), out _handle));
        }

        public void Commit(string productName, string author, PackagePlatform platform, int languageCode, Guid productCode)
        {
            IntPtr infoHandle;

            ThrowOnFailure(SafeNativeMethods.MsiGetSummaryInformation(_handle, null, 20, out infoHandle));
            
            SetProperty(infoHandle, 2, 30, 0, 0, "Installation Database");
            SetProperty(infoHandle, 3, 30, 0, 0, productName);
            SetProperty(infoHandle, 4, 30, 0, 0, author);
            SetProperty(infoHandle, 5, 30, 0, 0, "Installer");
            SetProperty(infoHandle, 7, 30, 0, 0, $"{(platform == PackagePlatform.X64 ? "x64" : "Intel")};{languageCode}");
            SetProperty(infoHandle, 9, 30, 0, 0, productCode.ToStringFormatted());
            SetProperty(infoHandle, 14, 3, MsiVersion, 0, string.Empty);
            SetProperty(infoHandle, 15, 3, Properties, 0, string.Empty);

            ThrowOnFailure(SafeNativeMethods.MsiSummaryInfoPersist(infoHandle));
            ThrowOnFailure(SafeNativeMethods.MsiCloseHandle(infoHandle));

            ThrowOnFailure(SafeNativeMethods.MsiDatabaseCommit(_handle));
        }

        public void Dispose()
        {
            ThrowOnFailure(SafeNativeMethods.MsiCloseHandle(_handle));
        }

        public void Execute(string sql)
        {
            IntPtr viewHandle;

            ThrowOnFailure(SafeNativeMethods.MsiDatabaseOpenView(_handle, sql, out viewHandle));
            ThrowOnFailure(SafeNativeMethods.MsiViewExecute(viewHandle, IntPtr.Zero));
            ThrowOnFailure(SafeNativeMethods.MsiCloseHandle(viewHandle));
        }

        public void ExecuteStream(string table, string filename, string name)
        {
            IntPtr viewHandle;

            string query = $"INSERT INTO `{table}` (`Name`, `Data`) VALUES ('{name}', ?)";

            IntPtr recordHandle = SafeNativeMethods.MsiCreateRecord(1);

            ThrowOnFailure(SafeNativeMethods.MsiRecordSetStream(recordHandle, 1, filename));
            ThrowOnFailure(SafeNativeMethods.MsiDatabaseOpenView(_handle, query, out viewHandle));
            ThrowOnFailure(SafeNativeMethods.MsiViewExecute(viewHandle, recordHandle));
            ThrowOnFailure(SafeNativeMethods.MsiCloseHandle(viewHandle));
            ThrowOnFailure(SafeNativeMethods.MsiCloseHandle(recordHandle));
        }
        
        private void SetProperty(IntPtr infoHandle, uint property, uint type, int intValue, long timeValue, string stringValue)
        {
            ThrowOnFailure(SafeNativeMethods.MsiSummaryInfoSetProperty(infoHandle, property, type, intValue, ref timeValue, stringValue ?? string.Empty));
        }
        
        private static void ThrowOnFailure(uint error)
        {
            if (error != 0)
                throw new Win32Exception((int)error);
        }
    }
}