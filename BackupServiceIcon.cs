using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace BackupService
{
    internal sealed class BackupServiceIcon
    {
        private bool _showing = false;
        internal bool Showing
        {
            get
            {
                if (_disposed)
                {
                    throw new Exception("BackupServiceIcon has been disposed.");
                }
                return _showing;
            }
        }
        private bool _disposed = false;
        internal bool Disposed => _disposed;
        internal delegate void BackupServiceIconEvent();
        internal BackupServiceIconEvent OnClose = null;
        internal BackupServiceIconEvent OnBackupLater = null;
        internal BackupServiceIconEvent OnBackupNow = null;
        internal BackupServiceIconEvent OnConfig = null;
        private NotifyIcon notifyIcon = null;
        private ContextMenuStrip contextMenu = null;
        internal BackupServiceIcon()
        {

        }
        private void OnConfigEvent(object sender, EventArgs e)
        {
            OnConfig?.Invoke();
        }
        private void OnBackupNowEvent(object sender, EventArgs e)
        {
            OnBackupNow?.Invoke();
        }
        private void OnBackupLaterEvent(object sender, EventArgs e)
        {
            OnBackupLater?.Invoke();
        }
        private void OnCloseEvent(object sender, EventArgs e)
        {
            OnClose?.Invoke();
        }
        public void Show()
        {
            if (_disposed)
            {
                throw new Exception("BackupServiceIcon has been disposed.");
            }
            if (_showing)
            {
                throw new Exception("BackupServiceIcon is already showing.");
            }
            bool contin = false;
            Thread GUIThread = new Thread(() =>
            {
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = Icon.ExtractAssociatedIcon(typeof(BackupServiceIcon).Assembly.Location);

                contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Close", null, OnCloseEvent);
                contextMenu.Items.Add("Backup Later", null, OnBackupLaterEvent);
                contextMenu.Items.Add("Backup Now", null, OnBackupNowEvent);
                contextMenu.Items.Add("Config", null, OnConfigEvent);

                notifyIcon.ContextMenuStrip = contextMenu;

                notifyIcon.Visible = true;

                contin = true;

                while (true)
                {
                    Application.DoEvents();
                }
            });
            GUIThread.Start();
            while (!contin)
            {
                
            }
        }
        public void Dispose()
        {
            if (_disposed)
            {
                throw new Exception("BackupServiceIcon has already been disposed.");
            }
            _disposed = true;
            notifyIcon.Visible = false;
            contextMenu.Dispose();
            notifyIcon.Dispose();
        }
    }
}