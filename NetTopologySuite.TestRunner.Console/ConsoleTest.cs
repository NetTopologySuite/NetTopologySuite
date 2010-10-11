using System;
using SysConsole = System.Console;

namespace NetTopologySuite.Console
{
    internal partial class ConsoleTest
    {
        private enum CoordinateFactories
        {
            None,
            BufferedCoordinate,
            SimpleCoordinate
        }

        private static CoordinateFactories ChooseFactories()
        {
            SysConsole.WriteLine("*** Choose which CoordinateFactory to use:");
            SysConsole.WriteLine("(a) BufferedCoordinate");
            SysConsole.WriteLine("(b) SimpleCoordinate");

            CoordinateFactories cf = CoordinateFactories.None;
            while (cf == CoordinateFactories.None)
            {
                ConsoleKeyInfo pressed = SysConsole.ReadKey(true);
                switch (pressed.KeyChar)
                {
                    case 'a':
                        cf = CoordinateFactories.BufferedCoordinate;
                        break;
                    case 'b':
                        cf = CoordinateFactories.SimpleCoordinate;
                        break;
                    default:
                        SysConsole.WriteLine(string.Format("Invalid CoordinateFactory entry: '{0}'", pressed.KeyChar));
                        SysConsole.WriteLine("Try again");
                        break;
                }
            }
            return cf;
        }

        private static void PrintMenu()
        {
            SysConsole.WriteLine("** Interactive Test Instructions **");
            SysConsole.WriteLine("a. Enter the name of the test script file to run.");
            SysConsole.WriteLine("b. Or enter 'exit' (without the quote) to end the test.");
            SysConsole.WriteLine("c. Or enter 'default' (without the quote) to run the default tests.");
            SysConsole.WriteLine();
            SysConsole.Write("Test Runner>>");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            switch (ChooseFactories())
            {
                case CoordinateFactories.BufferedCoordinate:
                    Buffered.Start(args);
                    break;
                case CoordinateFactories.SimpleCoordinate:
                    Simple.Start(args);
                    break;
            }
        }

    }
}