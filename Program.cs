//using System.Windows.Forms;
using System.Text.Json;

using GBX.NET;
//using GBX.NET.LZO;
using GBX.NET.Exceptions;
using GBX.NET.Engines.Game;
using System.Runtime.InteropServices;

namespace ReplayVisualizer
{
    public enum ViewPlane : int
    {
        ZX,
        XY,
        ZY
    }

    public struct Settings
    {
        public float scalingFactor { get; set; }
        public float penWidth { get; set; }
        public Dictionary<string, int> penColor { get; set; }
        public Dictionary<string, int> backgroundColor { get; set; }

        //  Default settings, in case settings.json cannot be opened:
        public Settings()
        { 
            scalingFactor = 5.0f;
            penWidth = 5.0f;
            penColor = new()
            {
                ["R"] = 0,
                ["G"] = 0,
                ["B"] = 0,
                ["A"] = 255
            };
            backgroundColor = new()
            {
                ["R"] = 255,
                ["G"] = 255,
                ["B"] = 255,
                ["A"] = 0
            };
        }
    }

    internal static class Program
    {
        static private ViewPlane viewPlane = ViewPlane.ZX;
        static private bool noWindow = false;
        static private bool saveBitmap = false;

        static void EvaluateArgument(string arg)
        {
            switch (arg)
            {
                case "--bitmap":
                    {
                        saveBitmap = true;
                        break;
                    }
                case "--nowindow":
                    {
                        noWindow = true;
                        break;
                    }
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        [STAThread]
        static void Main(string[] args)
        {
            // Checking whether parameters were given
            if (args.Length == 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);

                Console.WriteLine("\n\nPlease supply a file.");
            }
            else
            {
                // Checking whether more than 1 parameter was given -> ViewPlane
                if (args.Length > 1)
                {
                    // Checking which view plane was given as second parameter, ViewPlane.ZX is already default
                    switch (args[1])
                    {
                        case "XY":
                            {
                                viewPlane = ViewPlane.XY;
                                break;
                            }
                        case "ZY":
                            {
                                viewPlane = ViewPlane.ZY;
                                break;
                            }
                    }
                }

                // Checking whether more parameters were given -> "--nowindow", "--bitmap"
                for (int i = 2; i < args.Length; i++)
                {
                    EvaluateArgument(args[i]);
                }

                if(noWindow)
                {
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    Console.WriteLine("\n\nProgram started.\n\n");
                }


                try
                {
                    // Reading GBX file
                    GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));
                    var node = GameBox.ParseNode(args[0]);


                    // First, checking whether the node is a Replay
                    if (node is CGameCtnReplayRecord replay)
                    {
                        // Setting the node to the Ghost inside the Replay
                        if (replay.Ghosts is not null)
                        {
                            if (replay.Ghosts.Count != 0)
                            {
                                node = replay.Ghosts.First();
                            }
                            else { Console.WriteLine("No ghost found."); FreeConsole(); return; }
                        }
                        else { Console.WriteLine("No ghost found."); FreeConsole(); return; }
                    }


                    // Second, checking whether the node is a Ghost
                    if (node is CGameCtnGhost ghost)
                    {
                        if (ghost.SampleData is not null)
                        {
                            // Reading settings file
                            Settings settings = new();
                            try
                            {
                                string jsonString = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "settings.json"));
                                settings = JsonSerializer.Deserialize<Settings>(jsonString);
                            }
                            catch (FileNotFoundException)
                            {
                                Console.WriteLine("Cannot find \"settings.json\". Make sure it's in the same directory as this application.\nUsing default settings.");
                            }
                            catch (System.Text.Json.JsonException)
                            {
                                Console.WriteLine("Settings in \"settings.json\" are corrupted. Consider refreshing your installation.\nUsing default settings.");
                            }


                            // Initialize window to draw on
                            Form1 form = new() { Text = ghost.GhostLogin + " (" + ghost.EventsDuration + ")" };


                            // Drawing the samples
                            System.Collections.ObjectModel.ObservableCollection<CGameGhost.Data.Sample>? samples = ghost.SampleData.Samples;
                            Color penColor = Color.Black;
                            try
                            {
                                penColor = Color.FromArgb(settings.penColor["A"], settings.penColor["R"], settings.penColor["G"], settings.penColor["B"]);
                            }
                            catch
                            {
                                Console.WriteLine("Could not parse \"penColor\" in the settings file.\nKeys must be \"R\", \"G\", \"B\", \"A\".\nValues must be Integers between 0 and 255.\nUsing default pen color.\n");
                            }
                            Color bgColor = Color.Transparent;
                            try
                            {
                                bgColor = Color.FromArgb(settings.backgroundColor["A"], settings.backgroundColor["R"], settings.backgroundColor["G"], settings.backgroundColor["B"]);
                            }
                            catch
                            {
                                Console.WriteLine("Could not parse \"backgroundColor\" in the settings file.\nKeys must be \"R\", \"G\", \"B\", \"A\".\nValues must be Integers between 0 and 255.\nUsing default background color.\n");
                            }
                            form.DrawSamples(samples, viewPlane, penColor, bgColor, settings.scalingFactor, settings.penWidth, saveBitmap, args[0] + ".bmp");


                            // Displaying the drawn samples
                            if (!noWindow)
                            {
                                form.ShowDialog();
                            }
                        }
                    }
                }
                catch (NotAGbxException e)
                {
                    Console.WriteLine("This file is not a GBX!\n");
                    Console.WriteLine($"{e.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            Console.WriteLine("Program finished.\n");
            FreeConsole();
        }
    }
}