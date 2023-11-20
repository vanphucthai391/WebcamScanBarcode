using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebcamScanBarcode
{
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
        }
        private void frmRegister_Load(object sender, EventArgs e)
        {
        }
        private void btnCreateBarcode_Click(object sender, EventArgs e)
        {
            string text = txtName.Text+"," + txtSection.Text + "," + txtEmployeeCode.Text;
            QRCoder.QRCodeGenerator qg = new QRCoder.QRCodeGenerator();
            var myData = qg.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.H);
            var code = new QRCoder.QRCode(myData);
            pictureBox1.Image = code.GetGraphic(10);
            
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtEmployeeCode.Text = "";
            txtName.Text = "";
            txtSection.Text = "";
            pictureBox1.Image = null;
        }

    }
}
