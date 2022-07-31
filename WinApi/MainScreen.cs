using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinApi
{
    public partial class MainScreen : Form
    {
        private Form activeForm = null;
        public MainScreen()
        {
            InitializeComponent();
        }

        private void openChildForm(Form childForm)
        {
            if (activeForm != null)
            {
                activeForm.Close();
            }

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel.Controls.Add(childForm);
            panel.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            openChildForm(new Form1());           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ScanFiles sf = new ScanFiles();
            //sf.Show();
            openChildForm(new ScanFiles());
        }

     
        private void MainScreen_Load(object sender, EventArgs e)
        {

        }

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void checkVoilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChildForm(new Form1());
        }

        private void scanLocalFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChildForm(new ScanFiles());
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logDirectoryPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log";
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }
            Process.Start(logDirectoryPath);
        }
    }
}
