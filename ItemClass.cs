using System.Collections.ObjectModel;

namespace IconPosBackup
{
    public class ItemsViewModel
    {
        public ObservableCollection<IconPosBackupItem> Items { get; set; }

        public ItemsViewModel()
        {
            Items = [];
        }
    }

    public class IconPosBackupItem
    {
        public string? Title { get; set; }
        public ulong Id { get; set; }
    }
}
