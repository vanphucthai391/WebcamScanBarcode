using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebcamScanBarcode
{
    class COM
    {
        private SerialPort serialPort1;
        private string selectPort;
        public void initializePort()
        {
            serialPort1 = new SerialPort();
            string[] portList = SerialPort.GetPortNames();
            selectPort = portList[0];
            if (serialPort1.IsOpen) return;
            serialPort1.PortName = selectPort;
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Encoding = Encoding.ASCII;
            try
            {
                serialPort1.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void sendCmdToArduino( string command)
        {
            string rtn = "";
            if (serialPort1.IsOpen == false) return;
            try
            {
                string cmd = command + rtn;
                serialPort1.Write(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void closeSerialPort()
        {
            if (serialPort1.IsOpen) serialPort1.Close();
        }
        public string getNamePort()
        {
            return selectPort;
        }
    }
}
