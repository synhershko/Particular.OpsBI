namespace OpsBI.Importer.ViaHttp.Models
{
    public class FailedMessage : StoredMessage
    {
        public ExceptionDetails Exception { get; set; }

        public int NumberOfProcessingAttempts { get; set; }
    }
}
