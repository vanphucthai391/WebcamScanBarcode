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
    public partial class frmLogin : Form
    {
        public delegate void RefreshEventHandler(object sender, EventArgs e);
        public event RefreshEventHandler RefreshEvent;
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            authenticatonWithUsernameAndPassword();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                authenticatonWithUsernameAndPassword();
            }
        }
        private void authenticatonWithUsernameAndPassword()
        {
            if (txtUsername.Text == "Administrator" && txtPassword.Text == "12345678")
            {
                MessageBox.Show("Successed", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.RefreshEvent(this, new EventArgs());
                Close();
            }
            else
            {
                MessageBox.Show("Login Failed", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
