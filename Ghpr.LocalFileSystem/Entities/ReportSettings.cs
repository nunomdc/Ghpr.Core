﻿using Newtonsoft.Json;

namespace Ghpr.LocalFileSystem.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReportSettings
    {
        [JsonProperty(PropertyName = "runsToDisplay")]
        public int RunsToDisplay { get; set; }

        [JsonProperty(PropertyName = "testsToDisplay")]
        public int TestsToDisplay { get; set; }

        [JsonProperty(PropertyName = "coreVersion")]
        public string CoreVersion { get; set; }

        [JsonProperty(PropertyName = "reportName")]
        public string ReportName { get; set; }

        public ReportSettings(int runs, int tests, string reportName)
        {
            RunsToDisplay = runs;
            TestsToDisplay = tests;
            CoreVersion = typeof(ReportSettings).Assembly.GetName().Version.ToString();
            ReportName = reportName;
        }
    }
}