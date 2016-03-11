using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace CaptureAndMVControl
{
	public partial class ControlPanelMainForm : Form
	{
		#region Multiviewer Method Patterns
		public static string DEST_URL_TEMPLATE = "http://{0}:8090/Multiviewer/Rest";
		public static string GET_STATUS = "/GetStatus";
		public static string RESTART = "/Restart";
		public static string GET_LAYOUT_NUMBER = "/GetLayoutsNumber";
		public static string SET_ACTIVE_LAYOUT = "/SetActiveLayout?ID={0}";
		public static string GET_ACTIVE_LAYOUT = "/GetActiveLayout";
		public static string GET_PLAYER_NUMBER = "/GetPlayersNumber";
		public static string GET_ACTIVE_PLAYER = "/GetActivePlayer";
		public static string SET_ACTIVE_PLAYER = "/SetActivePlayer?ID={0}";
		public static string GET_AUDIOCHANNELS_NUMBER = "/GetAudioChannelsNumber?PlayerID={0}";
		public static string SET_ACTIVE_AUDIOCHANNEL = "/SetActiveAudioChannel?PlayerID={0}&ID={1}";
		public static string GET_ACTIVE_AUDIOCHANNEL = "/GetActiveAudioChannel?PlayerID={0}";
		public static string SET_PLAYER_BORDERCOLOR = "/SetPlayerBorderColor?PlayerID={0}&Color={1}";
		public static string GET_PLAYER_BORDERCOLOR = "/GetPlayerBorderColor?PlayerID={0}";
		#endregion

		#region Capture Method Patterns
		public static int CAPTURE_BASE_PORT = 8000;
		public static string DEST_CAPTURE_URL = "http://{0}:{1}/REST";
		public static string POST_CAPTURE_START = "/Start";
		public static string POST_CAPTURE_STOP = "/Stop";
        public static string POST_CAPTURE_LOAD_AND_START = "/StartByJobTemplateId";
		#endregion


		private static int TIMEOUT = 2500;
		private static string _server = "localhost";

        #region Multiviewer Methods

        private string SendRequest(string urlDest, string Method="GET", string data="")
		{

			string resString = String.Empty;
			HttpWebRequest request = null;
			try
			{
				request = (HttpWebRequest)WebRequest.Create(urlDest);
                request.Method = Method;
                request.ContentLength = 0;
				request.Timeout = TIMEOUT;
                request.AllowAutoRedirect = false;
                request.ContentType = "application/json";
                if (data != "")
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    request.ContentLength = byteArray.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

				if (request != null)
				{
					HttpWebResponse res = (HttpWebResponse)request.GetResponse();
					StreamReader reader = new StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8);
					resString = reader.ReadToEnd();
					reader.Close();
					res.Close();
				}
			}
			catch (WebException ex)
			{
			}
			return resString;
		}

		int GetNumberOfLayers()
		{
			int res = -1;
			string url = String.Format(DEST_URL_TEMPLATE+GET_LAYOUT_NUMBER, _server);
			string resStr = SendRequest(url);
			if (resStr.Length == 0)
				return res;
			XElement el = XElement.Parse(resStr);
			int.TryParse(el.Value, out res);
			return res;
		}

		public bool SetActiveLayout(int id)
		{
			bool res = false;
			string server = String.Format(DEST_URL_TEMPLATE, _server);
			string command = String.Format(SET_ACTIVE_LAYOUT, id);
			string resStr = SendRequest(server+command);
			if (resStr.Length == 0)
				return res;
			XElement el = XElement.Parse(resStr);
			bool.TryParse(el.Value, out res);
			return res;
		}

		public int GetActiveLayout()
		{
			int res = -1;
			string url = String.Format(DEST_URL_TEMPLATE+GET_ACTIVE_LAYOUT, _server);
			string resStr = SendRequest(url);
			if (resStr.Length == 0)
				return res;
			XElement el = XElement.Parse(resStr);
			int.TryParse(el.Value, out res);
			return res;
        }
        #endregion


        #region Capture Methods
        void StartCapture(int engine)
        {
            string url = String.Format(DEST_CAPTURE_URL + POST_CAPTURE_LOAD_AND_START, _server, CAPTURE_BASE_PORT + engine);
            string id = "";
            switch (engine)
            {
                case 1: id = "\"3b0cb2f4-3661-4235-a37b-ed55ec56f3b7\""; break;
                case 2: id = "\"791e8da0-160f-4642-951a-f59babe08d3d\"}"; break;

            }
            SendRequest(url, "POST", id);
        }

        void StopCapture(int engine)
        {
            string url = String.Format(DEST_CAPTURE_URL + POST_CAPTURE_STOP, _server, CAPTURE_BASE_PORT + engine);
            SendRequest(url, "POST");
        }
        #endregion

    }
}
