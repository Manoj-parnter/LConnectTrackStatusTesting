using Newtonsoft.Json;

namespace LConnectTrackStatus.APIClass
{
    public class Professional
    {
        public class ProfessionalResult
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string City { get; set; }
            public string Activity { get; set; }
            public string Forwardingno { get; set; }
            public string Pod_no { get; set; }
            public string Remarks { get; set; }
            public string Pieces { get; set; }
            public string Weight { get; set; }
            public string Receiver { get; set; }

            [JsonProperty("Receiver Phno")]
            public string ReceiverPhno { get; set; }
            public string Stamp { get; set; }
            public string Refno { get; set; }
            public string Idproof { get; set; }
            public string Type { get; set; }
        }

        public class Message
        {
            public string pod_no { get; set; }
            public string description { get; set; }
            public string error { get; set; }
        }

        public class ResponseData
        {
            public Message message { get; set; }
        }

        public class ProfessionalErrorResult
        {
            [JsonProperty("Response Data")]
            public ResponseData ResponseData { get; set; }
        }

        public class Professional_OrderTrackDetailStatus
        {
            public string LRNo { get; set; }
            public string TransitDate { get; set; }
            public string TransitLocation { get; set; }
            public string TransitStatus { get; set; }
            public string TransitStatusCode { get; set; }
            public string TransitReason { get; set; }
            public string TransitDescription { get; set; }
            public System.DateTime LastUpdatedDate { get; set; }

        }
    }
    
}
