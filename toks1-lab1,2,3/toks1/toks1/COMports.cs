using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace toks1
{
    public partial class COMports : Form
    {
        const int PACKAGE_LENGTH = 72;
        const string  PRIMARY_POLYNOMIAL = "10011101";
        private bool selectionFlag = true;
        private SerialPort serialPort = new SerialPort();
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
        private String byteDeStuffing(String text)
        {
            text = text.Replace("$1", "j");
            text = text.Replace("$2", "$");
            return text;
        }
        public void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            String str = Encoding.UTF8.GetString(data);
            //string newStr = byteDeStuffing(str);
            //outputBox.Text = stringFromBinary();
            cyclicDecoding(str);
        }
        public bool writeToPort(byte[] data)
        {
            try
            {
                serialPort.RtsEnable = true;
                serialPort.Write(data, 0, data.Length);
                serialPort.RtsEnable = false;
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
        private void cyclicDecoding(string str)
        {
            List<string> totalList = new List<string>();
            string part = "";
            char[] arr = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                if (arr[i] == ']') 
                {
                    part += arr[i];
                    totalList.Add(part);
                    part = "";
                }
                else
                {
                    part += arr[i];
                }
            }
            string allStr = "";
            //debugBox.Text += "*********" + Environment.NewLine;
            foreach(var element in totalList)
            {
                //debugBox.Text += element + Environment.NewLine;
                string newStr = element;
                newStr = newStr.Replace("[", "");
                newStr = newStr.Replace("]", "");
                newStr = makeMistake(newStr);
                //debugBox.Text += newStr + Environment.NewLine;
                //debugBox.Text += "****!!!*****" + Environment.NewLine;
                string remains = exOr(newStr);
                if (remains == "0000000")
                {
                    //debugBox.Text += "Code is correct!" + Environment.NewLine;
                    string strNew = newStr;  //+++++++++++++++++++++++++++++++++++++
                    strNew = strNew.Remove(0, 72);//+++++++++++++++++++++++++++++++++++++
                    debugBox.Text += strNew + Environment.NewLine;//+++++++++++++++++++++++++++++++++++++
                    newStr = newStr.Remove(newStr.Length - 7);
                    allStr += newStr;
                }
                else
                {
                    //debugBox.Text += "Code isn't correct!" + Environment.NewLine;
                    newStr = correctMistake(newStr, remains);
                    //outputBox.Text = byteDeStuffing(binaryToString(newStr));
                    string strNew = newStr;//+++++++++++++++++++++++++++++++++++++
                    strNew = strNew.Remove(0, 72);//+++++++++++++++++++++++++++++++++++++
                    debugBox.Text += strNew + Environment.NewLine;//+++++++++++++++++++++++++++++++++++++
                    newStr = newStr.Remove(newStr.Length - 7);
                    allStr += newStr;
                }
            }
            outputBox.Text = byteDeStuffing(binaryToString(allStr));
        }

        private string rightShift(string str)
        {
            char[] strArr = str.ToCharArray();
            string s = "";
            s += strArr[str.Length - 1];
            str = str.Remove(str.Length - 1, 1);
            str = str.Insert(0, s);
            return str;
        }
        private string leftShift(string str)
        {
            char[] strArr = str.ToCharArray();
            string s = "";
            s += strArr[0];
            str = str.Remove(0, 1);
            str = str.Insert(str.Length, s);
            return str;
        }

        private int checkWeight(string remains)
        {
            char[] remainsArr = remains.ToCharArray();
            int weight = 0;
            for (int i = 0; i < remainsArr.Length; i++)
            {
                if (remainsArr[i] == '1')
                {
                    weight++;
                }
            }
            return weight;
        }
        private string correctMistake(string str, string remains)
        {
            int moveCounter = 0;
            int weight = checkWeight(remains);
            if (weight == 1)
            {
                str = xor(str, remains);
            }
            if (weight > 1)
            {
                while (true)
                {
                    str = leftShift(str);
                    moveCounter++;
                    remains = exOr(str);
                    weight = checkWeight(remains);
                    if (weight == 1)
                    {
                        str = xor(str, remains);
                        for (int i = 0; i < moveCounter; i++)
                        {
                            str = rightShift(str);
                        }
                        break;
                    }
                    if (weight == 0)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

            }
            //debugBox.Text += "Code became correct!" + Environment.NewLine;
            return str;
        }

        private string xor(string str, string remains)
        {
            char[] strArr = str.ToCharArray();
            char[] remainsArr = remains.ToCharArray();
            string result = "";
            for (int i = 0; i < strArr.Length - remains.Length; i++)
            {
                result += strArr[i];
            }
            for (int i = 0; i < remainsArr.Length; i++)
            {
                if (strArr[i + strArr.Length - remains.Length] == '0' && remainsArr[i] == '0')
                {
                    result += '0';
                }
                if (strArr[i + strArr.Length - remains.Length] == '0' && remainsArr[i] == '1' || remainsArr[i] == '0' && strArr[i + strArr.Length - remains.Length] == '1')
                {
                    result += '1';
                }
                if (strArr[i + strArr.Length - remains.Length] == '1' && remainsArr[i] == '1')
                {
                    result += '0';
                }
            }
            return result;
        }
        private string makeMistake(string str)
        {
            char[] arr = str.ToCharArray();
            Random random = new Random();
            int rand = random.Next(0, 100);
            if (rand < 30) 
            {
                int pos = random.Next(0, 78);
                if (arr[pos] == '0')
                {
                    arr[pos] = '1';
                }
                else
                {
                    arr[pos] = '0';
                }
                String s = new String(arr);
                s = s.Insert(pos + 1, "]");
                s = s.Insert(pos, "[");
                string strNew = s;//+++++++++++++++++++++++++++++++++++++++++
                strNew = strNew.Remove(strNew.Length - 7);//+++++++++++++++++++++++++++++++++++++++++
                debugBox.Text += strNew + " - "; //+++++++++++++++++++++++++++++++++++++++++
                s = s.Replace("[", "");
                s = s.Replace("]", "");
                //debugBox.Text += "new"+s + Environment.NewLine;
                return s;
            }
            else
            {
                string strNew = str;//+++++++++++++++++++++++++++++++++++++++++
                strNew = strNew.Remove(strNew.Length - 7);//+++++++++++++++++++++++++++++++++++++++++
                debugBox.Text += strNew + " - ";  //+++++++++++++++++++++++++++++++++++++++++
                return str;
            }
        }
        private byte[] cyclicEncoding(string inputStr)
        {
            List<string> polynomialList = new List<string>();
            byte[] bytes = Encoding.ASCII.GetBytes(byteStuffing(inputStr));
            string newStr = stringToBinary(bytes);
            int remains = newStr.Length % PACKAGE_LENGTH;
            if (remains != 0)
            {
                for (int j = 0; j < 72 - remains; j++)
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
                    string polynomialStr = "";
                    polynomialStr = newStr.Substring(i - PACKAGE_LENGTH + 1, PACKAGE_LENGTH);
                    polynomialList.Add(polynomialStr);
                }
                i++;
            }  
            List<string> controlList = createControlBits(polynomialList);
            String total = "";
            foreach (var polynomial in polynomialList.Zip(controlList, Tuple.Create))
            {
                string str = polynomial.Item1 + "[" + polynomial.Item2 + "]";
                total += str;
                //debugBox.Text += str + Environment.NewLine;
            }
            //debugBox.Text += "===========================" + Environment.NewLine;
            //debugBox.Text += total + Environment.NewLine;
            byte[] byteStr = Encoding.ASCII.GetBytes(total);      
            return byteStr;
        }

        private List<string> createControlBits(List<string> polynomialList)
        {
            List<string> controlList = new List<string>();
            foreach(var infPolynomial in polynomialList)
            {
                string polynomial = infPolynomial;
                int counter = 0;
                while (counter < 7)
                {
                    polynomial += "0";
                    counter++;
                }
                string control = exOr(polynomial);
                controlList.Add(control);
            }
            return controlList;
        }

        private string exOr(string polynomial)
        {
            while (true)
            {
                int a = polynomial.IndexOf("1", 0);
                if (a > polynomial.Length - 8 || a == -1)
                    break;
                for (int i = 0; i < 8; i++)
                {
                    if (PRIMARY_POLYNOMIAL[i] == polynomial[a + i])
                    {
                        polynomial = polynomial.Remove(a + i, 1);
                        polynomial = polynomial.Insert(a + i, "0");
                    }
                    else
                    {
                        polynomial = polynomial.Remove(a + i, 1);
                        polynomial = polynomial.Insert(a + i, "1");
                    }
                }
            }
            polynomial = polynomial.Substring(polynomial.Length - 7);
            return polynomial;
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
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (selectionFlag)
            {
                debugBox.Text += "Please select a port!" + Environment.NewLine;
            }
            else
            {
                debugBox.Text += "The text after the byte stuffing is shown in the window below!" + Environment.NewLine;
                if (writeToPort(cyclicEncoding(inputBox.Text)))
                {
                    debugBox.Text += "Text successfully sent!" + Environment.NewLine;
                }
                else
                {
                    debugBox.Text += "Port write error!" + Environment.NewLine;
                }
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
        private String byteStuffing(String text)
        {
            int nextLineCounter = 0;
            text = text.Replace("$", "$2");
            char[] arrayText = text.ToCharArray();
            for (int i = 0; i < text.Length; i++)
            {
                if(arrayText[i]=='j')
                {
                    arrayText[i] = 'ф';
                    break;
                }
            }
            String strText = new String(arrayText);
            strText = strText.Replace("j", "$1");
            char[] arrayStrText = strText.ToCharArray();
            for (int i = 0; i < strText.Length; i++)
            {
                if (arrayStrText[i] == 'ф')
                {
                    arrayStrText[i] = 'j';
                }
            }
            String newText = new String(arrayStrText);
            richTextBox.Text = newText;
            for (int i = 0; i < newText.Length; i++)
            {
                if (arrayStrText[i] == '\n')
                {
                    nextLineCounter++;
                }
                else if ((arrayStrText[i] == '$' && arrayStrText[i + 1] == '1') || (arrayStrText[i] == '$' && arrayStrText[i + 1] == '2')) 
                {
                    richTextBox.SelectionStart = i - nextLineCounter;
                    richTextBox.SelectionLength = 2;
                    richTextBox.SelectionColor = Color.Red;
                }
            }
            return newText;
        }

        private void buttonClearDebug_Click(object sender, EventArgs e)
        {
            debugBox.Clear();
        }
    }
}