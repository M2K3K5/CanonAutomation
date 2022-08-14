using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Interop;
using System.ComponentModel;

namespace CanonAutomation
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

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

        public void Canon()
        {
            CopyFilesRecursively(@"F:\DCIM\100CANON", @"D:\Bilder R10\");
            Process.Start("explorer.exe", @"D:\Bilder R10");
            Process.Start(@"C:\Program Files\Adobe\Adobe DNG Converter\Adobe DNG Converter.exe");
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            int filesAll = 0;
            int filesCopied = 0;

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                filesAll++;
                FileInfo fi = new FileInfo(newPath);
                string targetPathSingle = newPath.Replace(sourcePath, targetPath + fi.Extension.Replace(".", "").ToUpper());
                if(!File.Exists(targetPathSingle)) { 
                    File.Copy(newPath, targetPathSingle, false);
                    filesCopied++;
                }
            }

            Trace.WriteLine("Copied new " + filesCopied.ToString() + " of " + filesAll.ToString() + " files");
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
                            Canon();
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
    }
}
