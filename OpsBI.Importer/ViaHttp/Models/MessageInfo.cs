using System;
using System.Linq;

namespace OpsBI.Importer.ViaHttp.Models
{
    [Serializable]
    public class MessageInfo
    {
        public MessageInfo()
        {
        }

        public MessageInfo(string id, string label, DateTime timeSent)
        {
            Id = id;
            Label = label;
            TimeSent = timeSent;
        }

        public string Id { get; set; }

        public string Label { get; set; }

        public string MessageType { get; set; }

        public DateTime TimeSent { get; set; }

        public string FriendlyMessageType { get; private set; }

        public void OnMessageTypeChanged()
        {
            if (string.IsNullOrEmpty(MessageType))
                return;

            var clazz = MessageType.Split(',').First();
            var objectName = clazz.Split('.').Last();

            if (objectName.Contains("+"))
                objectName = objectName.Split('+').Last();

            FriendlyMessageType = objectName;
        }
    }
}
