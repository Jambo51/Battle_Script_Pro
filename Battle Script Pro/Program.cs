using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Battle_Script_Pro
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            switch (args.Length)
            {
                case 2:
                    try
                    {
                        Application.Run(new Form1(Int32.Parse(args[0]), args[1]));
                    }
                    catch (FormatException)
                    {
                        Application.Run(new Form1(Int32.Parse(Convert.ToInt32(args[0].Substring(2), 16).ToString()), args[1]));
                    }
                    catch
                    {
                        MessageBox.Show("Unable to parse the passed location. Starting up without decompiling...");
                        Application.Run(new Form1());
                    }
                    break;
                case 1:
                    Application.Run(new Form1(args[0]));
                    break;
                default:
                    Application.Run(new Form1());
                    break;
            }
        }
    }
}
