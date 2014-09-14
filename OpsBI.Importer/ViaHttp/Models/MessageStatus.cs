using System.ComponentModel;

namespace OpsBI.Importer.ViaHttp.Models
{
    public enum MessageStatus
    {
        [Description("Failed")]
        Failed = 1,

        [Description("Repeated Failures")]
        RepeatedFailure = 2,

        [Description("Successful")]
        Successful = 3,

        [Description("Successfully resolved")]
        ResolvedSuccessfully = 4,

        [Description("Failure message archived")]
        ArchivedFailure = 5,

        [Description("Retry Requested")]
        RetryIssued = 6,

        // Failed message statuses:

        [Description("Unresolved")]
        Unresolved = 7,

        [Description("Resolved")]
        Resolved = 8,

        [Description("Archived")]
        Archived = 9,
    }
}
