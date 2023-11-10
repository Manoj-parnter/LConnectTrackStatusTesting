using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LConnectTrackStatus.APIClass
{
    public class ARCDoubleClass
    {
        public class Rootobject
        {
            public Xml xml { get; set; }
            public Arcresponse ARCresponse { get; set; }
        }

        public class Xml
        {
            public string version { get; set; }
        }

        public class Arcresponse
        {
            public string requid { get; set; }
            public string[] dktno { get; set; }
            public string[] result { get; set; }
            public PREPICKUP_INFO[] PREPICKUP_INFO { get; set; }
            public string[] DOCKET_NUMBER { get; set; }
            public string[] DOCKET_STATUS { get; set; }
            public string[] ORDER_NO { get; set; }
            public object[] REF_NUMBER { get; set; }
            public string[] CONSIGNOR_NAME { get; set; }
            public string[] CONSIGNEE_NAME { get; set; }
            public string[] BOOKING_STATION { get; set; }
            public string[] BOOKED_DATETIME { get; set; }
            public object[] ACTUAL_WEIGHT { get; set; }
            public string[] NO_OF_PKGS { get; set; }
            public object[] SERVICE_NAME { get; set; }
            public string[] DELIVERY_STATION { get; set; }
            public object[] ASSURED_DELIVERY_DATE { get; set; }
            public object[] RECEIVER_NAME { get; set; }
            public string[] DELIVERY_DATETIME { get; set; }
            public string[] POD { get; set; }
            public object[] TRANSIT_DTLS { get; set; }
        }

        public class PREPICKUP_INFO
        {
            public object PINFO { get; set; }
            public object PICKUP_STATUS { get; set; }
            public object LASTUPDATED_DATE { get; set; }
            public object INSTRUCTION { get; set; }
            public object PICKUP_DATE { get; set; }
        }

    }

    public class ARCClass
    {

        public class Rootobject
        {
            public Xml xml { get; set; }
            public Arcresponse ARCresponse { get; set; }
        }

        public class Xml
        {
            public string version { get; set; }
        }

        public class Arcresponse
        {
            public string requid { get; set; }
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
            public string BOOKED_DATETIME { get; set; }
            public object ACTUAL_WEIGHT { get; set; }
            public string NO_OF_PKGS { get; set; }
            public object SERVICE_NAME { get; set; }
            public string DELIVERY_STATION { get; set; }
            public object ASSURED_DELIVERY_DATE { get; set; }
            public object RECEIVER_NAME { get; set; }
            public string DELIVERY_DATETIME { get; set; }
            public string POD { get; set; }
            public object TRANSIT_DTLS { get; set; }
        }

        public class PREPICKUP_INFO
        {
            public object PINFO { get; set; }
            public object PICKUP_STATUS { get; set; }
            public object LASTUPDATED_DATE { get; set; }
            public object INSTRUCTION { get; set; }
            public object PICKUP_DATE { get; set; }
        }

    }
}
