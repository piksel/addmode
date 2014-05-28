using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using System.Globalization;

namespace ADDMode
{
    public partial class FormADDMode : Form
    {

#region Imports
        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
        public struct DISPLAY_DEVICE 
        {
              [MarshalAs(UnmanagedType.U4)]
              public int cb;
              [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
              public string DeviceName;
              [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
              public string DeviceString;
              [MarshalAs(UnmanagedType.U4)]
              public DisplayDeviceStateFlags StateFlags;
              [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
              public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
              public string DeviceKey;
        }

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // P/Invoke constants
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);


        // ID for the About item on the system menu
        private int SYSMENU_CLICKTHROUGH_ID = 0x1;
        private int initialStyle = -1;
        private CultureInfo ciEnUs;


#endregion

        public FormADDMode()
        {
            InitializeComponent();
        }

        private void FormADDMode_Load(object sender, EventArgs e)
        {
            for(int i=0; i < Screen.AllScreens.Length; i++)
            {
                var s = Screen.AllScreens[i];
                var dspName = GetDisplayName(s.DeviceName);
                Debug.WriteLine(dspName);
                var tsmi = new ToolStripMenuItem(string.Format("#{0} {1} ({2}x{3})", (i + 1), dspName, s.Bounds.Width, s.Bounds.Height));
                tsmi.Tag = i;
                tsmi.Click += new EventHandler(tsmiScreenX_clicked);
                tsmiMoveToScreen.DropDownItems.Add(tsmi);
            }
            MoveToScreen(Screen.PrimaryScreen);
            TopMost = true;
            ciEnUs = CultureInfo.GetCultureInfo("en-us");
        }

        void tsmiScreenX_clicked(object sender, EventArgs e)
        {
            int sid = (int) ((ToolStripMenuItem)sender).Tag;
            if(Screen.AllScreens.Length >= sid)
                MoveToScreen(Screen.AllScreens[sid]);
        }

        private object GetDisplayName(string p)
        {
            var device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);
            try
            {
                for (uint id = 0; EnumDisplayDevices(null, id, ref device, 0); id++)
                { 
                    device.cb = Marshal.SizeOf(device);

                    if (device.DeviceName == p)
                    {
                        string card = device.DeviceString;

                        EnumDisplayDevices(device.DeviceName, 0, ref device, 0);

                        string monitor = device.DeviceString;

                        return String.Format("{0} on {1}", monitor, card);
                    }

                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                Debug.Print("{0}", ex.ToString());
                return "Unknown";
            }
        }

        private void MoveToScreen(Screen screen)
        {
            Bounds = screen.Bounds;

            int sid = -1;
            for(int i=0; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i] == screen)
                {
                    sid = i;
                }
            }
            foreach (ToolStripMenuItem tsmi in tsmiMoveToScreen.DropDownItems)
            {
                tsmi.Checked = ((int)tsmi.Tag == sid);
            }
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = alwaysOnTopToolStripMenuItem.Checked;
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var tsmiClicked = (ToolStripMenuItem)sender;
            double alpha = double.Parse(tsmiClicked.Tag.ToString(), ciEnUs);
            Opacity = alpha;

            foreach (ToolStripItem tsi in tsmiAlpha.DropDownItems)
            {
                if(tsi.GetType() != typeof(ToolStripMenuItem)) continue;
                ToolStripMenuItem tsmi = (ToolStripMenuItem)tsi;
                tsmi.Checked = (tsmi == tsmiClicked);
            }
        }

        private void clickThroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (initialStyle == -1)
            {
                initialStyle = GetWindowLong(this.Handle, -20);
                SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            }
            else
            {
                SetWindowLong(this.Handle, -20, initialStyle);
                initialStyle = -1;
            }
            clickThroughToolStripMenuItem.Checked = (initialStyle != -1);
        }

        private void FormADDMode_Leave(object sender, EventArgs e)
        {
            //BringToFront();
        }

        private void FormADDMode_Deactivate(object sender, EventArgs e)
        {
            //BringToFront();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // get handle to window's system menu
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

            // separator
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, "");

            // clickthrough menu item
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_CLICKTHROUGH_ID, "&Disable clickthrough");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_CLICKTHROUGH_ID))
            {
                if (initialStyle != -1)
                {
                    SetWindowLong(this.Handle, -20, initialStyle);
                    initialStyle = -1;
                }
            }

        }
    }
}
