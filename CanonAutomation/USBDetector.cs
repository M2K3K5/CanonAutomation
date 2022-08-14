using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CanonAutomation
{
    /// <summary>
    /// To get USBBroadcastinterface
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct USBBroadcastinterface
    {
        /// <summary>
        /// The size
        /// </summary>
        internal int Size;

        /// <summary>
        /// The device type
        /// </summary>
        internal int USBType;

        /// <summary>
        /// The reserved
        /// </summary>
        internal int Reserved;

        /// <summary>
        /// The class unique identifier
        /// </summary>
        internal Guid ClassGuid;

        /// <summary>
        /// The name
        /// </summary>
        internal short Name;
    }

    /// <summary>
    /// To get DeviceDiscoveryManager
    /// </summary>
    public class USBDetector
    {
        /// <summary>
        /// The new usb device connected
        /// </summary>
        public const int NewUsbDeviceConnected = 0x8000;

        /// <summary>
        /// The usb device removed
        /// </summary>
        public const int UsbDeviceRemoved = 0x8004;

        /// <summary>
        /// The usb devicechange
        /// </summary>
        public const int UsbDevicechange = 0x0219;

        /// <summary>
        /// The DBT devtyp deviceinterface
        /// </summary>
        private const int DbtDevtypDeviceinterface = 5;

        /// <summary>
        /// The unique identifier devinterface usb device
        /// </summary>
        private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices

        /// <summary>
        /// The notification handle
        /// </summary>
        private static IntPtr notificationHandle;

        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            USBBroadcastinterface dbi = new USBBroadcastinterface
            {
                USBType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidDevinterfaceUSBDevice,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        /// <summary>
        /// Registers the device notification.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="notificationFilter">The notification filter.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>returns IntPtr</returns>
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        /// <summary>
        /// Unregisters the device notification.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>returns bool</returns>
        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);
    }
}