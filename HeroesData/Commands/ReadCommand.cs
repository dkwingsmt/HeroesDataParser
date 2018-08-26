﻿using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HeroesData.Commands
{
    internal class ReadCommand : ICommand
    {
        private readonly string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly HashSet<string> ValidFileExtensions = new HashSet<string>();
        private readonly CommandLineApplication CommandLineApplication;

        private ReadCommand(CommandLineApplication app)
        {
            CommandLineApplication = app;

            ValidFileExtensions.Add(".txt");
            ValidFileExtensions.Add(".xml");
            ValidFileExtensions.Add(".json");
        }

        public static ReadCommand Add(CommandLineApplication app)
        {
            return new ReadCommand(app);
        }

        public void SetCommand()
        {
            CommandLineApplication.Command("read", config =>
            {
                config.HelpOption("-?|-h|--help");

                CommandOption fileNameOption = config.Option("-f|--fileName", "The filename of the file to read and display on the console", CommandOptionType.SingleValue);

                config.OnExecute(() =>
                {
                    if (fileNameOption.HasValue())
                        ReadFile(fileNameOption.Value());
                    else
                        config.ShowHelp();

                    return 0;
                });
            });
        }

        private void ReadFile(string fileName)
        {
            string filePath = Path.Combine(AppPath, fileName);

            if (!Path.HasExtension(filePath))
            {
                filePath += ".txt";
                fileName += ".txt";
            }
            else if (!ValidFileExtensions.Contains(Path.GetExtension(filePath)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Not a valid file to read, must be one of the following file types: ");
                foreach (string type in ValidFileExtensions)
                    Console.Write($"{type} ");

                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();

                return;
            }

            if (File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{fileName} - {File.GetLastWriteTime(filePath)}");
                Console.ResetColor();
                Console.WriteLine();

                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        Console.WriteLine(reader.ReadLine());
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("===EOF===");
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File not found at {filePath}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
}
