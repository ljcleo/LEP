using System;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;

[assembly: NeutralResourcesLanguage("zh-CN")]
[assembly: CLSCompliant(true)]
namespace Lep
{
    class Program
    {
        private static TextReader _reader;
        private static Lexer _lexer;
        private static Parser _parser;
        private static Environment _environment = new Environment();

        [STAThread]
        static void Main(string[] args)
        {
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

            if (args.Length == 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK) _reader = new StreamReader(dialog.FileName);
                    else _reader = Console.In;
                }
            }
            else if (args[0] == "/m")
            {
                OpenNotepad("Readme_LEP.md");
                return;
            }
            else if (args[0] == "/h")
            {
                OpenNotepad("Readme_Intepreter.md");
                return;
            }
            else
            {
                try { _reader = new StreamReader(args[0]); }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            
            _lexer = new Lexer(_reader);
            _parser = new Parser(_lexer);
            Natives.Append(_environment);

            TestIntepreter();
        }

        static void TestIntepreter()
        {
            /*try
            {*/
                IAstNode tree;
                while (_lexer.Peek(0) != Token.EndOfFile)
                {
                    tree = null;

                    try { tree = _parser.Parse(); }
                    catch (ParseException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    /*try {*/ if (!(tree is NullNode)) tree.Evaluate(_environment); /*}
                    catch (LepException e) { Console.WriteLine(e.Message); }*/
                }
            /*}
            catch (LepException e) { Console.WriteLine(e.Message); }*/

            Console.WriteLine(Properties.Resources.ResourceManager.GetString("finish"));
            Console.ReadLine();
        }

        static void OpenNotepad(string filepath)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("notepad.exe", filepath);

                process.Start();
                if (process.HasExited) process.Kill();
            }
        }

        /*
        static void TestLexer() { for (Token next; (next = _lexer.Read()) != Token.EOF; ) Console.WriteLine(next.Text); }

        static void TestParser()
        {
            while (_lexer.Peek(0) != Token.EOF)
            {
                try
                {
                    IASTNode tree = _parser.Parse();
                    Console.WriteLine(tree.ToString());
                }
                catch (ParseException e) { Console.WriteLine(e.Message); }
            }
        }
         */
    }
}
