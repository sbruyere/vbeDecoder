using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace vbeDecoder.CLI
{
    public class Program
    {
        public static void MainRun(string[] args)
        {
            Console.Title = "vbeDecoder";

            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions);
        }

        public static void RunOptions(Options opts)
        {
            string result;

            if (opts.stdin)
            {
                Stream s = Console.OpenStandardInput();

                result = ScriptDecoder.DecodeStream(s);

                OutputResult(opts, result, null);
            }

            if (opts.InputFiles?.Any() == true)
            {
                foreach (var srcPath in opts.InputFiles)
                {
                    result = ScriptDecoder.DecodeFile(srcPath);

                    OutputResult(opts, result, ChangeFileName(Path.GetFileName(srcPath)));
                }
            }
        }

        private static void OutputResult(Options opts, string result, string filename)
        {
            if (opts.OutputPath == null)
            {
                Console.WriteLine();
                Console.WriteLine($"'############################################################");
                Console.WriteLine($"'# vbeDecoder - by Sylvain Bruyere ");
                Console.WriteLine($"'# - Input File: {opts.OutputPath}");
                Console.WriteLine();
                Console.WriteLine(result);
            }
            else if (Directory.Exists(opts.OutputPath))
            {
                string newFilename = ChangeFileName(Path.GetFileName(filename));
                string oPath = Path.Combine(opts.OutputPath, newFilename);

                WriteFileResult(result, oPath);
            }
            else
            {
                string newFilename = ChangeFileName(Path.GetFileName(filename));
                string oPath = opts.OutputPath + "-" + newFilename;

                WriteFileResult(result, oPath);
            }
        }

        private static string ChangeFileName(string filename)
        {
            string oPath;
            var extPos = filename.Trim().ToLower().IndexOf(".vbe");

            if (extPos > 0)
                oPath = filename.Substring(0, extPos);
            else
                oPath = filename;

            return $"{oPath}.vbs.decoded";
        }

        private static void WriteFileResult(string result, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                while (!"yYnNcC".Contains(key.KeyChar))
                {
                    Console.WriteLine($"File '{outputPath}' already exists. Overwrite ? [Y] Yes / [N] No / [C] Cancel");
                    key = Console.ReadKey();
                }

                switch (Char.ToUpper(key.KeyChar))
                {
                    case 'Y':
                        File.Delete(outputPath);
                        File.WriteAllText(outputPath, result);
                        break;
                    case 'N':
                    case 'C':
                        Console.WriteLine("Canceled.");
                        break;
                }
            } else
            {
                File.WriteAllText(outputPath, result);
            }
        }


    }
}
