using LConnectTrackStatus.APIClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using static LConnectTrackStatus.APIClass.GATIClass;
using static LConnectTrackStatus.APIClass.SAFEXPRESSClass;
using static System.Net.Mime.MediaTypeNames;

namespace LConnectTrackStatus
{
    static class Program
    {
        private static DataTable _EmptyDataTable = GetDataURL._EmptyDataTable();
        public static int _GatiSlno, _SafeExpressSlno, _ARCSlno, _SeenivasacoSlno, _DTDCSlno, _Professionalno;
        public static object _SAFEXPRESS_token;
        static void Main(string[] args)
        {
            //object _SAFEXPRESS_token = GetDataURL.GetSAFEXPRESS_Token();
            RunLConnectSchedule();
        }


        public static void RunLConnectSchedule()
        {
            //try
            //{
            DataSet dsLR = GetDataURL.APIList();

            var _DistinctTransport = dsLR.Tables[0].AsEnumerable().Select(row => new { TransportName = row.Field<string>("Transport").Trim() }).Distinct().OrderBy(o => o.TransportName);

            foreach (var _Transport in _DistinctTransport)
            {
                try
                {
                    if (_Transport.TransportName.ToUpper().Contains("SAFEXPRESS"))
                    {
                        _SAFEXPRESS_token = GetDataURL.GetSAFEXPRESS_Token();
                    }
                    var _DistinctLRNO = dsLR.Tables[0].AsEnumerable().Where(r => r.Field<string>("Transport").Trim() == _Transport.TransportName).Select(r => new { rLRNO = r.Field<string>("LRNo").Trim(), rAPIDeliveryDate = r.Field<DateTime?>("APIDeliveryDate"), rPlant = r.Field<string>("Plant").Trim() }).ToList();
                    foreach (var _LRNO in _DistinctLRNO)
                    {
                        try
                        {
                            #region Transport_Gati_API;
                            if (_Transport.TransportName.ToUpper().Contains("GATI"))
                            {

                                var GetData = GetDataURL.webGetMethod(String.Format(ConfigurationManager.AppSettings["GatiUrl"], _LRNO.rLRNO.Trim()), _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());

                                GATIClass.GatiData MyGatidata = JsonConvert.DeserializeObject<GATIClass.GatiData>(GetDataURL.XmltoJsonConvert(GetData));

                                if (!MyGatidata.Gatiresponse.dktinfo.result.Contains("failed"))
                                {
                                    bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                    if (!ValidPODUrl)
                                    {
                                        ValidPODUrl = GetDataURL.IsValidUri(MyGatidata.Gatiresponse.dktinfo.POD);

                                        if (ValidPODUrl)
                                        {
                                            ValidPODUrl = GetDataURL.GetImageFromPicPath(MyGatidata.Gatiresponse.dktinfo.POD, _LRNO.rLRNO.Trim(), "GATI");
                                        }
                                    }



                                    List<GATIClass.OrderTrackDetailStatus> _OrderTrackDetailStatus = new List<GATIClass.OrderTrackDetailStatus>();

                                    _OrderTrackDetailStatus = (from _TD in MyGatidata.Gatiresponse.dktinfo.TRANSIT_DTLS.ROW.AsEnumerable()
                                                               select new GATIClass.OrderTrackDetailStatus()
                                                               {
                                                                   LRNo = _LRNO.rLRNO.Trim(),
                                                                   TransitDate = DateTime.ParseExact(_TD.INTRANSIT_DATE + "" + _TD.INTRANSIT_TIME, "dd-MMM-yyyyHH:mm", new System.Globalization.CultureInfo("en-US")).ToString(),
                                                                   TransitLocation = _TD.INTRANSIT_LOCATION,
                                                                   TransitStatus = _TD.INTRANSIT_STATUS,
                                                                   TransitStatusCode = _TD.INTRANSIT_STATUS_CODE,
                                                                   TransitReason = (string)_TD.REASON_CODE,
                                                                   TransitDescription = (string)_TD.REASON_DESC,
                                                                   LastUpdatedDate = DateTime.Now
                                                               }).ToList();

                                    DataTable _dtTOSD = GetDataURL.LINQTODataTable(_OrderTrackDetailStatus);


                                    GetDataURL.UpdateAPIStatus(
                                                 _LRNO.rLRNO.Trim(),//LRNo
                                                 MyGatidata.Gatiresponse.dktinfo.DOCKET_STATUS == "Docket Delivered" ? "Delivered" : MyGatidata.Gatiresponse.dktinfo.DOCKET_STATUS == "Delivered" ? "Delivered" : MyGatidata.Gatiresponse.dktinfo.DOCKET_STATUS,//DeliveryStatus
                                                 _Transport.TransportName,//Transport
                                                 MyGatidata.Gatiresponse.dktinfo.ORDER_NO.ToString(),//APIOrderNumber
                                                 MyGatidata.Gatiresponse.dktinfo.CONSIGNOR_NAME,//APIOrderFrom
                                                 MyGatidata.Gatiresponse.dktinfo.BOOKING_STATION,//PIOrderLocation
                                                 MyGatidata.Gatiresponse.dktinfo.BOOKED_DATETIME,//APIOrderDate
                                                 MyGatidata.Gatiresponse.dktinfo.CONSIGNEE_NAME,//APIOrderedBy
                                                 MyGatidata.Gatiresponse.dktinfo.ACTUAL_WEIGHT,//APIActualWeight
                                                 MyGatidata.Gatiresponse.dktinfo.NO_OF_PKGS,//APIPackages
                                                 MyGatidata.Gatiresponse.dktinfo.SERVICE_NAME,//APIServiceName
                                                 MyGatidata.Gatiresponse.dktinfo.DELIVERY_STATION,//APIDeliveryLocation 
                                                 MyGatidata.Gatiresponse.dktinfo.ASSURED_DELIVERY_DATE,//APIExpectedDate
                                                 MyGatidata.Gatiresponse.dktinfo.CONSIGNEE_NAME,//APIReceiverName
                                                  MyGatidata.Gatiresponse.dktinfo.DOCKET_STATUS == "Docket Delivered" ? MyGatidata.Gatiresponse.dktinfo.DELIVERY_DATETIME : MyGatidata.Gatiresponse.dktinfo.DOCKET_STATUS == "Delivered" ? MyGatidata.Gatiresponse.dktinfo.DELIVERY_DATETIME : (DateTime?)null,//APIDeliveryDate
                                                 _dtTOSD,//tblOrderTrackStatus
                                                 "N",//IsSMS
                                                 ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                                 ValidPODUrl == false ? null : "Image" //rPODType
                                                 );

                                    _GatiSlno = _GatiSlno + 1;
                                    Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2}  APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _GatiSlno.ToString(), MyGatidata.Gatiresponse.dktinfo.DELIVERY_DATETIME));


                                }
                                else
                                {
                                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2}", _Transport.TransportName, _LRNO.rLRNO.Trim(), MyGatidata.Gatiresponse.dktinfo.result), _Transport.TransportName);
                                }



                            }
                            #endregion

                            #region Transport_SAFEXPRESS_API;
                            else if (_Transport.TransportName.ToUpper().Contains("SAFEXPRESS"))
                            {



                                var GetData = GetDataURL.webGetMethod(String.Format(ConfigurationManager.AppSettings["SafeExpressUrl"], _LRNO.rLRNO.Trim()), _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                SAFEXPRESSClass.SafeexpressData MySafeexpressdata = JsonConvert.DeserializeObject<SAFEXPRESSClass.SafeexpressData>(GetDataURL.XmltoJsonConvert(GetData));

                                if (MySafeexpressdata.Shipment != null)
                                {
                                    bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                    if (!ValidPODUrl)
                                    {
                                        ValidPODUrl = GetDataURL.GetImageFromPicPath(GetDataURL.GetSAFEXPRESS_Image(_SAFEXPRESS_token.ToString(), _LRNO.rLRNO.Trim()), _LRNO.rLRNO.Trim(), "SAFEXPRESS");
                                    }


                                    List<SAFEXPRESSClass.OrderTrackDetailStatus> _OrderTrackDetailStatus = new List<SAFEXPRESSClass.OrderTrackDetailStatus>();

                                    _OrderTrackDetailStatus = (from _TD in MySafeexpressdata.Shipment.Scans.ScanDetail.AsEnumerable()
                                                               select new SAFEXPRESSClass.OrderTrackDetailStatus()
                                                               {
                                                                   LRNo = _LRNO.rLRNO.Trim(),
                                                                   TransitDate = DateTime.ParseExact(_TD.ScanDate + "" + _TD.ScanTime, "dd-MMM-yyyyHH:mm", new System.Globalization.CultureInfo("en-US")).ToString(),
                                                                   TransitLocation = _TD.ScannedLocation,
                                                                   TransitStatus = _TD.Status,
                                                                   TransitStatusCode = null,
                                                                   TransitReason = _TD.ScanGroupType,
                                                                   TransitDescription = _TD.Scan,
                                                                   LastUpdatedDate = DateTime.Now
                                                               }).ToList();

                                    DataTable _dtTOSD = GetDataURL.LINQTODataTable(_OrderTrackDetailStatus);



                                    //string dt = MySafeexpressdata.Shipment.DeliveryDate.ToString() == "null" ? "" : MySafeexpressdata.Shipment.DeliveryDate;
                                    //DateTime? gg = MySafeexpressdata.Shipment.Status == "DELIVERED" ? MySafeexpressdata.Shipment.DeliveryDate == null ? (DateTime?)null : DateTime.Parse(MySafeexpressdata.Shipment.DeliveryDate) : (DateTime?)null;

                                    GetDataURL.UpdateAPIStatus(
                                                  _LRNO.rLRNO.Trim(),//LRNo
                                                   MySafeexpressdata.Shipment.Status == "DELIVERED" ? "Delivered" : "Intransit",//DeliveryStatus
                                                   _Transport.TransportName,//Transport
                                                  null,//APIOrderNumber
                                                  MySafeexpressdata.Shipment.Origin,//APIOrderFrom
                                                  MySafeexpressdata.Shipment.OriginAreaCode,//PIOrderLocation
                                                  DateTime.ParseExact(MySafeexpressdata.Shipment.PickUpDate + "" + MySafeexpressdata.Shipment.PickUpTime, "dd-MMM-yyyyHHmm", new System.Globalization.CultureInfo("en-US")),//APIOrderDate
                                                  MySafeexpressdata.Shipment.Destination,//APIOrderedBy
                                                  MySafeexpressdata.Shipment.Weight,//APIActualWeight
                                                  null,//APIPackages
                                                  MySafeexpressdata.Shipment.SenderName,//APIServiceName
                                                  MySafeexpressdata.Shipment.Destination,//APIDeliveryLocation 
                                                  DateTime.ParseExact(MySafeexpressdata.Shipment.Edd, "dd-MMM-yyyy", new System.Globalization.CultureInfo("en-US")),//APIExpectedDate
                                                 MySafeexpressdata.Shipment.Destination,//APIReceiverName
                                                  MySafeexpressdata.Shipment.Status == "DELIVERED" ? MySafeexpressdata.Shipment.DeliveryDate == "null" ? (DateTime?)null : DateTime.ParseExact(MySafeexpressdata.Shipment.DeliveryDate, "dd-MMM-yyyy", new System.Globalization.CultureInfo("en-US")) : (DateTime?)null,//APIDeliveryDate
                                                  _dtTOSD,//tblOrderTrackStatus
                                                  "N",//IsSMS
                                                 ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                                 ValidPODUrl == false ? null : "Image" //rPODType
                                                  );


                                    _SafeExpressSlno = _SafeExpressSlno + 1;
                                    Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2} APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _SafeExpressSlno.ToString(), MySafeexpressdata.Shipment.DeliveryDate));
                                }
                                else
                                {
                                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), "Failed"), _Transport.TransportName);
                                }


                            }
                            #endregion

                            #region Transport_ARC_API;
                            else if (_Transport.TransportName.ToUpper().Contains("ARC"))
                            {
                                var GetData = GetDataURL.webGetMethod(String.Format(ConfigurationManager.AppSettings["ARCUrl"], _LRNO.rLRNO.Trim()), _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());

                                DataTable ds = GetDataURL.XmlData(GetData);


                                if (ds.Columns.Count > 2)
                                {
                                    ARCClass.Rootobject MyARCdata = JsonConvert.DeserializeObject<ARCClass.Rootobject>(GetDataURL.XmltoJsonConvert(GetData));

                                    if ((MyARCdata.ARCresponse.result ?? "").ToString() == "successful")
                                    {
                                        bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                        if (!ValidPODUrl)
                                        {
                                            ValidPODUrl = GetDataURL.IsValidUri(MyARCdata.ARCresponse.POD);

                                            if (ValidPODUrl)
                                            {
                                                ValidPODUrl = GetDataURL.GetImageFromPicPath(MyARCdata.ARCresponse.POD, _LRNO.rLRNO.Trim(), "ARC");
                                            }
                                        }



                                        GetDataURL.UpdateAPIStatus(
                                                       _LRNO.rLRNO.Trim(),//LRNo
                                                       MyARCdata.ARCresponse.DOCKET_STATUS != "Delivered" ? "Intransit" : "Delivered",//DeliveryStatus
                                                       _Transport.TransportName,//Transport
                                                       null,//APIOrderNumber
                                                       null,//APIOrderFrom
                                                       MyARCdata.ARCresponse.BOOKING_STATION,//PIOrderLocation
                                                       MyARCdata.ARCresponse.BOOKED_DATETIME == "" ? (DateTime?)null : DateTime.ParseExact(MyARCdata.ARCresponse.BOOKED_DATETIME, "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")),//APIOrderDate
                                                       MyARCdata.ARCresponse.CONSIGNEE_NAME,//APIOrderedBy
                                                       (MyARCdata.ARCresponse.ACTUAL_WEIGHT ?? "").ToString() == "" ? (float?)null : float.Parse(MyARCdata.ARCresponse.ACTUAL_WEIGHT.ToString()),//APIActualWeight
                                                       MyARCdata.ARCresponse.NO_OF_PKGS == "" ? (float?)null : float.Parse(MyARCdata.ARCresponse.NO_OF_PKGS),//APIPackages
                                                       (MyARCdata.ARCresponse.SERVICE_NAME ?? "").ToString() == "" ? null : MyARCdata.ARCresponse.SERVICE_NAME.ToString(),//APIServiceName
                                                       MyARCdata.ARCresponse.DELIVERY_STATION,//APIDeliveryLocation 
                                                       (MyARCdata.ARCresponse.ASSURED_DELIVERY_DATE ?? "").ToString() == "" ? (DateTime?)null : DateTime.ParseExact(MyARCdata.ARCresponse.ASSURED_DELIVERY_DATE.ToString().Trim(), "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")),//APIExpectedDate
                                                       (MyARCdata.ARCresponse.RECEIVER_NAME ?? "").ToString(),//APIReceiverName
                                                       MyARCdata.ARCresponse.DOCKET_STATUS == "Delivered" ? DateTime.ParseExact(MyARCdata.ARCresponse.DELIVERY_DATETIME, "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")) : (DateTime?)null,//APIDeliveryDate
                                                       _EmptyDataTable,//tblOrderTrackStatus
                                                       "N",//IsSMS
                                                       ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                                       ValidPODUrl == false ? null : "Image" //rPODType
                                                       );

                                        _ARCSlno = _ARCSlno + 1;
                                        Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2}  APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _ARCSlno.ToString(), MyARCdata.ARCresponse.DELIVERY_DATETIME));
                                    }
                                    else
                                    {
                                        GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), "Failed"), _Transport.TransportName);
                                    }
                                }
                                else
                                {
                                    ARCDoubleClass.Rootobject MyARCdata = JsonConvert.DeserializeObject<ARCDoubleClass.Rootobject>(GetDataURL.XmltoJsonConvert(GetData));

                                    if ((MyARCdata.ARCresponse.result[0] ?? "").ToString() == "successful")
                                    {
                                        bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                        if (!ValidPODUrl)
                                        {
                                            ValidPODUrl = GetDataURL.IsValidUri(MyARCdata.ARCresponse.POD[0]);

                                            if (ValidPODUrl)
                                            {
                                                ValidPODUrl = GetDataURL.GetImageFromPicPath(MyARCdata.ARCresponse.POD[0], _LRNO.rLRNO.Trim(), "ARC");
                                            }
                                        }



                                        GetDataURL.UpdateAPIStatus(
                                                       _LRNO.rLRNO.Trim(),//LRNo
                                                       MyARCdata.ARCresponse.DOCKET_STATUS[0] != "Delivered" ? "Intransit" : "Delivered",//DeliveryStatus
                                                       _Transport.TransportName,//Transport
                                                       null,//APIOrderNumber
                                                       null,//APIOrderFrom
                                                       MyARCdata.ARCresponse.BOOKING_STATION[0],//PIOrderLocation
                                                       MyARCdata.ARCresponse.BOOKED_DATETIME[0] == "" ? (DateTime?)null : DateTime.ParseExact(MyARCdata.ARCresponse.BOOKED_DATETIME[0], "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")),//APIOrderDate
                                                       MyARCdata.ARCresponse.CONSIGNEE_NAME[0],//APIOrderedBy
                                                       (MyARCdata.ARCresponse.ACTUAL_WEIGHT[0] ?? "").ToString() == "" ? (float?)null : float.Parse(MyARCdata.ARCresponse.ACTUAL_WEIGHT[0].ToString()),//APIActualWeight
                                                       MyARCdata.ARCresponse.NO_OF_PKGS[0] == "" ? (float?)null : float.Parse(MyARCdata.ARCresponse.NO_OF_PKGS[0]),//APIPackages
                                                       (MyARCdata.ARCresponse.SERVICE_NAME[0] ?? "").ToString() == "" ? null : MyARCdata.ARCresponse.SERVICE_NAME[0].ToString(),//APIServiceName
                                                       MyARCdata.ARCresponse.DELIVERY_STATION[0],//APIDeliveryLocation 
                                                       (MyARCdata.ARCresponse.ASSURED_DELIVERY_DATE[0] ?? "").ToString() == "" ? (DateTime?)null : DateTime.ParseExact(MyARCdata.ARCresponse.ASSURED_DELIVERY_DATE[0].ToString().Trim(), "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")),//APIExpectedDate
                                                       (MyARCdata.ARCresponse.RECEIVER_NAME[0] ?? "").ToString(),//APIReceiverName
                                                       MyARCdata.ARCresponse.DOCKET_STATUS[0] == "Delivered" ? DateTime.ParseExact(MyARCdata.ARCresponse.DELIVERY_DATETIME[0], "dd-MM-yyyy", new System.Globalization.CultureInfo("en-US")) : (DateTime?)null,//APIDeliveryDate
                                                       _EmptyDataTable,//tblOrderTrackStatus
                                                       "N",//IsSMS
                                                       ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                                       ValidPODUrl == false ? null : "Image" //rPODType
                                                       );

                                        _ARCSlno = _ARCSlno + 1;
                                        Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2} APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _ARCSlno.ToString(), MyARCdata.ARCresponse.DELIVERY_DATETIME[0]));
                                    }
                                    else
                                    {
                                        GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2}", _Transport.TransportName, _LRNO.rLRNO.Trim(), "Failed"), _Transport.TransportName);
                                    }
                                }


                            }
                            #endregion

                            #region Transport_Both_SEENIVASA_SREENIVAS_API;
                            else if (_Transport.TransportName.ToUpper().Contains("SEENIVASA") || _Transport.TransportName.ToUpper().Contains("SREENIVAS"))
                            {
                                var GetData = GetDataURL.webGetMethod(String.Format(ConfigurationManager.AppSettings["SeenivasacoUrl"], _LRNO.rLRNO.Trim(), DateTime.Now.Year), _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                DataSet dsSEENIVASA = new DataSet();
                                dsSEENIVASA.Tables.Add(JsonConvert.DeserializeObject<DataTable>(GetData));

                                if (dsSEENIVASA.Tables[0].Rows.Count > 0)
                                {
                                    bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                    if (!ValidPODUrl)
                                    {
                                        if (dsSEENIVASA.Tables[0].Rows[0]["pod"].ToString().Trim() != "" || dsSEENIVASA.Tables[0].Rows[0]["pod"].ToString().Trim() != "0")
                                        {
                                            ValidPODUrl = GetDataURL.IsValidUri(dsSEENIVASA.Tables[0].Rows[0]["pod"].ToString().Trim());

                                            if (ValidPODUrl)
                                            {
                                                ValidPODUrl = GetDataURL.GetImageFromPicPath(dsSEENIVASA.Tables[0].Rows[0]["pod"].ToString().Trim(), _LRNO.rLRNO.Trim(), "SEENIVASA");
                                            }
                                        }
                                    }



                                    GetDataURL.UpdateAPIStatus(
                                              _LRNO.rLRNO.Trim(),//LRNo
                                               dsSEENIVASA.Tables[0].Rows[0]["DeliveryDate"].ToString().Trim() != "" ? "Delivered" : "Intransit",//DeliveryStatus
                                               _Transport.TransportName,//Transport
                                              dsSEENIVASA.Tables[0].Rows[0]["OrderNumber"].ToString().Trim(),//APIOrderNumber
                                              null,//APIOrderFrom
                                              dsSEENIVASA.Tables[0].Rows[0]["OrderLocation"].ToString().Trim(),//PIOrderLocation
                                              Convert.ToDateTime(dsSEENIVASA.Tables[0].Rows[0]["OrderDate"].ToString().Trim()),//APIOrderDate
                                              null,//APIOrderedBy
                                              null,//APIActualWeight
                                              null,//APIPackages
                                              null,//APIServiceName
                                              dsSEENIVASA.Tables[0].Rows[0]["DeliveryLocation"].ToString().Trim(),//APIDeliveryLocation 
                                              Convert.ToDateTime(dsSEENIVASA.Tables[0].Rows[0]["ExpectedDate"].ToString().Trim()),//APIExpectedDate
                                               null,//APIReceiverName
                                              dsSEENIVASA.Tables[0].Rows[0]["DeliveryDate"].ToString().Trim() != "" ? Convert.ToDateTime(dsSEENIVASA.Tables[0].Rows[0]["DeliveryDate"].ToString().Trim()) : (DateTime?)null,//APIDeliveryDate
                                              _EmptyDataTable,//tblOrderTrackStatus
                                              "N",//IsSMS
                                             ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                             ValidPODUrl == false ? null : "Image" //rPODType
                                              );

                                    _SeenivasacoSlno = _SeenivasacoSlno + 1;
                                    Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2} APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _SeenivasacoSlno.ToString(), dsSEENIVASA.Tables[0].Rows[0]["DeliveryDate"].ToString().Trim()));
                                }
                                else
                                {
                                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), "Nill Count"), _Transport.TransportName);
                                }

                            }
                            #endregion

                            #region Transport_DTDC_API
                            else if (_Transport.TransportName.ToUpper().Contains("DTDC"))
                            {
                                var GetData = GetDataURL.webGetMethod(ConfigurationManager.AppSettings["DTDCUrl"], _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                dynamic _DTDCdata = JObject.Parse(GetData);


                                if (_DTDCdata["status"].ToString().ToUpper().Contains("SUCCESS"))
                                {
                                    var TrackHeader = _DTDCdata.trackHeader;
                                    var TrackDetails = _DTDCdata.GetValue("trackDetails") as JArray;

                                    bool ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                    string ValidFileExtension = GetDataURL.CheckFileExtension(_LRNO.rLRNO.Trim());
                                    if (!ValidPODUrl)
                                    {
                                        ValidPODUrl = GetDataURL._DTDCCopyImageToPOD(_LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                    }



                                    var _OrderTrackDetailStatus = (from _TD in TrackDetails.AsEnumerable()
                                                                   select new
                                                                   {
                                                                       LRNo = _LRNO.rLRNO.Trim(),
                                                                       TransitDate = DateTime.ParseExact((_TD["strActionDate"] ?? "").ToString() + "" + (_TD["strActionTime"] ?? "").ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")).ToString(),
                                                                       TransitLocation = (_TD["strOrigin"] ?? "").ToString() + "->" + (_TD["strDestination"] ?? "").ToString(),
                                                                       TransitStatus = (_TD["strAction"] ?? "").ToString(),
                                                                       TransitStatusCode = (_TD["strCode"] ?? "").ToString(),
                                                                       TransitReason = (_TD["strManifestNo"] ?? "").ToString(),
                                                                       TransitDescription = (_TD["sTrRemarks"] ?? "").ToString(),
                                                                       LastUpdatedDate = DateTime.Now
                                                                   }).ToList();

                                    DataTable _dtTOSD = GetDataURL.LINQTODataTable(_OrderTrackDetailStatus);

                                    //DateTime? ManaulDT = DateTime.Parse(_LRNO.rAPIDeliveryDate.ToString()).ToString("yyyy-MM-dd")== "1900-01-01"? null : _LRNO.rAPIDeliveryDate;
                                    string StrStatus = TrackHeader.strStatus.ToString().Equals("Delivered") == true ? "Delivered" : TrackHeader.strStatus.ToString().Equals("Successfully Delivered") == true ? "Delivered" : TrackHeader.strStatus.ToString().Equals("OTP Based Delivered") == true ? "Delivered" : "Intransit";

                                    //if(StrStatus!= "Delivered")
                                    //{

                                    //}

                                    GetDataURL.UpdateAPIStatus(
                                             _LRNO.rLRNO.Trim(),//LRNo
                                                                //TrackHeader.strStatus.ToString().Equals("Delivered") == true ? "Delivered" : TrackHeader.strStatus.ToString().Equals("Successfully Delivered") == true ? "Delivered"  : ManaulDT != null ? "Delivered" : "Intransit",//DeliveryStatus
                                              TrackHeader.strStatus.ToString().Equals("Delivered") == true ? "Delivered" : TrackHeader.strStatus.ToString().Equals("Successfully Delivered") == true ? "Delivered" : TrackHeader.strStatus.ToString().Equals("OTP Based Delivered") == true ? "Delivered" : "Intransit",//DeliveryStatus
                                              _Transport.TransportName,//Transport
                                             TrackHeader.strShipmentNo.ToString(),//APIOrderNumber
                                             null,//APIOrderFrom
                                            TrackHeader.strOrigin.ToString(),//PIOrderLocation
                                             DateTime.ParseExact(TrackHeader.strBookedDate.ToString() + "" + TrackHeader.strBookedTime.ToString(), "ddMMyyyyHH:mm:ss", new System.Globalization.CultureInfo("en-US")),//APIOrderDate
                                             null,//APIOrderedBy
                                             null,//APIActualWeight
                                             null,//APIPackages
                                             null,//APIServiceName
                                             TrackHeader.strDestination.ToString(),//APIDeliveryLocation 
                                             DateTime.ParseExact(TrackHeader.strExpectedDeliveryDate.ToString(), "ddMMyyyy", new System.Globalization.CultureInfo("en-US")),//APIExpectedDate
                                              null,//APIReceiverName
                                                   //TrackHeader.strStatus.ToString().Equals("Delivered") == true ? DateTime.ParseExact(TrackHeader.strStatusTransOn.ToString() + "" + TrackHeader.strStatusTransTime.ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")) : TrackHeader.strStatus.ToString().Equals("Successfully Delivered") == true ? DateTime.ParseExact(TrackHeader.strStatusTransOn.ToString() + "" + TrackHeader.strStatusTransTime.ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")) : _LRNO.rAPIDeliveryDate != null ? _LRNO.rAPIDeliveryDate : (DateTime?)null,//APIDeliveryDate
                                            TrackHeader.strStatus.ToString().Contains("Delivered") == true ? DateTime.ParseExact(TrackHeader.strStatusTransOn.ToString() + "" + TrackHeader.strStatusTransTime.ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")) : TrackHeader.strStatus.ToString().Equals("Successfully Delivered") == true ? DateTime.ParseExact(TrackHeader.strStatusTransOn.ToString() + "" + TrackHeader.strStatusTransTime.ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")) : TrackHeader.strStatus.ToString().Equals("OTP Based Delivered") == true ? DateTime.ParseExact(TrackHeader.strStatusTransOn.ToString() + "" + TrackHeader.strStatusTransTime.ToString(), "ddMMyyyyHHmm", new System.Globalization.CultureInfo("en-US")) : (DateTime?)null,//APIDeliveryDate
                                             _dtTOSD,//tblOrderTrackStatus
                                             "N",//IsSMS
                                            ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                            ValidPODUrl == false ? null : ValidFileExtension == ".pdf" ? "Pdf" : "Image" //rPODType
                                             );

                                    _DTDCSlno = _DTDCSlno + 1;
                                    Console.WriteLine(String.Format("Slno :{3} Transport: {0} LRNo: {1}  PODStatus : {2}  APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _DTDCSlno.ToString(), TrackHeader.strStatusTransOn.ToString()));


                                }
                                else
                                {
                                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Status : {2}", _Transport.TransportName, _LRNO.rLRNO.Trim(), "Failed"), _Transport.TransportName);
                                }


                            }
                            #endregion

                            #region Transport_Professional_API
                            else if (_Transport.TransportName.ToUpper().Contains("PROFESSIONAL"))
                            {
                                                               
                                var GetData = GetDataURL.webGetMethod(String.Format(ConfigurationManager.AppSettings["ProfessionalUrl"], _LRNO.rLRNO.Trim(), ConfigurationManager.AppSettings[_LRNO.rPlant.Trim() == "2004" ? "TPCUsername" : "TPCUsernameBLR"], ConfigurationManager.AppSettings[_LRNO.rPlant.Trim() == "2004" ? "TPCFTPPassword" : "TPCFTPPasswordBLR"]), _Transport.TransportName, _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                if (GetData.Contains("Unable to connect to the remote server")) continue;

                                bool ValidPODUrl = false; string pDeliveryStatus = "Intransit"; DateTime? pAPIDeliveryDate = null;

                                List<Professional.ProfessionalErrorResult> myProfessionalErrordata = null;
                                List<Professional.ProfessionalResult> myProfessionaldata = null; Professional.ProfessionalResult GetLastData = null;

                                if (GetData.Contains("error"))
                                {
                                    myProfessionalErrordata = JsonConvert.DeserializeObject<List<Professional.ProfessionalErrorResult>>(GetData);
                                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1},Plant: {3}, Status : {2}", _Transport.TransportName, _LRNO.rLRNO.Trim(), myProfessionalErrordata[0].ResponseData.message.description, _LRNO.rPlant), _Transport.TransportName);
                                }
                                else
                                {
                                    myProfessionaldata = JsonConvert.DeserializeObject<List<Professional.ProfessionalResult>>(GetData);
                                    ValidPODUrl = GetDataURL.CheckImageExists(_LRNO.rLRNO.Trim());
                                    string ValidFileExtension = GetDataURL.CheckFileExtension(_LRNO.rLRNO.Trim());
                                    if (!ValidPODUrl)
                                    {
                                        ValidPODUrl = GetDataURL.GetImageFromPicPath(String.Format(ConfigurationManager.AppSettings["ProfessionalImageUrl"], _LRNO.rLRNO.Trim(), ConfigurationManager.AppSettings[_LRNO.rPlant.Trim() == "2004" ? "TPCUsername" : "TPCUsernameBLR"], ConfigurationManager.AppSettings[_LRNO.rPlant.Trim() == "2004" ? "TPCFTPPassword" : "TPCFTPPasswordBLR"]), _LRNO.rLRNO.Trim(), _LRNO.rPlant.Trim());
                                    }
                                }

                                if (!GetData.Contains("error"))
                                {
                                    GetLastData = myProfessionaldata.LastOrDefault(); CultureInfo provider = CultureInfo.InvariantCulture;
                                    pDeliveryStatus = GetLastData.Type.Contains("Delivered") ? "Delivered" : "Intransit";
                                    if (pDeliveryStatus == "Delivered")
                                    {
                                        var datetime_Delivery = GetLastData.Date.ToString() + " " + GetLastData.Time.ToString();
                                        pAPIDeliveryDate = DateTime.ParseExact(datetime_Delivery, "dd/MM/yyyy HH:mm", provider);
                                    }

                                    

                                    var _OrderTrackDetailStatus = (from _TD in myProfessionaldata.AsEnumerable()
                                                               select new Professional.Professional_OrderTrackDetailStatus()
                                                               {
                                                                   LRNo = _TD.Pod_no,
                                                                   TransitDate = DateTime.ParseExact(_TD.Date + " " + _TD.Time, "dd/MM/yyyy HH:mm", provider).ToString(),
                                                                   TransitLocation = _TD.City,
                                                                   TransitStatus = _TD.Type,
                                                                   TransitStatusCode = _TD.Forwardingno,
                                                                   TransitReason = (string)_TD.Activity,
                                                                   TransitDescription = (string)_TD.Remarks,
                                                                   LastUpdatedDate = DateTime.Now
                                                               }).ToList();

                                    DataTable _dtTOSD = GetDataURL.LINQTODataTable(_OrderTrackDetailStatus);


                                    GetDataURL.UpdateAPIStatus(
                                                _LRNO.rLRNO.Trim(),//LRNo
                                                pDeliveryStatus,//DeliveryStatus
                                                 _Transport.TransportName,//Transport
                                                null,//APIOrderNumber
                                                null,//APIOrderFrom
                                                null,//PIOrderLocation
                                                null,//APIOrderDate
                                                null,//APIOrderedBy
                                                null,//APIActualWeight
                                                null,//APIPackages
                                                null,//APIServiceName
                                                null,//APIDeliveryLocation 
                                                null,//APIExpectedDate
                                               null,//APIReceiverName
                                                pAPIDeliveryDate,//APIDeliveryDate
                                                _dtTOSD,//tblOrderTrackStatus
                                                "N",//IsSMS
                                               ValidPODUrl == false ? '\0' : 'Y',//IsPODAvailable
                                               ValidPODUrl == false ? null : "Image" //rPODType
                                                );
                                }                               

                                _Professionalno = _Professionalno + 1;
                                Console.WriteLine(String.Format("SlNo: {3} Transport: {0} LRNo: {1}  PODStatus : {2}  APIDeliveryDate : {4} ", _Transport.TransportName, _LRNO.rLRNO.Trim(), ValidPODUrl == true ? 'Y' : 'N', _Professionalno.ToString(), pAPIDeliveryDate));

                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1}, Error : {2}", _Transport.TransportName, _LRNO.rLRNO.Trim(), ex.Message), _Transport.TransportName);
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    GetDataURL.WriteToFile(String.Format("Transport: {0}, LRNo: {1} ", _Transport.TransportName, e.Message), _Transport.TransportName);
                    continue;
                }
            }
            //}
            //catch (Exception Mex)
            //{
            //    Console.WriteLine("Error Message :"+ Mex.Message);
            //}
        }
    }
}
