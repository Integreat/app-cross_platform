﻿using Newtonsoft.Json;

namespace Integreat.Shared.Models
{
    /// <summary>
    /// Describes a Language in our data model.
    /// </summary>
   	public class Language
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string ShortName { get; set; }

        [JsonProperty("native_name")]
        public string Name { get; set; }

        [JsonProperty("country_flag_url")]
        public string IconPath { get; set; }

        [JsonProperty("dir")]
        public string Direction { get; set; }

        public Location Location { get; set; }

        public string PrimaryKey { get; set; }

        public override string ToString()
        {
            return ShortName + "";
        }
    }
}

