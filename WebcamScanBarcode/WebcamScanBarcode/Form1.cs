using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thiết bị webcam.");
                return;
            }
        }
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                BarcodeReader reader = new BarcodeReader();
                var result = reader.Decode(bitmap);

                if (result != null)
                {
                    string decodedText = result.Text;
                    string[] decodedTextArray = decodedText.Split(';');
                    if(decodedTextArray.Length!=3)
                    {
                        return;
                    }
                    else
                    {
                        txtMessage.Invoke(new MethodInvoker(delegate ()
                        {
                            txtMessage.Text = decodedText;
                        }));
                        lbEmp.Invoke(new MethodInvoker(delegate ()
                        {
                            lbEmp.Text = decodedTextArray[0];
                        }));
                        lbName.Invoke(new MethodInvoker(delegate ()
                        {
                            lbName.Text = decodedTextArray[1];
                        }));
                        lbSection.Invoke(new MethodInvoker(delegate ()
                        {
                            lbSection.Text = decodedTextArray[2];
                        }));
                        stopCamera();
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
            }
        }
        private void resetInfo()
        {
            txtMessage.Clear();
            lbEmp.Text = "";
            lbName.Text = "";
            lbSection.Text = "";
        }
    }
}
