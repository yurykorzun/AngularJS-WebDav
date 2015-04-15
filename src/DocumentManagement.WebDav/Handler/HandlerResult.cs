namespace DocumentManagement.WebDav.Handler
{
    public class HandlerResult
    {
        public HandlerResult()
        {
            ResponseXml = string.Empty;
            ErrorXml = string.Empty;
        }

        public int StatusCode { get; set; }
        public string ResponseXml { get; set; }
        public string ErrorXml { get; set; } 
    }
}