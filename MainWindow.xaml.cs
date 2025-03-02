using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IconPosBackup;

/// <summary>
/// Main window class
/// </summary>
public partial class MainWindow : Window
{
    private const string REGISTRY_ICONS_PATH = @"SOFTWARE\Microsoft\Windows\Shell\Bags\1\Desktop";

    public MainWindow()
    {
        InitializeComponent();
        Title = "Backup desktop icon position";

        DatabaseHelper.DB_PATH = "D:\\utenti\\luca\\Download\\test.sqlite";

        ShowElementUI(false);
        ShowRenameBorder(false);

        ReloadElements();
    }

    // Window dragging
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    // Window minimize
    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // Window close
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    // Rename button
    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Rename clicked");

        ReloadElements();
    }


    // Rename button
    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Apply clicked");

        DatabaseHelper.RenameBackup(5, "RENAMEDDD YEAHHHHHH");
    }


    // Rename button
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Delete clicked");

        List<RegistryReadWrite.RegistryItem> items = RegistryReadWrite.GetCurrentUserRegistryContent(REGISTRY_ICONS_PATH);

        DatabaseHelper.InsertDataList(items, "testBackup");
    }


    // Rename button
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Add clicked");

        List<RegistryReadWrite.RegistryItem> items = RegistryReadWrite.GetCurrentUserRegistryContent(REGISTRY_ICONS_PATH);
        foreach (RegistryReadWrite.RegistryItem item in items)
        {
            Debug.WriteLine($"Key: {item.KeyName}, Type: {item.Type}");
        }

        DatabaseHelper.EnsureDBExists();
    }

    private void ItemList_selectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is IconPosBackupItem selectedItem)
        {
            ulong itemId = selectedItem.Id;
            Debug.WriteLine($"Selected Item ID: {itemId}");

            if (itemId == 1)
            {
                ShowElementUI(false);
            }
            else if (itemId == 2)
            {
                ShowElementUI(true);
            }
            else if (itemId == 3)
            {
                ShowRenameBorder(true);
            }
            else
            {
                ShowRenameBorder(false);
            }
        }
    }

    internal void ReloadElements()
    {
        ObservableCollection<IconPosBackupItem> newItemsList = DatabaseHelper.GetItemsViewList();

        ItemsViewModel NewDataContext = new()
        {
            Items = newItemsList
        };

        DataContext = NewDataContext;
    }

    internal void ShowElementUI(bool show)
    {
        var visibleWhenShow = show ? Visibility.Visible : Visibility.Collapsed;
        var collapseWhenShow = show ? Visibility.Collapsed : Visibility.Visible;

        SelectedElementName.Visibility = visibleWhenShow;
        RenameButton.Visibility = visibleWhenShow;
        BackupButtonsGrid.Visibility = visibleWhenShow;

        NoElementSelectedText.Visibility = collapseWhenShow;
    }

    internal void ShowRenameBorder(bool show)
    {
        int borderThickness = show ? 1 : 0;

        SelectedElementName.BorderThickness = new Thickness(borderThickness);
    }
}