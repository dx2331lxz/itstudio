using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Itstudiowindow
{
    public partial class 远程应用管理 : Form
    {
        private NotifyIcon notifyIcon;

        public 远程应用管理()
        {
            InitializeComponent();
            InitializeNotifyIcon();
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            var screen = Screen.FromPoint(new Point(Cursor.Position.X, Cursor.Position.Y));
            var x = screen.WorkingArea.X + screen.WorkingArea.Width - this.Width;
            var y = screen.WorkingArea.Y + screen.WorkingArea.Height - this.Height;
            this.Location = new Point(x, y);
        }

            //this.Visible = false;
            // 开一个异步线程确保只有一个rdp2tcp.exe一直运行


        protected override void OnLoad(EventArgs e)
        {
            this.Width = 1;
            this.Height = 1;
            base.OnLoad(e);
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.logo;
            notifyIcon.Visible = true;

            // 添加一个右键菜单项
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItemExit = new MenuItem("Exit");
            menuItemExit.Click += MenuItemExit_Click;
            contextMenu.MenuItems.Add(menuItemExit);
            notifyIcon.ContextMenu = contextMenu;

        }
        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            // 当用户点击退出菜单项时，退出应用程序
            Close();
        }

    }
}