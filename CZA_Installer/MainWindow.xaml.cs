 
using System.IO;
using System.Windows.Forms;
using System.Windows;
 
using System.Windows.Media;

using Path = System.IO.Path;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
namespace CZA_Installer
{

    public partial class MainWindow : Window
    {
        private string CurrentlyLoadedArchive = "";
        private string SelectedOutPutFolder = "";
        bool inMaker = false;
        bool Generating = false;
        bool Extracting = false;
        FileData Data = new FileData();
        public MainWindow()
        {
            InitializeComponent();
            MakerFooter.Visibility = Visibility.Visible;
            InstallFooter.Visibility = Visibility.Collapsed;
            inMaker = true;
        }

        internal void LoadFile(string file)
        {
            MakerFooter.Visibility = Visibility.Collapsed;
            InstallFooter.Visibility = Visibility.Visible;
            inMaker = false;
            CurrentlyLoadedArchive = file;
            ReadFile(file);


        }

        private void ReadFile(string file)
        {
            Data = ArchiveManager.ReadArchiveInfo(file);
            Title = Data.Title + $" ({Data.Year})";
            header.Source = Data.HeaderImage;


        }

        private async void install(object sender, RoutedEventArgs e)
        {
            SelectedOutPutFolder = TextBoxOutputPath.Text;
            if (Directory.Exists(SelectedOutPutFolder) == false) { MessageBox.Show("Output folder doesnt exist"); return; }
            if (Data.Listfile.Count==0) { MessageBox.Show("Empty listfile"); return; }
            Extracting = true;
            DisableButtons(true);
             await ArchiveManager.Extract(CurrentlyLoadedArchive, SelectedOutPutFolder, Data, this);
            Extracting = false;
            DisableButtons(false);

        }
        private void DisableButtons(bool v)
        {
            ButtonInstall.IsEnabled = v;
            ButtonMake.IsEnabled = v;
        }
        private void select(object sender, RoutedEventArgs e)
        {
            TextBoxOutputPath.Text = SelectSaveFolder();
        }

        private string SelectSaveFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                // Set description and initial folder (optional)
                folderDialog.Description = "Select a folder to save files";
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;  // Initial directory, you can set this

                // Show the dialog and check if user selected a folder

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return folderDialog.SelectedPath; // Return the folder path
                }
                else
                {
                    return null; // If user cancels, return null
                }
            }
        }
        private string GetFolder()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.FileName = "Select Folder"; // this just sets a placeholder name
            dialog.Title = "Select a Folder";

            if (dialog.ShowDialog() == true)
            {
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                TextBoxOutputPath.Text = folderPath;
                return folderPath;
            }
            return string.Empty;
        }

        private void SetMakerOutPutFile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Maker_OutPutFile.Text = selectSaveLocation();
        }

        private void SetMakerInputFolder(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Maker_InputFolder.Text = GetFolder();
        }
        private string selectSaveLocation()
        {
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Save File",
                Filter = "CZA Files (*.cza)|*.cza",
                DefaultExt = "cza",
                AddExtension = true,
                FileName = "myfile.cza"
            };

            var result = dialog.ShowDialog(); // returns DialogResult

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedPath = dialog.FileName;

                // Assuming TextBoxOutputPath is accessible here; otherwise remove this line
                TextBoxOutputPath.Text = selectedPath;

                return selectedPath;
            }

            return string.Empty;
        }


         
        private async void Make(object sender, RoutedEventArgs e)
        {
            DisableButtons(true);
            string folder = Maker_InputFolder.Text;
            string file = Maker_OutPutFile.Text;
            string title = Maker_InputTitle.Text;
            int year = GetInt(Maker_InputYear.Text);
            string fullPath = Path.Combine(folder,title);
            if (Directory.Exists(folder) == false) { MessageBox.Show("Folder doesnt exist"); return; }
            
            
            if (FolderAndSubfoldersAreEmpty(folder)) { MessageBox.Show("folder contains no files"); return; }
            if (title.Length == 0) { MessageBox.Show("Empty title"); return; }
            if (header.Source == null) { MessageBox.Show("No header image"); return; }
           // if (Data.Listfile.Count == 0) { MessageBox.Show("Empty listfile"); return; }
            Generating = true;
            await ArchiveManager.GenerateArchive(file, folder, title, year, header.Source, this);
            Generating = false;
            DisableButtons(false);
            if (CheckOpen.IsChecked == true) { OpenFolder(fullPath); }
        }

        private void OpenFolder(string target)
        {
            try
            {
                if (Directory.Exists(target))
                {
                    Process.Start("explorer.exe", target);
                }
                else
                {
                    MessageBox.Show($"The folder \"{target}\" does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private int GetInt(string t)
        {
            if (int.TryParse(t, out int result)) { return result; } else { return 0; }
        }
        public bool FolderAndSubfoldersAreEmpty(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return false; // or throw exception if you want to handle invalid paths

            // Check if any files exist in this folder or any subfolders
            string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            return allFiles.Length == 0;
        }

        private void SelectImage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (inMaker)
            {

                header.Source = SelectAndLoadImage();
            }
        }
        public ImageSource SelectAndLoadImage()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select an image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                // Use BitmapImage to load the image
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = null; // Clear Uri if using stream
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Makes it cross-thread accessible
                }

                return bitmap;
            }

            return null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Generating) { e.Cancel = true; }
            if (Extracting) { e.Cancel = true; }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (!Extracting && !Generating) { Environment.Exit(0); return; }

            }
            if (e.Key == System.Windows.Input.Key.F1)
            {
                reader r = new reader(Data.Listfile); r.ShowDialog();

            }
        }
        public class FileData
        {
            public string Title;
            public string Year;
            public List<string> Listfile = new List<string>();
            public ImageSource HeaderImage;
        }

        public static class ArchiveManager
        {
            public static async Task GenerateArchive(string outputFileName, string inputFolder, string archiveTitle, int year, ImageSource image, MainWindow windowToUpdate)
            {

                string extension = "cza";
                List<string> files = GetFilesFromFolder(inputFolder);
                List<string> listfile = GetListFileOfFolder(inputFolder);
                int fileCount = files.Count;
                float percentage = 0f;
                float step = 100f / fileCount;

                using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // Write Title and Year
                    writer.Write(archiveTitle);
                    writer.Write(year);

                    // Write Header Image
                    byte[] headerImageBytes = EncodeImageToPngBytes(image);
                    writer.Write(headerImageBytes.Length);
                    writer.Write(headerImageBytes);

                    // Write Listfile (with sizes for extraction)
                    writer.Write(fileCount);
                    foreach (string file in files)
                    {
                        writer.Write(file);
                        byte[] content = File.ReadAllBytes(file);
                        writer.Write(content.Length);
                    }

                    // Now write file data
                    for (int i = 0; i < fileCount; i++)
                    {
                        string file = files[i];
                        byte[] content = File.ReadAllBytes(file);
                        writer.Write(content);

                        // Update progress
                        percentage += step;
                        windowToUpdate.Title = $"Generated {percentage:F2}%";
                        await Task.Yield(); // Allows UI to update
                    }
                }

                windowToUpdate.Title = "Archive generated!";
            }

            private static List<string> GetFilesFromFolder(string inputFolder)
            {
                if (!Directory.Exists(inputFolder))
                    return new List<string>(); // or throw, depending on your use case

                return new List<string>(Directory.GetFiles(inputFolder, "*", SearchOption.AllDirectories));
            }


            private static byte[] EncodeImageToPngBytes(ImageSource image)
            {
                if (image is BitmapSource bitmapSource)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        return ms.ToArray();
                    }
                }

                throw new InvalidOperationException("Unsupported image format.");
            }

            public static FileData ReadArchiveInfo(string inputFile)
            {
                FileData result = new FileData();

                using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // Title and Year
                    result.Title = reader.ReadString();
                    result.Year = reader.ReadInt32().ToString();

                    // Header Image
                    int imageLength = reader.ReadInt32();
                    byte[] imageBytes = reader.ReadBytes(imageLength);
                    result.HeaderImage = LoadImageFromBytes(imageBytes);

                    // Listfile
                    int fileCount = reader.ReadInt32();
                    result.Listfile = new List<string>(fileCount);

                    for (int i = 0; i < fileCount; i++)
                    {
                        string path = reader.ReadString();
                        int size = reader.ReadInt32(); // only needed for extraction
                        result.Listfile.Add(path);
                    }
                }
                result.Listfile = ReduceListFile(result.Listfile);
                return result;
            }

            private static List<string> ReduceListFile(List<string> listfile)
            {
                if (listfile == null || listfile.Count == 0)
                    return new List<string>();

                string root = FindCommonRoot(listfile);
                var reduced = new List<string>();

                foreach (var file in listfile)
                {
                    string relative = file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);
                    reduced.Add(relative);
                }

                return reduced;
            }

            private static string FindCommonRoot(List<string> paths)
            {
                if (paths.Count == 1)
                    return Path.GetDirectoryName(paths[0]) + Path.DirectorySeparatorChar;

                string[] splitFirst = paths[0].Split(Path.DirectorySeparatorChar);
                int commonLength = splitFirst.Length;

                for (int i = 1; i < paths.Count; i++)
                {
                    string[] splitCurrent = paths[i].Split(Path.DirectorySeparatorChar);
                    commonLength = Math.Min(commonLength, splitCurrent.Length);

                    for (int j = 0; j < commonLength; j++)
                    {
                        if (!string.Equals(splitFirst[j], splitCurrent[j], StringComparison.OrdinalIgnoreCase))
                        {
                            commonLength = j;
                            break;
                        }
                    }
                }

                string commonRoot = string.Join(Path.DirectorySeparatorChar.ToString(), splitFirst, 0, commonLength);
                return commonRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) ? commonRoot : commonRoot + Path.DirectorySeparatorChar;
            }

            public static async Task Extract(string inputFile, string outputFolder, FileData archiveInfo, MainWindow window)
            {
                try
                {
                    // Create the directory in which we extract
                    string targetFolder = Path.Combine(outputFolder, archiveInfo.Title);
                    Directory.CreateDirectory(targetFolder);

                    int filesCount = archiveInfo.Listfile.Count;
                    float step = 100f / filesCount;
                    float percentage = 0f;

                    using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        // Skip title and year
                        reader.ReadString();
                        reader.ReadInt32();

                        // Skip header image
                        int imageLength = reader.ReadInt32();
                        reader.ReadBytes(imageLength);

                        // Skip file metadata but store the sizes
                        int fileCountFromFile = reader.ReadInt32();
                        var fileSizes = new List<int>(fileCountFromFile);

                        for (int i = 0; i < fileCountFromFile; i++)
                        {
                            reader.ReadString(); // skip path (we use archiveInfo instead)
                            int size = reader.ReadInt32();
                            fileSizes.Add(size);
                        }

                        // Extract file data using archiveInfo.Listfile
                        for (int i = 0; i < filesCount; i++)
                        {
                            string relativePath = archiveInfo.Listfile[i];
                            int dataLength = fileSizes[i];

                            // Safety check
                            long remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                            if (remaining < dataLength)
                                throw new InvalidDataException($"Data for file '{relativePath}' exceeds stream length.");

                            byte[] data = reader.ReadBytes(dataLength);

                            string outputPath = Path.Combine(targetFolder, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                            File.WriteAllBytes(outputPath, data);

                            percentage += step;
                            window.Title = $"Installed {percentage:F2}%";
                            await Task.Yield(); // Keep UI responsive
                        }
                    }

                    window.Title = "Archive extracted!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Extraction failed:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    window.Title = "Extraction failed";
                }
            }


            private static ImageSource LoadImageFromBytes(byte[] imageBytes)
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    var bitmap = decoder.Frames[0];
                    bitmap.Freeze(); // makes it cross-thread safe
                    return bitmap;
                }

            }

            private static byte[] ConvertImageSourceToPngBytes(ImageSource image)
            {
                if (image is BitmapSource bitmapSource)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        return stream.ToArray();
                    }
                }

                throw new InvalidOperationException("Unsupported image format.");
            }

            private static List<string> GetListFileOfFolder(string folderPath)
            {
                var allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                var relativePaths = allFiles
                    .Select(file => GetRelativePath(folderPath, file))
                    .ToList();

                return relativePaths;
            }
            private static string GetRelativePath(string basePath, string fullPath)
            {
                Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
                Uri fullUri = new Uri(fullPath);
                Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
                return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
            }

            private static string AppendDirectorySeparatorChar(string path)
            {
                if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    return path + Path.DirectorySeparatorChar;
                return path;
            }






        }
    }
}
