using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace toks1
{
    public partial class COMports : Form
    {
        const int PACKAGE_LENGTH = 72;

        private int attemptCounter = 0;

        private bool selectionFlag = true;
        private SerialPort serialPort = new SerialPort();

        private bool makeBusyorCollision()
        {
            Random random = new Random();
            return (random.Next(0, 100) < 30);
        }
        
        private void createEncoding(string inputStr)
        {
            attemptCounter = 0;
            byte[] bytes = Encoding.ASCII.GetBytes(inputStr);
            string newStr = stringToBinary(bytes);
            int remains = newStr.Length % PACKAGE_LENGTH;
            if (remains != 0)
            {
                for (int j = 0; j < PACKAGE_LENGTH - remains; j++)
                {
                    newStr += "0";
                }
            }
            int i = 0;
            int counter = 0;
            while (i < newStr.Length)
            {
                counter++;
                if (counter == PACKAGE_LENGTH)
                {
                    counter = 0;
                    string tempStr = newStr.Substring(i - PACKAGE_LENGTH + 1, PACKAGE_LENGTH);
                    debugBox.Text += tempStr + " - ";
                    byte[] byteStr = Encoding.ASCII.GetBytes(tempStr);
                    while (makeBusyorCollision())
                    {
                        //waiting
                    }
                    if (writeToPort(byteStr))
                    {
                        debugBox.Text += Environment.NewLine + "Text successfully sent!" + Environment.NewLine;
                    }
                    else
                    {
                        debugBox.Text += "Port write error!" + Environment.NewLine;
                    }
                }
                i++;
   
            }
        }

        private string binaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();
            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }
            return Encoding.ASCII.GetString(byteList.ToArray());
        }
        private string stringToBinary(byte[] data)
        {
            string result = string.Empty;
            foreach (byte value in data)
            {
                string binarybyte = Convert.ToString(value, 2);
                while (binarybyte.Length < 8)
                {
                    binarybyte = "0" + binarybyte;
                }
                result += binarybyte;
            }
            return result;
        }
        public COMports()
        {
            InitializeComponent();
            initComboBox();
        }
        public bool checkPort(String portName)
        {
            try
            {
                serialPort.PortName = portName;
                serialPort.BaudRate = 9600; 
                serialPort.DataBits = 8;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.Open();
                serialPort.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            String str = Encoding.UTF8.GetString(data);
            outputBox.Text += binaryToString(str);
        }
        public bool writeToPort(byte[] data)
        {
            try
            {
                Thread.Sleep(2000); //collision window = 2s
                if (makeBusyorCollision())
                {
                    debugBox.Text += "#";
                    attemptCounter++;
                    if (attemptCounter > 10)
                    {
                        debugBox.Text += "Attempts limit exceeded!" + Environment.NewLine;
                        return false;
                    }
                    int k = Math.Min(attemptCounter, 10);
                    Random random = new Random();
                    int r = random.Next(0, (int)Math.Pow(2, k));
                    Thread.Sleep(r * 1000); //backoff
                    while (makeBusyorCollision())
                    {
                        //waiting
                    }
                    writeToPort(data);
                }
                else
                {
                    serialPort.RtsEnable = true;
                    serialPort.Write(data, 0, data.Length);
                    serialPort.RtsEnable = false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public void initComboBox()
        {
            comboBoxPort.Items.Add("COM1");
            comboBoxPort.Items.Add("COM2");
            comboBoxPort.SelectedIndex = 0;
        }
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (selectionFlag)
            {
                debugBox.Text += "Please select a port!" + Environment.NewLine;
            }
            else
            {
                createEncoding(inputBox.Text);     
            }
        }
        private void buttonClearInput_Click(object sender, EventArgs e)
        {
            inputBox.Clear();
        }
        private void buttonClearOutput_Click(object sender, EventArgs e)
        {
            outputBox.Clear();
        }
        private void buttonSelect_Click(object sender, EventArgs e)
        {
            if (selectionFlag)
            {
                if (checkPort((String)comboBoxPort.SelectedItem))
                {
                    debugBox.Text += (String)comboBoxPort.SelectedItem + " port successfully opened!" + Environment.NewLine;
                    selectionFlag = false;
                }
                else
                {
                    debugBox.Text += (String)comboBoxPort.SelectedItem + " port is not available, try to open other port!" + Environment.NewLine;
                }
            }
            else
            {
                debugBox.Text += "Port already selected!" + Environment.NewLine;
            }
        }
        private void inputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= 'А' && e.KeyChar <= 'Я') || (e.KeyChar >= 'а' && e.KeyChar <= 'я'))
            {
                e.Handled = true;
                debugBox.Text += "Russian characters are not supported, please try again!" + Environment.NewLine;
            }
        }
        private void buttonClearDebug_Click(object sender, EventArgs e)
        {
            debugBox.Clear();
        }
    }
}