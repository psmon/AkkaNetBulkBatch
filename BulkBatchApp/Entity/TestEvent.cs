using System;

namespace BulkBatchApp.Entity
{
    public class TestEvent
    {
        public string id { get; set; }

        public int action_type { get; set; }

        public string action_name { get; set; }

        public DateTime upd_dt { get; set; }

        public DateTime reg_dt { get; set; }

    }


    public class InsertOrUpdateTestEvent : TestEvent
    {

    }

}
