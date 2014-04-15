using System;

namespace Hatfield.Aquarius.PublicServiceAPI
{
    public class QueryResult
    {
        public DateTime Time { get; set; }

        public double? Value { get; set; }

        public int? Quality { get; set; }

        public int? ApprovalLevel { get; set; }
    }
}