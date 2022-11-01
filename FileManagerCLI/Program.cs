using System;
using System.Linq;

namespace FileManagerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "FileManager";
            FileManagerDisplay.InitDisplay();

            while (true)
            {
                var readKey = Console.ReadKey(true);
                switch (readKey.Key)
                {
                    case ConsoleKey.UpArrow:
                        FileManagerDisplay.ChangeSelected(true);
                        break;
                    case ConsoleKey.DownArrow:
                        FileManagerDisplay.ChangeSelected(false);
                        break;
                    
                    case ConsoleKey.Enter:
                        FileManagerDisplay.Select();
                        break;
                        
                }
            }
        }
    }
}