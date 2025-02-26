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
    public MainWindow()
    {
        InitializeComponent();
        Title = "Backup desktop icon position";
        DataContext = new ItemsViewModel();
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
    }


    // Rename button
    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Apply clicked");
    }


    // Rename button
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Delete clicked");
    }


    // Rename button
    private void Add_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Add clicked");

        List<RegistryReadWrite.RegistryItem> items = RegistryReadWrite.GetCurrentUserRegistryContent(@"SOFTWARE\Microsoft\Windows\Shell\Bags\1\Desktop");
        foreach (RegistryReadWrite.RegistryItem item in items)
        {
            Debug.WriteLine($"Key: {item.KeyName}, Type: {item.Type}");
        }
    }


    private void ItemList_selectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is IconPosBackupItem selectedItem)
        {
            ulong itemId = selectedItem.Id;
            Debug.WriteLine($"Selected Item ID: {itemId}");

            if (itemId == 1)
            {
                NoElementSelectedText.Visibility = Visibility.Visible;

                SelectedElementName.Visibility = Visibility.Collapsed;
                RenameButton.Visibility = Visibility.Collapsed;
                BackupButtonsGrid.Visibility = Visibility.Collapsed;
            }
            else if (itemId == 2)
            {
                SelectedElementName.Visibility = Visibility.Visible;
                RenameButton.Visibility = Visibility.Visible;
                BackupButtonsGrid.Visibility = Visibility.Visible;

                NoElementSelectedText.Visibility = Visibility.Collapsed;
            }
            else if (itemId == 3)
            {
                SelectedElementName.BorderThickness = new Thickness(1);
            }
            else
            {
                SelectedElementName.BorderThickness = new Thickness(0);
            }
        }
    }
}