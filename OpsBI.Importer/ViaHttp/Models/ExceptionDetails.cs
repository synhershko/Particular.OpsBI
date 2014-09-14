namespace OpsBI.Importer.ViaHttp.Models
{
    public class ExceptionDetails
    {
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
    }
}
