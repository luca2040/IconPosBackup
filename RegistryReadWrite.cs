using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace IconPosBackup
{
    public class RegistryReadWrite
    {
        public static List<RegistryItem> GetCurrentUserRegistryContent(string currentUserPath)
        {
            List<RegistryItem> registryItems = [];

            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(currentUserPath);
                if (key != null)
                {
                    string[] keyValueList = key.GetValueNames();

                    foreach (string valueName in keyValueList)
                    {
                        object? value = key.GetValue(valueName);
                        if (value != null)
                        {
                            RegistryValueKind valueKind = key.GetValueKind(valueName);

                            var valueType = valueKind switch
                            {
                                RegistryValueKind.DWord => 0,
                                RegistryValueKind.String => 1,
                                RegistryValueKind.Binary => 2,
                                _ => -1,
                            };

                            if (valueType != -1)
                            {
                                RegistryItem newItem = new()
                                {
                                    KeyName = valueName,
                                    Type = valueType,
                                    Value = value
                                };
                                registryItems.Add(newItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading registry\nException: " + ex.Message);
                return [];
            }

            return registryItems;
        }


        public static void SetCurrentUserValue(string currentUserPath, string valName, object value)
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(currentUserPath, true) ?? throw new InvalidOperationException("Cant open registry key");

                if (value is int v)
                {
                    key.SetValue(valName, v, RegistryValueKind.DWord);
                }
                else if (value is string v1)
                {
                    key.SetValue(valName, v1, RegistryValueKind.String);
                }
                else if (value is byte[] v2)
                {
                    key.SetValue(valName, v2, RegistryValueKind.Binary);
                }
                else
                {
                    throw new ArgumentException("Value type not supported");
                }

                Debug.WriteLine($"set {valName} in {currentUserPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting key values: {ex.Message}");
            }
        }

        public class RegistryItem
        {
            public string? KeyName { get; set; }

            // Type:
            // 0 = REG_DWORD
            // 1 = REG_SZ
            // 2 = REG_BINARY

            // i know long is useless for just 3 nums but db is like this so i dont care
            // yes there are better solutions but this works the same
            public long? Type { get; set; }
            public object? Value { get; set; }
        }
    }

    public static class ExplorerProcess
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        const int WM_USER = 0x0400; //http://msdn.microsoft.com/en-us/library/windows/desktop/ms644931(v=vs.85).aspx

        public static void CloseExplorer()
        {
            try
            {
                var ptr = FindWindow("Shell_TrayWnd", null);
                PostMessage(ptr, WM_USER + 436, 0, 0);

                do
                {
                    ptr = FindWindow("Shell_TrayWnd", null);

                    if (ptr.ToInt32() == 0)
                    {
                        break;
                    }

                    Thread.Sleep(500);
                } while (true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} {1}", ex.Message, ex.StackTrace);
            }
        }

        public static void RestartExplorer()
        {
            string explorer = string.Format("{0}\\{1}", Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
            Process process = new();
            process.StartInfo.FileName = explorer;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}