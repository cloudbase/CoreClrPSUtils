/*
Copyright 2014 Cloudbase Solutions Srl

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Cloudbase.PSUtils
{
    public sealed class Win32IniApi
    {
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(
           string lpAppName,
           string lpKeyName,
           StringBuilder lpString,
           string lpFileName);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        [SecuritySafeCritical]
        public string GetIniValue(string app, string key, string defaultValue, string path)
        {
            var sb = new StringBuilder(1000);
            var retVal = Win32IniApi.GetPrivateProfileString(app, key, defaultValue, sb, (uint)sb.Capacity, path);
            if(retVal == 0)
            {
                var lastErr = Win32IniApi.GetLastError();
                if(lastErr != 2)
                {
                    throw new Exception("Cannot get value from ini file: " + lastErr);
                }
                else if (!System.IO.File.Exists(path))
                {
                    throw new Exception("Ini file '$Path' does not exist");
                }
            }

            return sb.ToString();
        }

        // Use StringBuilder instead of string, as Powershell replaces $null with an empty string
        [SecuritySafeCritical]
        public void SetIniValue(string app, string key, StringBuilder value, string path)
        {
            var sb = new StringBuilder(1000);
            var retVal = Win32IniApi.WritePrivateProfileString(app, key, value, path);
            if(!retVal && Win32IniApi.GetLastError() != 0)
            {
                throw new Exception("Cannot set value in ini file: " + Win32IniApi.GetLastError());
            }
        }
    }
}
