﻿
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using mCleaner.Helpers;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class StartupFiles : ScannerBase
    {
        public StartupFiles() { }
        static StartupFiles _i = new StartupFiles();
        public static StartupFiles I { get { return _i; } }

        public override void Clean()
        {
            try
            {
                Preview();

                foreach (InvalidKeys k in this.BadKeys)
                {
                    try
                    {

                        using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                        {
                            BackUpRegistrykey(k);
                            key.DeleteValue(k.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public override void Preview()
        {
            this.BadKeys.Clear();

            try
            {
                // all user keys
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunServices");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnceEx");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce\\Setup");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunEx");
                checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                // current user keys
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunServices");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnceEx");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce\\Setup");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunEx");
                checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                if (Utils.Is64BitOS)
                {
                    // all user keys
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunServices");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnceEx");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce\\Setup");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunEx");
                    checkAutoRun(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run");

                    // current user keys
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunServices");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnceEx");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce\\Setup");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunEx");
                    checkAutoRun(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run");
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Checks for invalid files in startup registry key
        /// </summary>
        /// <param name="regKey">The registry key to scan</param>
        private void checkAutoRun(RegistryKey root, string subkey)
        {
            RegistryKey regKey = root.OpenSubKey(subkey);
            
            if (regKey == null)
                return;

            foreach (string strProgName in regKey.GetValueNames())
            {
                ProgressWorker.I.EnQ(string.Format("Scanning {0}\\{1}", regKey.ToString(), string.Empty));

                string strRunPath = regKey.GetValue(strProgName) as string;
                string strFilePath, strArgs;

                if (!string.IsNullOrEmpty(strRunPath))
                {
                    // Check run path by itself
                    if (File.Exists(strRunPath)) continue;

                    // See if file exists (also checks if string is null)
                    //if (FileOperations.ExtractArguments(strRunPath, out strFilePath, out strArgs))
                    //    continue;

                    //if (FileOperations.ExtractArguments2(strRunPath, out strFilePath, out strArgs))
                    //    continue;

                    if (IsFileExists(strRunPath)) continue;

                    //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, strProgName);

                    this.BadKeys.Add(new InvalidKeys()
                    {
                        Root = root,
                        Subkey = subkey,
                        Key = string.Empty,
                        Name = strProgName
                    });
                }
            }

            regKey.Close();
            return;
        }

        bool IsFileExists(string value)
        {
            bool ret = false;
            string filepath = value;
            filepath = filepath.Replace("\"", string.Empty);

            string file = string.Empty;
            for (int i = 0; i < filepath.Length; i++)
            {
                file += filepath[i];

                if (File.Exists(file))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }
}
