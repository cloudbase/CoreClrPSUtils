﻿using System;
using System.Runtime.InteropServices;

namespace Cloudbase.PSUtils
{
    public class ProcessManager
    {
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_BATCH = 4;
        const int LOGON32_LOGON_SERVICE = 5;
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int TOKEN_ALL_ACCESS = 0x000f01ff;
        const uint GENERIC_ALL_ACCESS = 0x10000000;
        const uint INFINITE = 0xFFFFFFFF;
        const uint PI_NOUI = 0x00000001;
        const uint WAIT_FAILED = 0xFFFFFFFF;

        enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROFILEINFO
        {
            public int dwSize;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpUserName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpProfilePath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpDefaultPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpPolicyPath;
            public IntPtr hProfile;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_4
        {
            public string name;
            public string password;
            public int password_age;
            public int priv;
            public string home_dir;
            public string comment;
            public int flags;
            public string script_path;
            public int auth_flags;
            public string full_name;
            public string usr_comment;
            public string parms;
            public string workstations;
            public int last_logon;
            public int last_logoff;
            public int acct_expires;
            public int max_storage;
            public int units_per_week;
            public IntPtr logon_hours;    // This is a PBYTE
            public int bad_pw_count;
            public int num_logons;
            public string logon_server;
            public int country_code;
            public int code_page;
            public IntPtr user_sid;     // This is a PSID
            public int primary_group_id;
            public string profile;
            public string home_dir_drive;
            public int password_expired;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle,
                                                 UInt32 dwMilliseconds);

        [DllImport("Kernel32.dll")]
        static extern int GetLastError();

        [DllImport("Kernel32.dll")]
        extern static int CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetExitCodeProcess(IntPtr hProcess,
                                              out int lpExitCode);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LoadUserProfile(IntPtr hToken,
                                           ref PROFILEINFO lpProfileInfo);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnloadUserProfile(IntPtr hToken, IntPtr hProfile);

        public static int RunProcess(string userName, string password,
                                      string domain, string cmd,
                                      string arguments,
                                      bool loadUserProfile = true,
                                      int logonType = LOGON32_LOGON_BATCH)
        {
            bool retValue;
            IntPtr phToken = IntPtr.Zero;
            IntPtr phTokenDup = IntPtr.Zero;
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();
            PROFILEINFO pi = new PROFILEINFO();

            try
            {
                retValue = LogonUser(userName, domain, password,
                                     logonType,
                                     LOGON32_PROVIDER_DEFAULT,
                                     out phToken);
                if (!retValue)
                    throw new Exception("Failed to logon as user: " + GetLastError());

                var sa = new SECURITY_ATTRIBUTES();
                sa.nLength = Marshal.SizeOf(sa);

                retValue = DuplicateTokenEx(
                    phToken, GENERIC_ALL_ACCESS, ref sa,
                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    TOKEN_TYPE.TokenPrimary, out phTokenDup);
                if (!retValue)
                    throw new Exception("Failed to duplicate token: " + GetLastError());

                STARTUPINFO sInfo = new STARTUPINFO();
                sInfo.lpDesktop = "";

                if (loadUserProfile)
                {
                    pi.dwSize = Marshal.SizeOf(pi);
                    pi.dwFlags = PI_NOUI;
                    pi.lpUserName = userName;

                    retValue = LoadUserProfile(phTokenDup, ref pi);
                    if (!retValue)
                        throw new Exception("Failed to get user profile: " + GetLastError());
                }

                retValue = CreateProcessAsUser(phTokenDup, cmd, arguments,
                                               ref sa, ref sa, false, 0,
                                               IntPtr.Zero, null,
                                               ref sInfo, out pInfo);
                if (!retValue)
                    throw new Exception("Failed to create process as user: " + GetLastError());

                if (WaitForSingleObject(pInfo.hProcess, INFINITE) == WAIT_FAILED)
                    throw new Exception("Process returned exit status: " + GetLastError());

                int exitCode;
                retValue = GetExitCodeProcess(pInfo.hProcess, out exitCode);
                if (!retValue)
                    throw new Exception("Failed to get exit code status" + GetLastError());

                return exitCode;
            }
            finally
            {
                if (pi.hProfile != IntPtr.Zero)
                    UnloadUserProfile(phTokenDup, pi.hProfile);
                if (phToken != IntPtr.Zero)
                    CloseHandle(phToken);
                if (phTokenDup != IntPtr.Zero)
                    CloseHandle(phTokenDup);
                if (pInfo.hProcess != IntPtr.Zero)
                    CloseHandle(pInfo.hProcess);
            }
        }
    }
}