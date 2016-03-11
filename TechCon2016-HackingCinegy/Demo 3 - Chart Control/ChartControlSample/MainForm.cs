using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChartControlSample
{
    public partial class MainForm : Form
    {
        Variable _LeftPos;
        Variable _RightPos;
        Variable _LeftValue;
        Variable _RightValue;

        int leftPerc = 0;
        int rightPerc = 0;
        String _server = "localhost";

        public MainForm()
        {
            InitializeComponent();
             _LeftPos = new Variable ("MaskLeftY", "float", "0");
             _RightPos = new Variable ("MaskRightY", "float", "0");
            _LeftValue = new Variable ("LeftValue", "Text", "0%");
            _RightValue = new Variable ("RightValue", "Text", "0%");

            UpdatePostBox();
        }

        public void UpdatePostBox()
        {
            int leftPos = leftPerc*800/100;
            _LeftPos.Value = leftPos.ToString();
            _LeftValue.Value = leftPerc.ToString() + "%";
            int rightPos = rightPerc*800/100;
            _RightPos.Value = rightPos.ToString();
            _RightValue.Value = rightPerc.ToString() + "%";
            Array var = new[] { _LeftPos, _RightPos, _LeftValue, _RightValue };
            RequestManager.SendPlayoutRequest(_server, 0, RequestManager.CreateXmlRequest(var));
        }

        private void OnLeftValueChanged(object sender, EventArgs e)
        {
            leftPerc = 100-LeftPercControl.Value;
            labelVal1.Text = leftPerc.ToString()+"%";
            UpdatePostBox();
        }

        private void OnRightValueChanged(object sender, EventArgs e)
        {
            rightPerc = 100-RightPercControl.Value;
            labelVal2.Text = rightPerc.ToString()+"%";
            UpdatePostBox();
        }
    }
}
