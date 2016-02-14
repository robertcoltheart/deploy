using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Deploy.Win32
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiCloseHandle(IntPtr hAny);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr MsiCreateRecord(uint cParams);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiDatabaseOpenView(IntPtr hDatabase, string szQuery, out IntPtr hView);
        
        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiGetSummaryInformation(IntPtr hDatabase, string szDatabasePath, uint uiUpdateCount, out IntPtr hSummaryInfo);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr phPersist, out IntPtr phDatabase);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiRecordSetStream(IntPtr hRecord, uint iField, string szFilePath);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiSummaryInfoPersist(IntPtr hSummaryInfo);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiSummaryInfoSetProperty(IntPtr hSummaryInfo, uint uiProperty, uint uiDataType, int iValue, ref long ftValue, string szValue);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiDatabaseCommit(IntPtr database);

        [PreserveSig]
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        internal static extern uint MsiViewExecute(IntPtr hView, IntPtr hRecord);
    }
}