using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureAndMVControl
{
	public partial class ControlPanelMainForm : Form
	{
        int currentLayout = -1;
        int layoutCount = 1;
		public ControlPanelMainForm()
		{
			InitializeComponent();
            layoutCount = GetNumberOfLayers();
            currentLayout = 0;
            InitSerial();
            Serial_Clear();
            UpdateUIStatus();
		}

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            Serial_Clear();
        }

        void UpdateUIStatus()
        {
            btnStop1.Enabled = btnRec1.Enabled = btnLayout1.Enabled = (layoutCount > 0);
            btnStop2.Enabled = btnRec2.Enabled = btnLayout2.Enabled = (layoutCount > 1);
            btnStop3.Enabled = btnRec3.Enabled = btnLayout3.Enabled = (layoutCount > 2);
            btnStop4.Enabled = btnRec4.Enabled = btnLayout4.Enabled = (layoutCount > 3);

            group1.Visible = (currentLayout == 0);
            group2.Visible = (currentLayout == 1);
            group3.Visible = (currentLayout == 2);
            group4.Visible = (currentLayout == 3);
        }

        void OnSerialData(string Msg)
        {
            if (Msg.CompareTo("CAPSTOP")==0) 
                DoStopCapture(1);
        }

        void DoStopCapture(int engine)
        {
            StopCapture(engine);
            Serial_Clear();
        }
        
		#region Button Comamnds
		private void OnLayout1(object sender, EventArgs e)
		{
            SetActiveLayout(currentLayout=0);
            UpdateUIStatus();
        }

		private void OnLayout2(object sender, EventArgs e)
		{
            SetActiveLayout(currentLayout = 1);
            UpdateUIStatus();
        }

		private void OnLayout3(object sender, EventArgs e)
		{
            SetActiveLayout(currentLayout = 2);
            UpdateUIStatus();
        }

		private void OnLayout4(object sender, EventArgs e)
		{
            SetActiveLayout(currentLayout = 3);
            UpdateUIStatus();
        }

		private void OnRec1(object sender, EventArgs e)
		{
            StartCapture(1);
            Serial_StartRecord();
		}

		private void OnRec2(object sender, EventArgs e)
		{
            StartCapture(2);
        }

		private void OnRec3(object sender, EventArgs e)
		{
            StartCapture(3);
		}

		private void OnRec4(object sender, EventArgs e)
		{
            StartCapture(4);
		}

		private void OnStop1(object sender, EventArgs e)
		{
            DoStopCapture(1);
		}

		private void OnStop2(object sender, EventArgs e)
		{
            StopCapture(2);
		}

		private void OnStop3(object sender, EventArgs e)
		{
            StopCapture(3);
		}

		private void OnStop4(object sender, EventArgs e)
		{
            StopCapture(4);
		}
		#endregion
	}
}
