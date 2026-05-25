using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LCA_Project.Form.Devices.Cameras
{
    public partial class frmTeachingCamera : System.Windows.Forms.Form
    {
        private CameraAS cameraAS;
        public frmTeachingCamera()
        {
            InitializeComponent();
        }
        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            if (float.TryParse(txtA.Text.ToString().Trim(), out float a) &&
                float.TryParse(txtB.Text.ToString().Trim(), out float b) &&
                float.TryParse(txtC.Text.ToString().Trim(), out float c) &&
                float.TryParse(txtX.Text.ToString().Trim(), out float x) &&
                float.TryParse(txtY.Text.ToString().Trim(), out float y) &&
                float.TryParse(txtZ.Text.ToString().Trim(), out float z)&& cameraAS!=null)
            {
                cameraAS.StepCalib(a, b, c, x, y, z);
            }
            else
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông số hiệu chuẩn và đảm bảo chúng là số hợp lệ hoặc tiến hành kết nối lại ");
            }
        }
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            cameraAS.StartCalib();
        }
        private void frmTeachingCamera_Load(object sender, EventArgs e)
        {
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.ToString() != "" && textBox1.Text.ToString() != null)
            {
                Task.Run(() =>
                {
                    cameraAS = new CameraAS(textBox1.Text.ToString().Trim(), 7890);
                });
                MessageBox.Show("Kết nối thành công");
            }
            else
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP");
            }
        }
        private void frmTeachingCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(cameraAS != null)
            {
                cameraAS.Dispose();
            }
        }
    }
}
