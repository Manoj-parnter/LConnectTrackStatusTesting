using System;

namespace LConnectTrackStatus.APIClass
{
    public class SAFEXPRESSClass
    {

        public class SafeexpressData
        {
            public Shipment Shipment { get; set; }
        }

        public class Shipment
        {
            public string RefNo { get; set; }
            public string Waybill { get; set; }
            public string PickUpDate { get; set; }
            public string PickUpTime { get; set; }
            public string Origin { get; set; }
            public string OriginAreaCode { get; set; }
            public string Destination { get; set; }
            public string ProductType { get; set; }
            public string SenderName { get; set; }
            public string ToAttention { get; set; }
            public float? Weight { get; set; }
            public string Status { get; set; }
            public string Edd { get; set; }
            public string ShipDate { get; set; }
            public string DeliveryDate { get; set; }
            public Scans Scans { get; set; }
        }

        public class Scans
        {
            public Scandetail[] ScanDetail { get; set; }
        }

        public class Scandetail
        {
            public string Scan { get; set; }
            public string ScanGroupType { get; set; }
            public string ScanDate { get; set; }
            public string ScanTime { get; set; }
            public string ScannedLocation { get; set; }
            public string Status { get; set; }
        }

        public class POD
        {
            public string WAYBL_NO { get; set; }
            public string IMG_BINARY { get; set; }
        }

        public class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
        }

        public class OrderTrackDetailStatus
        {
            public string LRNo { get; set; }
            public string TransitDate { get; set; }
            public string TransitLocation { get; set; }
            public string TransitStatus { get; set; }
            public string TransitStatusCode { get; set; }
            public string TransitReason { get; set; }
            public string TransitDescription { get; set; }
            public DateTime LastUpdatedDate { get; set; }

        }
    }
}
