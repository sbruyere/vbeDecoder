using CommandLine;
using System.Collections.Generic;

namespace vbeDecoder.CLI
{
    public class Options
    {
        [Option("stdin",
          Group = "input",
          Default = false,
          HelpText = "Read from stdin")]
        public bool stdin { get; set; }

        [Option('i', "input", Group = "input", Default = true, Required = true, HelpText = "Input files to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }
        

        [Option('o', "output", HelpText = "Output path.")]
        public string OutputPath { get; set; }

        //[Option(Group = "append", HelpText = "Prefix to append to output file name")]
        //public string Prefix { get; set; }

        //[Option(Group = "append", HelpText = "Suffix to append to output file name")]
        //public string Suffix { get; set; }

    }
}
