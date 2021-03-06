﻿using System;
using Newtonsoft.Json;

namespace Ghpr.LocalFileSystem.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SimpleItemInfo
    {
        [JsonProperty(PropertyName = "itemName")]
        public string ItemName { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }
    }
}