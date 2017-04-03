﻿using Newtonsoft.Json;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Integreat.Shared.Models
{
	[Table ("Event")]
	public class Event
	{
		[PrimaryKey, AutoIncrement]
		public int PrimaryKey { get; set; }

		[ForeignKey (typeof(Page))]
		public string PageId { get; set; }


        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("start_date")]
        public string JsonStartDate { get; set; }
        [JsonProperty("end_date")]
        public string JsonEndDate { get; set; }
        [JsonProperty("start_time")]
        public string JsonStartTime { get; set; }
        [JsonProperty("end_time")]
        public string JsonEndTime { get; set; }
        [JsonProperty("all_day")]
        public string JsonAllDay { get; set; }

	    [JsonIgnore]
	    public long StartTime => (JsonStartDate + " " + JsonStartTime).DateTimeFromRestString().Ticks;

	    [JsonIgnore]
	    public long EndTime => (JsonEndDate + " " + JsonEndTime).DateTimeFromRestString().Ticks;

	    [JsonIgnore]
	    public bool AllDay => JsonAllDay.IsTrue();

	}
}

