using System;

namespace LConnectTrackStatus.APIClass
{
    public class GATIClass
    {
        public class GatiData
        {
            public Gatiresponse Gatiresponse { get; set; }
        }

        public class Gatiresponse
        {
            public string requid { get; set; }
            public Dktinfo dktinfo { get; set; }
        }

        public class Dktinfo
        {
            public string dktno { get; set; }
            public string result { get; set; }
            public PREPICKUP_INFO PREPICKUP_INFO { get; set; }
            public string DOCKET_NUMBER { get; set; }
            public string DOCKET_STATUS { get; set; }
            public string ORDER_NO { get; set; }
            public object REF_NUMBER { get; set; }
            public string CONSIGNOR_NAME { get; set; }
            public string CONSIGNEE_NAME { get; set; }
            public string BOOKING_STATION { get; set; }
            public DateTime? BOOKED_DATETIME { get; set; }
            public float? ACTUAL_WEIGHT { get; set; }
            public float? NO_OF_PKGS { get; set; }
            public string SERVICE_NAME { get; set; }
            public string DELIVERY_STATION { get; set; }
            public DateTime? ASSURED_DELIVERY_DATE { get; set; }
            public DateTime? REVISED_DELIVERY_DATE { get; set; }
            public object REVISED_DELIVERY_REASON { get; set; }
            public string RECEIVER_NAME { get; set; }
            public DateTime? DELIVERY_DATETIME { get; set; }
            public TRANSIT_DTLS TRANSIT_DTLS { get; set; }
            public string POD { get; set; }
        }

        public class PREPICKUP_INFO
        {
            public PINFO PINFO { get; set; }
        }

        public class PINFO
        {
            public object PICKUP_STATUS { get; set; }
            public object LASTUPDATED_DATE { get; set; }
            public object INSTRUCTION { get; set; }
            public object PICKUP_DATE { get; set; }
        }

        public class TRANSIT_DTLS
        {
            public ROW[] ROW { get; set; }
        }

        public class ROW
        {
            public string INTRANSIT_DATE { get; set; }
            public string INTRANSIT_TIME { get; set; }
            public string INTRANSIT_LOCATION { get; set; }
            public string INTRANSIT_STATUS { get; set; }
            public string INTRANSIT_STATUS_CODE { get; set; }
            public object REASON_CODE { get; set; }
            public object REASON_DESC { get; set; }
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
