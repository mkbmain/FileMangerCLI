using System;
using System.Text.Json.Serialization;

namespace FileManagerCLI.Settings
{

    public class Config
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.White;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Black;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ConsoleModifiers ModKey { get; set; } = ConsoleModifiers.Control;
    }
}