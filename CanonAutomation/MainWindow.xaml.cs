using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Interop;
using System.ComponentModel;
using System.Collections.Generic;

namespace CanonAutomation
{

    public partial class MainWindow : Window
    {

        String pathExported = "";

        public MainWindow()
        {
            InitializeComponent();
            pathExported = inputPath.Text.ToString();

            var window = new Window();
            var handle = new WindowInteropHelper(window).EnsureHandle();

            HwndSource hwndSource = HwndSource.FromHwnd(handle);
            Hide();

            if (hwndSource != null)
            {
                IntPtr windowHandle = hwndSource.Handle;
                hwndSource.AddHook(UsbNotificationHandler);
                USBDetector.RegisterUsbDeviceNotification(windowHandle);
            }
        }

        public void CanonMain()
        {
            MoveFilesRecursively(@"F:\DCIM\100CANON", @"D:\Bilder R10\");
            Process.Start("explorer.exe", @"D:\Bilder R10");
            Process.Start(@"C:\Program Files\Adobe\Adobe DNG Converter\Adobe DNG Converter.exe");
        }

        private static void MoveFilesRecursively(string sourcePath, string targetPath)
        {
            int filesAll = 0;
            int filesMoved = 0;

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                filesAll++;
                FileInfo fi = new FileInfo(newPath);
                string targetPathSingle = newPath.Replace(sourcePath, targetPath + fi.Extension.Replace(".", "").ToUpper());
                if(!File.Exists(targetPathSingle)) { 
                    File.Move(newPath, targetPathSingle, false);
                    filesMoved++;
                }
            }

            Trace.WriteLine("Moved " + filesMoved.ToString() + " of " + filesAll.ToString() + " files");
        }

        private void ComparePhotos()
        {
            String sourcePath = pathExported.Remove(pathExported.LastIndexOf("/"), pathExported.Length - pathExported.LastIndexOf("/"));
            String pathExportedCompare = inputPath.Text.ToString() + "Compare";
            String pathJpg = sourcePath + "/JPG";
            int copiedFiles = 0;

            if(Directory.Exists(pathJpg))
            {
                Trace.WriteLine("dir /JPG exists");

                List<string> exportedFileNames = new List<string>();
                foreach (string fileName in Directory.GetFiles(pathExported, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if(copiedFiles == 0 && !Directory.Exists(pathExportedCompare)) Directory.CreateDirectory(pathExportedCompare);

                    FileInfo fi = new FileInfo(fileName);
                    exportedFileNames.Add(fi.Name.ToLower());
                }
                Trace.WriteLine($"found {exportedFileNames.Count} exported files");

                foreach (string file in Directory.GetFiles(pathJpg, "*.*", SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(file);
                    if(exportedFileNames.Contains(fi.Name.ToLower()))
                    {
                        String fileName = "/" + fi.Name;
                        if (File.Exists(pathExported + fileName) && !File.Exists(pathExportedCompare + fileName))
                        {
                            File.Copy(pathExported + fileName, pathExportedCompare + fileName.Replace(".JPG", ".jpg"), false);
                        }
                        else if (File.Exists(pathExported + fileName.Replace(".jpg", ".JPG")) && !File.Exists(pathExportedCompare + fileName))
                        {
                            File.Copy(pathExported + fileName.Replace(".jpg", ".JPG"), pathExportedCompare + fileName.Replace(".JPG", ".jpg"), false);
                        }

                        String pathOldFile = pathExportedCompare + fileName.Replace(".jpg", "_old.jpg");
                        if(fi.Name.EndsWith(".JPG")) pathOldFile = pathExportedCompare + fileName.Replace(".JPG", "_old.jpg");
                        if (!File.Exists(pathOldFile))
                        {
                            File.Copy(fi.FullName, pathOldFile, false);
                        }
                        copiedFiles++;
                    }
                }
                Trace.WriteLine($"copied {copiedFiles} files");
            }
            else
            {
                Trace.WriteLine("dir /JPG doesnt exists");
            }
        }

        private IntPtr UsbNotificationHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == USBDetector.UsbDevicechange)
            {
                switch ((int)wparam)
                {
                    case USBDetector.UsbDeviceRemoved:
                        Trace.WriteLine("USB Removed");
                        break;
                    case USBDetector.NewUsbDeviceConnected:
                        Trace.WriteLine("New USB Detected");

                        if(Directory.Exists(@"F:\DCIM\100CANON"))
                        {
                            Trace.WriteLine("Canon SD-Card Detected");
                            CanonMain();
                        }

                        break;
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        private void Mainform_Formclosing(object sender, CancelEventArgs e)
        {
            this.Hide();
            //NotifyIcon.Visible = true;
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void NotifyIcon_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Show();
            //notifyIcon.Visible = false;
        }

        private void MenuItem_Click_Quit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuItem_Click_Show(object sender, RoutedEventArgs e)
        {
            Show();
            ShowInTaskbar = true;
        }

        private void MenuItem_Click_Compare(object sender, RoutedEventArgs e)
        {
            ComparePhotos();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                dialog.Description = "Choose the path of your exported pictures";
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    pathExported = dialog.SelectedPath;
                    pathExported = pathExported.Replace("\\", "/");
                    inputPath.Text = pathExported;
                    Trace.WriteLine($"changed pathExported to {pathExported}");
                }
            }
        }
    }
}
