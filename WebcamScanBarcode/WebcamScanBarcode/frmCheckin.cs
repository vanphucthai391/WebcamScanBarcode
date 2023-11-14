using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace WebcamScanBarcode
{
    public partial class frmCheckin : Form
    {
        public delegate void RefreshEventHandler(object sender, EventArgs e);
        public event RefreshEventHandler RefreshEvent;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private string serverFtpin = @"\\192.168.145.7\ftpin\BGL_0246";
        private bool dataSent = false;

        public frmCheckin()
        {
            InitializeComponent();
        }
        string foldermonth;
        /*COM c1 = new COM();*/
        private void Form1_Load(object sender, EventArgs e)
        {
            /*c1.initializePort();
            lbCOM.Text=c1.getNamePort();*/
            string datalocal = @"C:\BGL0246_CHECKIN";
            if (!Directory.Exists(datalocal))
                Directory.CreateDirectory(datalocal);
            string folderyear = datalocal + @"\"  + DateTime.Now.ToString("yyyy");
            if (!Directory.Exists(folderyear))
                Directory.CreateDirectory(folderyear);
            foldermonth = folderyear + @"\" + DateTime.Now.ToString("MM");
            if (!Directory.Exists(foldermonth))
                Directory.CreateDirectory(foldermonth);

            btnStop.Enabled = false;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("webcam not found!");
                return;
            }
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)menuStrip1.Items["registerToolStripMenuItem"];
            toolStripMenuItem.Visible = false;
            
        }
        private async void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                BarcodeReader reader = new BarcodeReader();
                var result = reader.Decode(bitmap);

                if (result != null&&!dataSent)
                {
                    dataSent = true;
                    string decodedText = result.Text;
                    //string[] decodedTextArray = decodedText.Split(';');
                    if(decodedText.StartsWith("##"))
                    {
                        string timeCheck = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss tt");//don't change format here
                        Invoke((MethodInvoker)async delegate
                        {
                            string decodedText_remove2First= decodedText.Substring(2);
                            lbName.Text = decodedText_remove2First.Substring(0, decodedText_remove2First.Length - 7);
                            string decodedText_remove5Last = decodedText.Substring(2);
                            string _7last = decodedText.Substring(decodedText.Length - 7, 7);//7 last
                            lbEmp.Text = _7last.Substring(_7last.Length - 5, 5);//5 last
                            lbSection.Text = _7last.Substring(0, 2);//2 first
                            lbTime.Text = timeCheck;
                            await Task.Delay(1000); // Delay for 1 seconds before stopping the camera (adjust as needed)
                            stopCamera();
                            btnStop.Enabled = false;
                            btnStart.Enabled = true;
                        });
                        await Task.Run(() => {
                            authenticationWithMasterList(lbEmp.Text, lbName.Text, lbSection.Text, lbTime.Text);
                        });
                    }
                    else
                    {
                        return;
                    }
                }
                pictureBox1.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            dataSent = false;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            resetInfo();
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
            videoSource.Start();        
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            stopCamera();

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopCamera();
            /*c1.closeSerialPort();*/
        }
        private void stopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
                pictureBox1.Image = null;
                videoSource = null;

            }
        }
        private void resetInfo()
        {
            lbEmp.Text = "";
            lbName.Text = "";
            lbSection.Text = "";
            lbTime.Text = "";
        }
        private void pushDataToPqm(string empNo, string nameOrg, string section, string timeOrg,string judge)
        {
            string name = Regex.Replace(nameOrg, @"\s", "");
            string model = "BGL_0246";
            string site = "NCVP";
            string factory = "2B";
            string line = "1";
            string process = "GATE";
            string inspect = "CHECKIN";
            string[] datetime = timeOrg.Split(' ');
            string date = datetime[0];
            string time = datetime[1];
            string nameFile = "BGL0246_" + DateTime.Now.ToString("yyyyMMddHHmmssfff")+".csv";
            string outFile = serverFtpin+"/"+ nameFile;
            try
            {
                System.IO.File.AppendAllText(outFile, name + "," + section + "_" + empNo + "," + model + "," + site + "," + factory + "," + line + "," + process + "," + inspect + "," + date + "," + time + "," +judge+ ","+ judge +",,\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void saveAtLocal( string empNo, string nameOrg, string section, string timeOrg,string judge)
        {
            try
            {
                string nameFile = "BGL0246_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                string outFile = foldermonth + "/" + nameFile;
                System.IO.File.AppendAllText(outFile, timeOrg + "," + nameOrg + "," + section + "_" + empNo+","+ judge + "\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmReport rp1 = new frmReport();
            rp1.Show();
        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegister re = new frmRegister();
            re.Show();
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loginToolStripMenuItem.Text == "Login")
            {
                frmLogin lg = new frmLogin();
                lg.RefreshEvent += delegate (object sndr, EventArgs excp)
                {
                    showRegisterToolStripMenuItem(true);
                    loginToolStripMenuItem.Text = "Logout";
                    this.Focus();
                };
                lg.Show();
            }
            else
            {
                showRegisterToolStripMenuItem(false);
                loginToolStripMenuItem.Text = "Login";
            }
        }
        private void showRegisterToolStripMenuItem(bool flag)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)menuStrip1.Items["registerToolStripMenuItem"];
            toolStripMenuItem.Visible = flag;
        }
        private async void authenticationWithMasterList(string empNo, string nameOrg, string section, string timeOrg)
        {
            string sernoQR = Regex.Replace(nameOrg, @"\s", "");
            string lotQR = section + "_" + empNo;
            string sql = "select * from bgl_0246_usermaster where serno='" + sernoQR + "' and lot='" + lotQR + "' and allow='t'";
            DataTable dt = new DataTable();
            TfSQL tf = new TfSQL();
            tf.sqlDataAdapterFillDatatableFromTesterDb(sql, ref dt);
            if (dt.Rows.Count < 1)
            {
                Invoke((MethodInvoker)delegate
               {
                   lbMessage.Text = "You are not in list to enter this room!";
                   lbMessage.ForeColor = Color.Red;
               });
                await Task.Run(() => {
                    pushDataToPqm(empNo, nameOrg, section, timeOrg, "1");
                    saveAtLocal(empNo, nameOrg, section, timeOrg, "1");
                });
                /*c1.sendCmdToArduino("1");*/
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    lbMessage.Text = "WELCOME. Please get in!";
                    lbMessage.ForeColor = Color.Green;
                });
                await Task.Run(() => {
                    pushDataToPqm(empNo, nameOrg, section, timeOrg, "0");
                    saveAtLocal(empNo, nameOrg, section, timeOrg, "0");
                });
                /*c1.sendCmdToArduino("0");*/
            }
        }
    }
}
