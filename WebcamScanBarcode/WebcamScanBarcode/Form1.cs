using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private string serverFtpin = @"\\192.168.145.7\ftpin\TTTT";
        private bool dataSent = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thiết bị webcam.");
                return;
            }
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
                    string[] decodedTextArray = decodedText.Split(';');
                    if(decodedTextArray.Length!=3)
                    {
                        return;
                    }
                    else
                    {
                        string timeCheck= DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
                        Invoke((MethodInvoker)async delegate
                        {
                            lbEmp.Text = decodedTextArray[0];
                            lbName.Text = decodedTextArray[1];
                            lbSection.Text = decodedTextArray[2];
                            lbTime.Text = timeCheck;
                            await Task.Delay(2000); // Delay for 2 seconds before stopping the camera (adjust as needed)
                            stopCamera();
                            btnStop.Enabled = false;
                            btnStart.Enabled = true;
                        });
                        pushDataToPqm(lbEmp.Text, lbName.Text, lbSection.Text, lbTime.Text);
                    }
                }
                pictureBox1.Image = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
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
                System.IO.File.AppendAllText(outFile, name + "," + section+"_"+ empNo + "," + model + "," + site + "," + factory + "," + line + "," + process + "," + inspect + "," + date + "," + time + ", 0,0,,\r\n");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
