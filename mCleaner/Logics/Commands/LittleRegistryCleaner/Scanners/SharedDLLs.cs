﻿
using Microsoft.Win32;
using System.IO;
namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class SharedDLLs : ScannerBase
    {
        public SharedDLLs() { }
        static SharedDLLs _i = new SharedDLLs();
        public static SharedDLLs I { get { return _i; } }

        public void Clean(bool preview)
        {
            if (preview)
            {
                Preview(); 
            }
            else
            {
                Clean();
            }
        }

        public void Clean()
        {
            Preview();

            foreach (InvalidKeys k in this.BadKeys)
            {
                using (RegistryKey key = k.Root.OpenSubKey(k.Subkey, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(k.Name);
                    }
                }
            }
        }

        public void Preview()
        {
            this.BadKeys.Clear();

            try
            {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\SharedDLLs");

                if (regKey == null)
                    return;

                // Validate Each DLL from the value names
                foreach (string strFilePath in regKey.GetValueNames())
                {
                    if (!string.IsNullOrEmpty(strFilePath))
                    {
                        if (!File.Exists(strFilePath))
                        {
                            //ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, strFilePath);

                            this.BadKeys.Add(new InvalidKeys()
                            {
                                Root = Registry.LocalMachine,
                                Subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\SharedDLLs",
                                Key = string.Empty,
                                Name = strFilePath
                            });
                        }
                    }
                }

                regKey.Close();
            }
            catch (System.Security.SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
