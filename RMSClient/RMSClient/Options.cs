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
        public string InputFile { get; set; }

        [Option('a', "action", Required = true, HelpText = "Protect/Unprotect file")]
        public string action { get; set; }

        [Option('t', "templateName", HelpText = "Template Name")]
        public string templateName { get; set; }

        [Option('r', "rights", HelpText = "list of rights")]
        public string rights { get; set; }
        

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
