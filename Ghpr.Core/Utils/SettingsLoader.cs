﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace Ghpr.Core.Utils
{
    public static class SettingsLoader
    {
        public static T LoadSettingsAs<T>(this string fileName)
        {
            var uri = new Uri(typeof(T).Assembly.CodeBase);
            var settingsPath = Path.Combine(Path.GetDirectoryName(uri.LocalPath) ?? "", fileName);
            var settings = JsonConvert.DeserializeObject<T>(File.ReadAllText(settingsPath), new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            });
            return settings;
        }
    }
}