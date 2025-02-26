using System.Collections.ObjectModel;

namespace IconPosBackup
{
    public class ItemsViewModel
    {
        public ObservableCollection<IconPosBackupItem> Items { get; set; }

        public ItemsViewModel()
        {
            Items =
            [
                new() { Title = "Nascondi", Id = 1 },
                new() { Title = "Vedi", Id = 2 },
                new() { Title = "Visualizza bordo edit", Id = 3 },
                new() { Title = "Togli bordo edit", Id = 4 },
            ];
        }
    }

    public class IconPosBackupItem
    {
        public string? Title { get; set; }
        public ulong Id { get; set; }
    }
}
