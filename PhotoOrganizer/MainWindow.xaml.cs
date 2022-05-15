using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PhotoOrganizer.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PhotoOrganizer;

public sealed partial class MainWindow : Window
{
    //public RowDefinitionCollection? RowDefinitions { get; }
    //public ColumnDefinitionCollection? ColumnDefinitions { get; }

    public MainWindow()
    {
        this.InitializeComponent();

        Title ="Photo Organizer";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);

        ViewModel = Ioc.Default.GetService<MainWindowViewModel>();

        OutputFolderExample();
    }

    public MainWindowViewModel? ViewModel { get; }

    public StorageFolder? selectedInputFolder { get; set; }

    public StorageFolder? selectedOutputFolder { get; set; }

    private async void btnStart_Click(object sender, RoutedEventArgs e)
    {
        ContentDialogResult result = await dialogSettings.ShowAsync();

        if (result is ContentDialogResult.Primary && ViewModel is not null)
        {
            ViewModel.UpdateInputFolderPathCommand?.Execute(selectedInputFolder?.Path);
            ViewModel.UpdateOutputFolderPathCommand?.Execute(selectedOutputFolder?.Path);

            ViewModel.LoadPhotosCommand?.Execute(selectedInputFolder?.Path);
        }
    }

    private async Task<StorageFolder?> SelectFolderAsync()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
        return await folderPicker.PickSingleFolderAsync();
    }

    private async void btnInputFolder_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder? folder = await SelectFolderAsync();

        if (folder is not null && ViewModel is not null)
        {
            selectedInputFolder = folder;
            txtInputFolder.Text = folder.Path;
        }
    }

    private async void btnOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        StorageFolder? folder = await SelectFolderAsync();

        if (folder is not null)
        {
            selectedOutputFolder = folder;
            txtOutputFolder.Text = folder.Path;
        }
    }

    private void FolderStructureCheckBox_Click(object sender, RoutedEventArgs e) => OutputFolderExample();

    private void OutputFolderExample()
    {
        string example = @"[Output]";

        if (selectedOutputFolder?.Path.Length>0) example = selectedOutputFolder.Path;

        string dateFormat = CreateDateFolderFormat();

        if (dateFormat.Length>0) example += DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);

        example += @"\[FileName]";

        ExampleTextBlock.Text = example;
    }


    private string CreateDateFolderFormat()
    {
        string format = string.Empty;

        if (chkCreatedYear.IsChecked is true) format += @"\\yyyy";

        if (chkCreatedMonth.IsChecked is true) format += @"\\MM";

        if (chkCreatedDay.IsChecked is true) format += @"\\dd";

        if (chkCreatedDate.IsChecked is true) format += @"\\yyyy-MM-dd";

        return format;
    }

    private void PhotosList_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        ViewModel?.PreparePhotoCommand?.ExecuteAsync(args.Index);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.LoadPhotosCommand.Cancel();
    }
}
