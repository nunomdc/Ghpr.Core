﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Ghpr.Core.Extensions
{
    public static class StringExtensions
    {
        public static void Create(this string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static Guid ToMd5HashGuid(this string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            var bytes = Encoding.Default.GetBytes(value);
            byte[] data;

            using (var md5Hasher = MD5.Create())
            {
                data = md5Hasher.ComputeHash(bytes);
            }

            return new Guid(data);
        }

        public static T LoadAs<T>(this string fullPath)
        {
            T t;
            using (var file = File.OpenText(fullPath))
            {
                var serializer = new JsonSerializer();
                t = (T)serializer.Deserialize(file, typeof(T));
            }
            return t;
        }
    }
}