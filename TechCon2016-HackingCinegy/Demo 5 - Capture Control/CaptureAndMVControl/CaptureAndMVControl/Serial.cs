using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace CaptureAndMVControl
{
    public partial class ControlPanelMainForm : Form
    {
        static SerialPort _serialPort;

        private void InitSerial()
        {
            _serialPort = new SerialPort("COM3", 9600);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                _serialPort = null;
            }
        }

        void SendToSerial(string cmd)
        {
            if (_serialPort != null)
                _serialPort.Write(cmd);
        }

        void Serial_StartRecord()
        {
            SendToSerial("CAPREC@");
        }

        void Serial_StopRecord()
        {
            SendToSerial("CAPSTOP@");
        }

        void Serial_Clear()
        {
            SendToSerial("CAPCLR@");
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int dataLength = _serialPort.BytesToRead;
            char[] data = new char[dataLength];
            int nbrDataRead = _serialPort.Read(data, 0, dataLength);
            if (nbrDataRead == 0)
                return;

            string Msg = new string(data);
            Msg = Msg.TrimEnd('\r', '\n');
            OnSerialData(Msg);
        }
    }
}
