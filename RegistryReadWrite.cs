using System.Diagnostics;
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
}