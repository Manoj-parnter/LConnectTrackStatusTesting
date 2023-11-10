using LConnectTrackStatus.APIClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Xml;

namespace LConnectTrackStatus
{
	public static class GetDataURL
	{
		private static string Safexpress_BaseAddress = ConfigurationManager.AppSettings["Safexpress_BaseAddress"];
		private static string FilepathPODImage = ConfigurationManager.AppSettings["FilepathPODImage"];
		private static Image _ImagePOD;
		private static string FileExtension;
		public static string webGetMethod(string URL, string _Transport, string _LRNo,string _Plant)
		{
			try
			{
				//_LRNo = "105604776";
				//if (_Transport.ToUpper().Contains("ARC"))
				//{
				//ServicePointManager.Expect100Continue = true;
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
				ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 ;
				//}


				if (_Transport.ToUpper().Contains("DTDC"))
				{
					var httpRequest = (HttpWebRequest)WebRequest.Create(URL);
					httpRequest.Method = "POST";

					//httpRequest.Headers["X-Access-Token"] = "EF014_00007_trk:3221fa4a81a7bb1ed77c1de6f672cf1d";
                    if (_Plant != "2004")
                    {
                        if (_Plant == "2500")
                        {
                            httpRequest.Headers["X-Access-Token"] = "BL11678_trk_json:4db8e57b59c0d854fbbe1a793c3f433f";
                        }
                    }
                    else
                    {
                        httpRequest.Headers["X-Access-Token"] = "EO1200_trk:1f98114b42f5a25a3a59e1f288661fb3";
                    }

                    httpRequest.ContentType = "application/json";
					//httpRequest.Headers["Cookie"] = "JSESSIONID=7DE39A96F8251189FC5957D2BA6D7CB9";

					// var data = @"{ ""trkType"": ""cnno"", ""strcnno"": ""C06385062"",""addtnlDtl"": ""Y""}";

					object input = new
					{
						trkType = "cnno",
						strcnno = _LRNo,
						addtnlDtl = "Y"
					};
					var data = JsonConvert.SerializeObject(input);

					var streamWriter = new StreamWriter(httpRequest.GetRequestStream());
					streamWriter.Write(data);
					streamWriter.Close();

					var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
					var streamReader = new StreamReader(httpResponse.GetResponseStream());
					var result = streamReader.ReadToEnd();

					streamReader.Close();
					httpResponse.Close();
					httpRequest.GetResponse().Close();

					return result;
				}
				else
				{
					WebRequest request = HttpWebRequest.Create(URL);
					WebResponse response = request.GetResponse();
					StreamReader reader1 = new StreamReader(response.GetResponseStream());
					var urlText = reader1.ReadToEnd().Trim();
					reader1.Close();
					response.Close();
					request.GetResponse().Close();
					return urlText;
				}


			}
			catch (Exception ex)
			{

				return ex.Message;
			}


			//string jsonString = "";
			//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
			//request.Method = "GET";
			//request.Credentials = CredentialCache.DefaultCredentials;
			//((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)";
			//request.Accept = "/";
			//request.UseDefaultCredentials = true;
			//request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			//request.ContentType = "application/x-www-form-urlencoded";



			//WebResponse response = request.GetResponse();
			//StreamReader sr = new StreamReader(response.GetResponseStream());
			//jsonString = sr.ReadToEnd();
			//sr.Close();
			//return jsonString;
		}

		public static string XmltoJsonConvert(string Xmldata)
		{
			// To convert an XML node contained in string xml into a JSON string   
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(Xmldata);
			return JsonConvert.SerializeXmlNode(doc);
		}

		public static XmlDocument JsontoXmlConvert(string Jsondata)
		{
			// To convert JSON text contained in string json into an XML node
			return JsonConvert.DeserializeXmlNode(Jsondata);
		}
		public static Boolean IsValidUri(String uri)
		{
			return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
		}

		internal static bool GetImageFromPicPath(string strUrl, string _LRNO, string _Transport)
		{
			try
			{
				if (_Transport == "SAFEXPRESS")
				{
					byte[] data = System.Convert.FromBase64String(strUrl);
					MemoryStream ms = new MemoryStream(data);
					Image image = System.Drawing.Image.FromStream(ms);
					image.Save(FilepathPODImage + _LRNO + ".jpg", ImageFormat.Jpeg);
					image.Dispose();
					return true;
				}
                else if(_Transport == "PROFESSIONAL")
                {
                    byte[] data = System.Convert.FromBase64String(strUrl);
                    MemoryStream ms = new MemoryStream(data);
                    Image image = System.Drawing.Image.FromStream(ms);
                    image.Save(FilepathPODImage + _LRNO + ".jpg", ImageFormat.Jpeg);
                    image.Dispose();
                    return true;
                }
                else
				{
					//using (WebClient webClient = new WebClient())
					//{
					//    byte[] data = webClient.DownloadData(strUrl);
					//    MemoryStream ms = new MemoryStream(data);
					//    ms.Seek(0, SeekOrigin.Begin);
					//    Bitmap bmp = new Bitmap(ms);
					//    Image image = (Image)bmp;
					//    image.Save(FilepathPODImage + _LRNO + ".jpg", ImageFormat.Jpeg);
					//    image.Dispose();
					//    webClient.Dispose();
					//    return true;
					//}

					using (WebClient webClient = new WebClient())
					{
						String sProfile = Convert.ToBase64String(webClient.DownloadData(strUrl));
						//byte[] data = webClient.DownloadData(strUrl);

						using (MemoryStream mem = new MemoryStream(Convert.FromBase64String(sProfile)))
						{
							_ImagePOD = Image.FromStream(mem);
							// If you want it as Jpeg
							_ImagePOD.Save(FilepathPODImage + _LRNO + ".jpg", ImageFormat.Jpeg);
						}
					}

					return true;
				}

            }
			catch (Exception ex)
			{
				WriteToFile(strUrl + "  Error :" + ex.Message, _Transport);
				return false;
			}

		}

		//public static bool SaveImage(MemoryStream ImgUrl, string _LRNO)
		//{
		//    try
		//    {
		//        //using (WebClient webClient = new WebClient())
		//        //{
		//        //    byte[] data = webClient.DownloadData(ImgUrl);

		//        //    using (MemoryStream mem = new MemoryStream(data))
		//        //    {                       

		//        //        using (var yourImage = Image.FromStream(mem))
		//        //        {
		//        //            yourImage.Save(@"\\172.16.1.64\f$\inetpub\wwwroot\LConnect\PODImages\" + _LRNO + ".jpg", ImageFormat.Jpeg);
		//        //        }
		//        //    }

		//        //}

		//        Image img = System.Drawing.Image.FromStream(ImgUrl);

		//        return true;
		//    }
		//    catch
		//    {
		//        WriteToFile(ImgUrl);
		//        return false;
		//    }
		//}

		//public static class ConvertTiffToJpeg
		//{
		//    static string base64String = null;
		//    public static string ImageToBase64(String tifpath, string _LRNO)
		//    {
		//        string path = tifpath;
		//        using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
		//        {
		//            using (MemoryStream m = new MemoryStream())
		//            {
		//                image.Save(m, ImageFormat.Jpeg);
		//                byte[] imageBytes = m.ToArray();
		//                base64String = Convert.ToBase64String(imageBytes);
		//                return base64String;
		//            }
		//        }
		//    }
		//}
		public static DataTable XmlData(string xmlData)
		{
			StringReader theReader = new StringReader(xmlData);
			DataSet theDataSet = new DataSet();
			theDataSet.ReadXml(theReader);

			return theDataSet.Tables[0];
		}
		public static void WriteToFile(string Message, string _Transport)
		{
			string path = Path.GetFullPath(ConfigurationManager.AppSettings["ErrorLogFile"]);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string filepath = Path.GetFullPath(ConfigurationManager.AppSettings["ErrorLogFile"] + "\\" + _Transport + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt");
			if (!File.Exists(filepath))
			{
				// Create a file to write to.   
				using (StreamWriter sw = File.CreateText(filepath))
				{
					sw.WriteLine(Message);
				}
			}
			else
			{
				using (StreamWriter sw = File.AppendText(filepath))
				{
					sw.WriteLine(Message);
				}
			}
		}
		public static DataTable LINQTODataTable<T>(List<T> items)
		{

			DataTable dataTable = new DataTable(typeof(T).Name);
			//Get all the properties

			PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo prop in Props)
			{


				//Setting column names as Property names
				dataTable.Columns.Add(prop.Name);


			}

			foreach (T item in items)
			{
				var values = new object[Props.Length];
				for (int i = 0; i < Props.Length; i++)
				{

					//inserting property values to datatable rows
					values[i] = Props[i].GetValue(item, null);


				}
				dataTable.Rows.Add(values);
			}
			//put a breakpoint here and check datatable

			return dataTable;

		}
		public static DataSet APIList()
		{

			DataSet ods = new DataSet();
			using (SqlConnection objConn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString()))
			{
				SqlCommand cmd = new SqlCommand(ConfigurationManager.AppSettings["APIList_SP"], objConn);
				cmd.CommandType = CommandType.StoredProcedure;
				SqlDataAdapter oda = new SqlDataAdapter();
				oda.SelectCommand = cmd;
				objConn.Open();
				oda.Fill(ods, "APIList");
				objConn.Close();
			}
			return ods;
		}

		public static void UpdateAPIStatus(string rLRNo, string rDeliveryStatus, string rTransport, string rAPIOrderNumber, string rAPIOrderFrom, string rPIOrderLocation, DateTime? rAPIOrderDate,
									string rAPIOrderedBy, float? rAPIActualWeight, float? rAPIPackages, string rAPIServiceName, string rAPIDeliveryLocation, DateTime? rAPIExpectedDate,
									string rAPIReceiverName, DateTime? rAPIDeliveryDate, DataTable rtblOrderTrackStatus, string rIsSMS, char rIsPODAvailable, string rPODType)
		{

			SqlTransaction objTrans = null;
			using (SqlConnection objConn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString()))
			{
				objConn.Open();
				objTrans = objConn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(ConfigurationManager.AppSettings["Update_SP"], objConn, objTrans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@LRNo", DbNullIfNull(rLRNo));
					cmd.Parameters.AddWithValue("@DeliveryStatus", DbNullIfNull(rDeliveryStatus));
					cmd.Parameters.AddWithValue("@Transport", DbNullIfNull(rTransport));
					cmd.Parameters.AddWithValue("@APIOrderNumber", DbNullIfNull(rAPIOrderNumber));
					cmd.Parameters.AddWithValue("@APIOrderFrom", DbNullIfNull(rAPIOrderFrom));
					cmd.Parameters.AddWithValue("@APIOrderLocation", DbNullIfNull(rPIOrderLocation));
					cmd.Parameters.AddWithValue("@APIOrderDate", DbNullIfNull(rAPIOrderDate));
					cmd.Parameters.AddWithValue("@APIOrderedBy", DbNullIfNull(rAPIOrderedBy));
					cmd.Parameters.AddWithValue("@APIActualWeight", DbNullIfNull(rAPIActualWeight));
					cmd.Parameters.AddWithValue("@APIPackages", DbNullIfNull(rAPIPackages));
					cmd.Parameters.AddWithValue("@APIServiceName", DbNullIfNull(rAPIServiceName));
					cmd.Parameters.AddWithValue("@APIDeliveryLocation", DbNullIfNull(rAPIDeliveryLocation));
					cmd.Parameters.AddWithValue("@APIExpectedDate", DbNullIfNull(rAPIExpectedDate));
					cmd.Parameters.AddWithValue("@APIReceiverName", DbNullIfNull(rAPIReceiverName));
					cmd.Parameters.AddWithValue("@APIDeliveryDate", DbNullIfNull(rAPIDeliveryDate));
					cmd.Parameters.AddWithValue("@IsPODAvailable", DbNullIfNull(rIsPODAvailable));
					cmd.Parameters.AddWithValue("@PODType", DbNullIfNull(rPODType));
					cmd.ExecuteNonQuery();


					if (rtblOrderTrackStatus.Rows.Count > 0)
					{
						SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(objConn, SqlBulkCopyOptions.Default, objTrans);

						//Set the database table name
						sqlBulkCopy.DestinationTableName = "dbo.OrderTrackStatusDetail";

						//[OPTIONAL]: Map the DataTable columns with that of the database table
						sqlBulkCopy.ColumnMappings.Add("LRNo", "LRNo");
						sqlBulkCopy.ColumnMappings.Add("TransitDate", "TransitDate");
						sqlBulkCopy.ColumnMappings.Add("TransitLocation", "TransitLocation");
						sqlBulkCopy.ColumnMappings.Add("TransitStatus", "TransitStatus");
						sqlBulkCopy.ColumnMappings.Add("TransitStatusCode", "TransitStatusCode");
						sqlBulkCopy.ColumnMappings.Add("TransitReason", "TransitReason");
						sqlBulkCopy.ColumnMappings.Add("TransitDescription", "TransitDescription");
						sqlBulkCopy.ColumnMappings.Add("LastUpdatedDate", "LastUpdatedDate");

						sqlBulkCopy.WriteToServer(rtblOrderTrackStatus);


					}

					if (rDeliveryStatus.Equals("Delivered"))
					{
						try
						{
							SqlCommand cmdSMS = new SqlCommand("pUpdateSAP_Order_Track_ScheduleSMS", objConn, objTrans);
							cmdSMS.CommandType = CommandType.StoredProcedure;
							cmdSMS.Parameters.AddWithValue("@LRNo", DbNullIfNull(rLRNo));
							cmdSMS.ExecuteNonQuery();
						}
						catch (Exception)
						{

						}
						
					}

					objTrans.Commit();
				}
				catch (Exception ex)
				{
					objTrans.Rollback();
				}
				finally
				{
					objConn.Close();
				}
			}

		}

		public static object DbNullIfNull(this object obj)
		{
			return obj != null ? obj : DBNull.Value;
		}


		public static object GetSAFEXPRESS_Token()
		{
			// to generate token
			using (var client = new HttpClient())
			{
				var form = new Dictionary<string, string>
				{
					{"grant_type", "client_credentials"},
					{"Client_id", "roots.co.in"},
					{"client_secret", "001C9C38CA8E087C236E601A559E702322A3CB783C55D82FE8DD569F73C86110AEE6EA1D32EDCB3BBCA2D32452D3AB0C07348B6A4BCAE932F4AF9297EF7DF929"},
				};
				var tokenResponse = client.PostAsync(Safexpress_BaseAddress + "token", new FormUrlEncodedContent(form)).Result;
				//var token = tokenResponse.Content.ReadAsStringAsync().Result;  
				var token = tokenResponse.Content.ReadAsAsync<SAFEXPRESSClass.Token>(new[] { new JsonMediaTypeFormatter() }).Result;
				if (!string.IsNullOrEmpty(token.access_token))
				{

					return token.access_token;

				}
				else
				{
					return null;
				}

			}
		}

		public static string GetSAFEXPRESS_Image(string token, string LRNo)
		{
			try
			{
				using (var client = new HttpClient())
				{
					// Pass the token in Authorization header as bearer
					client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
					// var content = new StringContent(JsonConvert.SerializeObject(INV_WB_STG), Encoding.UTF8, "application/json");
					var tokenResponse = client.GetAsync(Safexpress_BaseAddress + "api/portal/ViewPod?wayblNo=" + LRNo).Result;

					//Get Response
					var val = tokenResponse.Content.ReadAsAsync<SAFEXPRESSClass.POD>(new[] { new JsonMediaTypeFormatter() }).Result;
					return val.IMG_BINARY;

				}
			}
			catch 
			{

				return null;
			}
			
		}


		public static bool CheckImageExists(string _LRNO)
		{

			DirectoryInfo dir = new DirectoryInfo(FilepathPODImage);
			FileInfo[] files = dir.GetFiles(_LRNO + ".*");
			if (files.Length > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
			//string curFile = FilepathPODImage + _LRNO + ".jpg";
			//return File.Exists(curFile) ? true : false;
		}


		public static string CheckFileExtension(string _LRNO)
		{
			FileExtension=string.Empty;
			DirectoryInfo dir = new DirectoryInfo(FilepathPODImage);
			FileInfo[] files = dir.GetFiles(_LRNO + ".*");
			if (files.Length > 0)
			{
				foreach (FileInfo file in files)
				{
					FileExtension = Path.GetExtension(file.Name);
				}
				return FileExtension;
			}
			else
			{
				return "";
			}
			//string curFile = FilepathPODImage + _LRNO + ".jpg";
			//return File.Exists(curFile) ? true : false;
		}

		public static DataTable _EmptyDataTable()
		{
			DataTable _dtTD = new DataTable();
			_dtTD.Columns.Add(new DataColumn("LRNo", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("TransitDate", typeof(DateTime)));
			_dtTD.Columns.Add(new DataColumn("TransitLocation", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("TransitStatus", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("TransitStatusCode", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("TransitReason", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("TransitDescription", typeof(string)));
			_dtTD.Columns.Add(new DataColumn("LastUpdatedDate", typeof(DateTime)));
			return _dtTD;
		}

		public static bool _DTDCCopyImageToPOD(string _LRNO,string _Plant)
		{

			//FTP Server URL.
			string ftp = _Plant == "2004" ? ConfigurationManager.AppSettings["DTDCFTPPath"] : ConfigurationManager.AppSettings["DTDCFTPPathBLR"];

			//FTP Folder name. Leave blank if you want to list files from root folder.
			string ftpFolder = _Plant == "2004" ? ConfigurationManager.AppSettings["DTDCFTPFolderName"] : ConfigurationManager.AppSettings["DTDCFTPFolderNameBLR"];
			try
			{
				var fileName = _LRNO.Trim() + ".jpg";

				var request = (FtpWebRequest)FtpWebRequest.Create(ftp + ftpFolder + fileName);
				request.Method = WebRequestMethods.Ftp.DownloadFile;


				var dtDusername = _Plant == "2004"
					? ConfigurationManager.AppSettings["DTDCFTPUsername"]
					: ConfigurationManager.AppSettings["DTDCFTPUsernameBLR"];
				var dtdPassword = _Plant == "2004"
					? ConfigurationManager.AppSettings["DTDCFTPPassword"]
					: ConfigurationManager.AppSettings["DTDCFTPPasswordBLR"];
				//var dtDusername= ConfigurationManager.AppSettings["DTDCFTPUsername"];
				//var dtdPassword = ConfigurationManager.AppSettings["DTDCFTPPassword"];

				request.Credentials = new NetworkCredential(dtDusername, dtdPassword);
				
				request.UseBinary = true;
				


				var response = (FtpWebResponse)request.GetResponse();

				using (var responseStream = response.GetResponseStream())
				{
					using (Stream fileStream = new FileStream(FilepathPODImage + fileName, FileMode.CreateNew))
					{
						responseStream?.CopyTo(fileStream);
					}
				}

				response.Close();
				return true;
			}

			catch (WebException ex)
			{
				//var status = ((FtpWebResponse)ex.Response).StatusDescription;
				//throw new Exception((ex.Response as FtpWebResponse).StatusDescription);

                var Defaultboo = false;
                Defaultboo = _DTDCCopyImageToPOD1(_LRNO, _Plant);
                return Defaultboo;
			}











			////FTP Server URL.
			//string ftp = _Plant == "2004" ? ConfigurationManager.AppSettings["DTDCFTPPath"] : ConfigurationManager.AppSettings["DTDCFTPPathBLR"];

			////FTP Folder name. Leave blank if you want to list files from root folder.
			//string ftpFolder = _Plant=="2004"? ConfigurationManager.AppSettings["DTDCFTPFolderName"] : ConfigurationManager.AppSettings["DTDCFTPFolderNameBLR"];
			//         try
			//{
			//	var fileName = _LRNO.Trim() + ".jpg";

			//	var request = (FtpWebRequest)WebRequest.Create(ftp + ftpFolder + fileName);
			//	request.Method = WebRequestMethods.Ftp.DownloadFile;


			//	var dtDusername = _Plant == "2004"
			//		? ConfigurationManager.AppSettings["DTDCFTPUsername"]
			//		: ConfigurationManager.AppSettings["DTDCFTPUsernameBLR"];
			//	var dtdPassword = _Plant == "2004"
			//		? ConfigurationManager.AppSettings["DTDCFTPPassword"]
			//		: ConfigurationManager.AppSettings["DTDCFTPPasswordBLR"];
			//	//var dtDusername= ConfigurationManager.AppSettings["DTDCFTPUsername"];
			//	//var dtdPassword = ConfigurationManager.AppSettings["DTDCFTPPassword"];

			//	request.Credentials = new NetworkCredential(dtDusername, dtdPassword);
			//	request.UsePassive = true;
			//	request.UseBinary = true;
			//	request.EnableSsl = false;


			//	var response = (FtpWebResponse)request.GetResponse();

			//	using (Stream responseStream = response.GetResponseStream())
			//	{
			//		using (Stream fileStream = new FileStream(FilepathPODImage + fileName, FileMode.CreateNew))
			//		{
			//			responseStream?.CopyTo(fileStream);
			//		}
			//	}

			//	response.Close();
			//	return true;
			//}

			//         catch (WebException ex)
			//{
			//             var status = ((FtpWebResponse)ex.Response).StatusDescription;
			//             //throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
			//             return false;
			//}

		}


        public static bool _DTDCCopyImageToPOD1(string _LRNO, string _Plant)
        {

            //FTP Server URL.
            string ftp = _Plant == "2004" ? ConfigurationManager.AppSettings["DTDCFTPPath"] : ConfigurationManager.AppSettings["DTDCFTPPathBLR"];

            //FTP Folder name. Leave blank if you want to list files from root folder.
            string ftpFolder = _Plant == "2004" ? ConfigurationManager.AppSettings["DTDCFTPFolderName"] : ConfigurationManager.AppSettings["DTDCFTPFolderNameBLR"];
            try
            {
                var fileName = _LRNO.Trim() + ".png";

                var request = (FtpWebRequest)FtpWebRequest.Create(ftp + ftpFolder + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;


                var dtDusername = _Plant == "2004"
                    ? ConfigurationManager.AppSettings["DTDCFTPUsername"]
                    : ConfigurationManager.AppSettings["DTDCFTPUsernameBLR"];
                var dtdPassword = _Plant == "2004"
                    ? ConfigurationManager.AppSettings["DTDCFTPPassword"]
                    : ConfigurationManager.AppSettings["DTDCFTPPasswordBLR"];
                //var dtDusername= ConfigurationManager.AppSettings["DTDCFTPUsername"];
                //var dtdPassword = ConfigurationManager.AppSettings["DTDCFTPPassword"];

                request.Credentials = new NetworkCredential(dtDusername, dtdPassword);

                request.UseBinary = true;



                var response = (FtpWebResponse)request.GetResponse();

                using (var responseStream = response.GetResponseStream())
                {
                    using (Stream fileStream = new FileStream(FilepathPODImage + fileName, FileMode.CreateNew))
                    {
                        responseStream?.CopyTo(fileStream);
                    }
                }

                response.Close();
                return true;
            }

            catch (WebException ex)
            {
                var status = ((FtpWebResponse)ex.Response).StatusDescription;
                //throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
                return false;
            }

        }
    }

}