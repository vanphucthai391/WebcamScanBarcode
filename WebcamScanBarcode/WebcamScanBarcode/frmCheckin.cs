using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        private string serverFtpin = @"\\192.168.145.7\ftpin\BGP_0372";
        private bool flagFrame = false;//take 1 Frame
        private bool flagInternet = false;//take 1 Frame
        bool sound;

        public frmCheckin()
        {
            InitializeComponent();
        }
        const string dataLocalFolder = @"C:\BGP0372_CHECKIN";
        COM c1 = new COM();
        private void Form1_Load(object sender, EventArgs e)
        {
            c1.initializePort();
            lbCOM.Text=c1.getNamePort();
            isInternetConnected();
            if (!Directory.Exists(dataLocalFolder))
                Directory.CreateDirectory(dataLocalFolder);
            if (!Directory.Exists(dataLocalFolder + @"\data"))
                Directory.CreateDirectory(dataLocalFolder + @"\data");
            if (!Directory.Exists(dataLocalFolder + @"\err"))
                Directory.CreateDirectory(dataLocalFolder + @"\err");
            if (!Directory.Exists(dataLocalFolder + @"\bk"))
                Directory.CreateDirectory(dataLocalFolder + @"\bk");
            if (flagInternet)
                pushDataToPqm();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("webcam not found!");
                return;
            }
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)menuStrip1.Items["registerToolStripMenuItem"];
            toolStripMenuItem.Visible = false;
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
            videoSource.Start();
        }
        private void isInternetConnected()
        {
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send("192.168.145.7", 3000);
                if (reply != null && reply.Status == IPStatus.Success)
                {
                    flagInternet = true;
                }
                else
                {
                    flagInternet = false;
                    lbInternet.Text = "No Internet ";
                    lbInternet.ForeColor = Color.Red;
                }
            }
            catch (PingException)
            {

            } 
        }
        private async void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                BarcodeReader reader = new BarcodeReader();
                var result = reader.Decode(bitmap);

                if (result != null&&!flagFrame)
                {
                    flagFrame = true;
                    string decodedText = result.Text;
                    string [] decodedTextArr = decodedText.Split(',');
                    if (decodedTextArr.Length==3)
                    {
                        string timeCheck = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss tt");//don't change format here
                        await Task.Run(() => {
                            Invoke((MethodInvoker)async delegate
                            {
                                soundAlarm();
                                lbTime.Text = timeCheck;
                                lbName.Text = decodedTextArr[0];
                                lbSection.Text= decodedTextArr[1];
                                lbEmp.Text= decodedTextArr[2];
                                authenticationWithMasterList(lbEmp.Text, lbName.Text, lbSection.Text, lbTime.Text);
                                await Task.Delay(1000); // Delay for 1 seconds before stopping the camera (adjust as needed)
                                //string ImagePath2 = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\JigQuickDesk\JigQuickApp\images\STANDBY.bmp";
                                //pictureJudge.BackgroundImageLayout = ImageLayout.Zoom;
                                //pictureJudge.BackgroundImage = System.Drawing.Image.FromFile(ImagePath2);
                                flagFrame = false;
                            });

                        });
                    }
                    else
                    {
                        flagFrame = false;
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
        private void newPerson()
        {
            flagFrame = false;
            //resetInfo();

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopCamera();
            c1.closeSerialPort();
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
            txtMessage.Text = "";
        }
        private void saveAtLocal( string empNo, string nameOrg, string section, string timeOrg,string judge)
        {
            string name = Regex.Replace(nameOrg, @"\s", "");
            string model = "BGP_0372";
            string site = "NCVP";
            string factory = "2B";
            string line = "1";
            string process = "GATE";
            string inspect = "CHECKIN";
            string inspectData;
            if (flagInternet)
            {
                inspectData = judge;
            }
            else
            {
                inspectData = "2";
            }
            string[] datetime = timeOrg.Split(' ');
            string date = datetime[0];
            string time = datetime[1];
            string nameFile = "BGP0372_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".csv";
            string outFile = dataLocalFolder + @"\data\" + nameFile;
            try
            {
                System.IO.File.AppendAllText(outFile, name + "," + section + "_" + empNo + "," + model + "," + site + "," + factory + "," + line + "," + process + "," + inspect + "," + date + "," + time + "," + inspectData + "," + judge + ",,\r\n");
            }
            catch (Exception ex)
            {
                writeErrorList(ex);
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
        private async void authenticationWithMasterList(string empNo, string nameOrg, string sectionOrg, string timeOrg)
        {
            string sql = "select name, emp_no, section from bgp_0372_usermaster where name='" + nameOrg + "' and emp_no='"+ empNo + "' and section='" + sectionOrg + "' and allow='t'";
            DataTable dt = new DataTable();
            TfSQL tf = new TfSQL();
            tf.sqlDataAdapterFillDatatableFromTesterDb(sql, ref dt);
            flagInternet = tf.getStatusSQLConnection();
            if (dt.Rows.Count < 1 && flagInternet)
            {
                Invoke((MethodInvoker)delegate
                {
                    txtMessage.Text = "You are not in list to enter this room. Please contact FA for support!";
                    string ImagePath2 = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\JigQuickDesk\JigQuickApp\images\NG_BEAR.png";
                    pictureJudge.BackgroundImageLayout = ImageLayout.Zoom;
                    pictureJudge.BackgroundImage = System.Drawing.Image.FromFile(ImagePath2);
                    txtMessage.ForeColor = Color.Red;
                });
                await Task.Run(() => {
                    saveAtLocal(empNo, nameOrg, sectionOrg, timeOrg, "1");
                    pushDataToPqm();
                    c1.sendCmdToArduino("1");
                });
            }
            else if (dt.Rows.Count >= 1 && flagInternet)
            {
                //byte[] imageUser = tf.getImageUser(nameOrg, sectionOrg, empNo);
                Invoke((MethodInvoker)delegate
                {
                    //txtMessage.Text = "Please get in!";
                    string ImagePath2 = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\JigQuickDesk\JigQuickApp\images\OK_BEAR.png";
                    pictureJudge.BackgroundImageLayout = ImageLayout.Zoom;
                    pictureJudge.BackgroundImage = System.Drawing.Image.FromFile(ImagePath2);
                    txtMessage.ForeColor = Color.Green;
                    //convertByteArrayToImage(imageUser, ref pictureBoxUser);
                });
                await Task.Run(() => {
                    saveAtLocal(empNo, nameOrg, sectionOrg, timeOrg, "0");
                    pushDataToPqm();
                    c1.sendCmdToArduino("0");
                });
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    txtMessage.Text = "Your information has been saved!";
                    txtMessage.ForeColor = Color.Red;
                    c1.sendCmdToArduino("1");
                });
                await Task.Run(() => {
                    saveAtLocal(empNo, nameOrg, sectionOrg, timeOrg, "1");
                });
            }

        }
        private void pushDataToPqm()
        {
            string[] filePathArr = Directory.GetFiles(dataLocalFolder + @"\data", "*.csv");
            if (filePathArr.Length <= 0) return;
            try
            {
                foreach (string path in filePathArr)
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string outFile = serverFtpin + @"\" + fileName + ".csv";
                    string outFileBk = dataLocalFolder + @"\bk\" + fileName + ".csv";
                    string[] lineArr = System.IO.File.ReadAllLines(path);
                    for (int i = 0; i < lineArr.Length; i++)
                    {
                        System.IO.File.AppendAllText(outFile, lineArr[i] + "\r\n");
                        System.IO.File.AppendAllText(outFileBk, lineArr[i] + "\r\n");
                    }
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                writeErrorList(ex);
            }
        }
        private void writeErrorList(Exception ex1)
        {
            string outFileErr = dataLocalFolder + @"\err\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string inforError = $"{DateTime.Now}: {ex1.Message}";
            System.IO.File.AppendAllText(outFileErr, inforError + "\r\n");
        }
        private string aliasName = "MediaFile";
        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int mciSendString(String command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
        private void soundAlarm()
        {
            string currentDir = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\JigQuickDesk\JigQuickApp\images";
            string fileName = currentDir + @"\beep.mp3";
            string cmd;

            if (sound)
            {
                cmd = "stop " + aliasName;
                mciSendString(cmd, null, 0, IntPtr.Zero);
                cmd = "close " + aliasName;
                mciSendString(cmd, null, 0, IntPtr.Zero);
                sound = false;
            }

            cmd = "open \"" + fileName + "\" type mpegvideo alias " + aliasName;
            if (mciSendString(cmd, null, 0, IntPtr.Zero) != 0) return;
            cmd = "play " + aliasName;
            mciSendString(cmd, null, 0, IntPtr.Zero);
            sound = true;
        }
        //private void convertByteArrayToImage(byte[] byteArray, ref PictureBox picturBox1)
        //{
        //    if (byteArray == null || byteArray.Length == 0)
        //        return;
        //    using (MemoryStream ms = new MemoryStream(byteArray))
        //    {
        //        picturBox1.Image= Image.FromStream(ms);
        //    }
        //}
    }
}
