using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

    private string? selectedUneditedName = "";

    private const string DB_FOLDER_NAME = "IconPosBackup";
    private const string DB_FILE_NAME = "db.sqlite";

    internal static string GetDbUserPath()
    {
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(local, DB_FOLDER_NAME, DB_FILE_NAME);
    }

    public MainWindow()
    {
        InitializeComponent();
        Title = "Backup desktop icon position";

        DatabaseHelper.DB_PATH = GetDbUserPath();
        DatabaseHelper.EnsureDBExists();

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

    // When text is changed into the rename field
    private void SelectedElementNameText_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string newText = textBox.Text;
            ShowRenameBorder(newText != selectedUneditedName);
        }
    }

    // Rename button
    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        string fieldText = SelectedElementNameText.Text;

        if (fieldText == null || fieldText.Length == 0) return;

        IconPosBackupItem? currentlySelectedItem = (IconPosBackupItem)ElementsList.SelectedItem;
        ulong? currentId = currentlySelectedItem?.Id;

        if (currentId == null) return;

        DatabaseHelper.RenameBackup(currentId, fieldText);

        ReloadElements();
    }


    // Apply button
    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        IconPosBackupItem? currentlySelectedItem = (IconPosBackupItem)ElementsList.SelectedItem;
        ulong? currentId = currentlySelectedItem?.Id;

        if (currentId == null) return;

        List<RegistryReadWrite.RegistryItem> readenItems = DatabaseHelper.ReadBackupItem(currentId);

        ExplorerProcess.CloseExplorer();

        foreach (RegistryReadWrite.RegistryItem item in readenItems)
        {
            if (item.KeyName != null && item.Value != null) RegistryReadWrite.SetCurrentUserValue(REGISTRY_ICONS_PATH, item.KeyName, item.Value);
        }

        ExplorerProcess.RestartExplorer();
    }


    // Delete button
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        IconPosBackupItem? currentlySelectedItem = (IconPosBackupItem)ElementsList.SelectedItem;
        ulong? currentId = currentlySelectedItem?.Id;

        if (currentId == null) return;

        DatabaseHelper.DeleteBackup(currentId);

        ReloadElements();
    }


    // Add button
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        string newElementName = NewElementNameText.Text;

        if (newElementName == null || newElementName.Length == 0) return;

        List<RegistryReadWrite.RegistryItem> items = RegistryReadWrite.GetCurrentUserRegistryContent(REGISTRY_ICONS_PATH);
        DatabaseHelper.InsertDataList(items, newElementName);

        ReloadElements();
    }

    private void ItemList_selectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is IconPosBackupItem selectedItem)
        {
            ulong itemId = selectedItem.Id;
            Debug.WriteLine($"Selected Item ID: {itemId}");

            ShowElementUI(true);
            ShowRenameBorder(false);

            // IMPORTANT: EXECUTE IN THIS ORDER OR IT WONT WORK
            selectedUneditedName = selectedItem.Title;
            SelectedElementNameText.Text = selectedItem.Title;
        }
    }

    internal void ReloadElements()
    {
        IconPosBackupItem? currentlySelectedItem = (IconPosBackupItem)ElementsList.SelectedItem;
        ulong? currentId = currentlySelectedItem?.Id;

        ObservableCollection<IconPosBackupItem> newItemsList = DatabaseHelper.GetItemsViewList();

        ItemsViewModel NewDataContext = new()
        {
            Items = newItemsList
        };

        DataContext = NewDataContext;

        bool viewSelectedElementUI = true;
        if (currentId != null)
        {
            var selectedItem = ((ItemsViewModel)DataContext).Items.FirstOrDefault(item => item.Id == currentId);

            if (selectedItem != null)
            {
                ElementsList.SelectedValue = selectedItem;
            }
            else
            {
                viewSelectedElementUI = false;
            }
        }
        else
        {
            viewSelectedElementUI = false;
        }

        ShowElementUI(viewSelectedElementUI);
        ShowRenameBorder(false);
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