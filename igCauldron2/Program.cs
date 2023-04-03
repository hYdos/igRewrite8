using System.Runtime.InteropServices;
using igLibrary.Core;

namespace igCauldron2
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            AllocConsole();
#endif
            igFileContext.Singleton.Initialize(args[0]);
            igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);

            igArchive arc = new igArchive("archives/zoneInfos.pak");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
#if DEBUG
            FreeConsole();
#endif
        }

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool FreeConsole();
    }
}