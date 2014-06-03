using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSClient
{
    class Options
    {
        [Option('f', "file", Required = true, HelpText = "Input file to be processed.")]
        public string inputFile { get; set; }

        [Option('a', "action", Required = false, HelpText = "Protect/Unprotect file")]
        public string action { get; set; }

        [Option('t', "templateName", Required = false, HelpText = "Template Name")]
        public string templateName { get; set; }

        [Option('r', "rights", Required = false, HelpText = "List of Rights")]
        public string rights { get; set; }

        [Option('i', "fileInfo", Required = false, HelpText = "File Information")]
        public bool fileInfo { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
