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
    public partial class Form1 : Form
    {
        public delegate void RefreshEventHandler(object sender, EventArgs e);
        public event RefreshEventHandler RefreshEvent;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private string serverFtpin = @"\\192.168.145.7\ftpin\BGL_0246";
        private bool dataSent = false;

        public Form1()
        {
            InitializeComponent();
        }
        string foldermonth;
        private void Form1_Load(object sender, EventArgs e)
        {
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
                            pushDataToPqm(lbEmp.Text, lbName.Text, lbSection.Text, lbTime.Text);
                            saveAtLocal(lbEmp.Text, lbName.Text, lbSection.Text, lbTime.Text);
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
        private void pushDataToPqm(string empNo, string nameOrg, string section, string timeOrg)
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
                System.IO.File.AppendAllText(outFile, name + "," + section + "_" + empNo + "," + model + "," + site + "," + factory + "," + line + "," + process + "," + inspect + "," + date + "," + time + ",0,0,,\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void saveAtLocal( string empNo, string nameOrg, string section, string timeOrg)
        {
            try
            {
                string nameFile = "BGL0246_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                string outFile = foldermonth + "/" + nameFile;
                System.IO.File.AppendAllText(outFile, timeOrg + "," + nameOrg + "," + section + "_" + empNo + "\r\n");
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
        private void authenticationWithMasterList()
        {

        }
    }
}
