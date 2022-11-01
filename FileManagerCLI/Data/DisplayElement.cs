using System;
using System.Drawing;

namespace FileManagerCLI.Data
{
    public struct DisplayElement
    {
        public Point Point { get; set; }
        public char Value { get; set; }
        public bool Selected { get; set; }
        public ConsoleColor BackgroundColor => Selected ? ConsoleColor.Black : ConsoleColor.White;
        public ConsoleColor ForegroundColor => Selected ? ConsoleColor.White : ConsoleColor.Black;
    }
}