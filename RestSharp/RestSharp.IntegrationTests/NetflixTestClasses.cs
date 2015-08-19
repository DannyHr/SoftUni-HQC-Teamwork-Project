using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.IntegrationTests
{
    using System.Xml.Serialization;

    #region Netflix test classes

    [XmlRoot("queue")]
    internal class Queue
    {
        [XmlElement("etag")]
        public string Etag { get; set; }

        public List<QueueItem> Items { get; set; }
    }

    [XmlRoot("queue_item")]
    internal class QueueItem
    {
        [XmlElement("id")]
        public string ID { get; set; }

        [XmlElement("position")]
        public int Position { get; set; }
    }

    #endregion
}
