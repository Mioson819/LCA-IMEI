using LCA_Project.Form.Teaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNow;
            using (var mutex = new System.Threading.Mutex(true, "LCA_Project_Unique_Mutex_Name", out createdNow))
            {
                if (!createdNow)
                {
                    MessageBox.Show("Phần Mềm Đã Được Mở Rồi", "Information", MessageBoxButtons.OK, MessageBoxIcon.Question);
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMaincs());
            }
        }
    }
}
