using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TextSynth
{
    class Program
    {
        static List<string> savedStrings;
        static string pathToGPT2TC;
        static string lastInput;

        enum StartTypes : int
        {
            RESET,
            LAST,
            MORE
        }

        static string lastOutput
        {
            get
            {
                if (savedStrings.Count == 0)
                    return "";
                return savedStrings[savedStrings.Count - 1];
            }
        }

        static void Main(string[] args)
        {
            savedStrings = new List<string>();

            StartTextGenProcess();
        }

        private static string GenerateText(string input)
        {
            Process texProc = new Process();
            texProc.StartInfo.FileName = Path.Combine(pathToGPT2TC, "gpt2tc.exe");
            texProc.StartInfo.WorkingDirectory = pathToGPT2TC;
            texProc.StartInfo.ArgumentList.Add("g");
            texProc.StartInfo.ArgumentList.Add(input);
            texProc.StartInfo.RedirectStandardOutput = true;

            texProc.Start();
            texProc.WaitForExit();

            return texProc.StandardOutput.ReadToEnd().Trim();
        }

        private static void StartTextGenProcess()
        {
            Console.WriteLine("Welcome to TextSynth.");
            
            bool reset = !File.Exists("GPTpath.txt");
            if (!reset)
            {
                reset = YNPrompt("New path to GPT2TC folder?");
            }

            if (reset)
            {
                Console.Write("Path to folder containing GPT2TC.exe: ");
                pathToGPT2TC = Console.ReadLine();
                File.WriteAllText("GPTpath.txt", pathToGPT2TC);
            }
            else
            {
                pathToGPT2TC = File.ReadAllText("GPTpath.txt");
            }

            TextGenProcess(0);
        }

        private static void TextGenProcess(StartTypes st)
        {
            string input;

            switch(st)
            {
                default:
                    Console.Write("Enter Text: ");
                    input = Console.ReadLine();

                    lastInput = input;

                    savedStrings.Add(input);
                    break;

                case StartTypes.LAST:
                    input = lastInput;
                    break;

                case StartTypes.MORE:
                    input = lastOutput;
                    break;
            }

            string output = GenerateText(input);
            Console.WriteLine(output);

            savedStrings.Add(output);

            char nextType;

            do 
            {
                Console.Write("Enter new prompt (0)/Use this prompt again (1)/More (2): ");
                nextType = Console.ReadKey().KeyChar;
                Console.WriteLine();
            }
            while (!"012".Contains(nextType));
            TextGenProcess((StartTypes)int.Parse(nextType.ToString()));
        }

        static bool YNPrompt(string question)
        {
            char key;

            do
            {
                Console.Write(question);
                key = Console.ReadKey().KeyChar;
            }
            while (!"YyNn".Contains(key));

            Console.WriteLine();
            return "Yy".Contains(key);
        }
    }
}
