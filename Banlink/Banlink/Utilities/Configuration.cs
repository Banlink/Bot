using System;
using System.IO;
using Tommy;

namespace Banlink.Utilities
{
    public static class Configuration
    {
        public static Config ReadConfig(string path)
        {
            try
            {
                var reader = File.OpenText(path);
                var table = TOML.Parse(reader);

                var config = new Config
                {
                    Prefix = table["bot"]["Prefix"],
                    DbUri = table["neo4j"]["ConnectionURL"],
                    Username = table["neo4j"]["Username"],
                    Password = table["neo4j"]["Password"],
                    Token = table["bot"]["Token"]
                };

                reader.Close();
                return config;
            }
            catch (FileNotFoundException)
            {
                Logger.Log(Logger.LogLevel.Fatal, "Could not locate config file! Killing bot...");
                Environment.Exit(0);
            }

            return new Config();
        }

        public struct Config
        {
            public string Token { get; set; }
            public string Prefix { get; set; }
            public string DbUri { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}