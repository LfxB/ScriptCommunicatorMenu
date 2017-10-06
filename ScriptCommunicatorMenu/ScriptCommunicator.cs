using GTA; // This is a reference that is needed! do not edit this
using GTA.Native; // This is a reference that is needed! do not edit this
using System; // This is a reference that is needed! do not edit this
using System.Windows.Forms; // This is a reference that is needed! do not edit this
using Color = System.Drawing.Color;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using SimpleUI;
using Control = GTA.Control;

namespace ScriptCommunicator
{
    public class ScriptCommunicator : Script // declare Modname as a script
    {
        bool initialize = true;
        static string SettingsDirectory = @"scripts\";
        int InputTimer;

        MenuPool _menuPool;
        UIMenu LeMainMenu;

        public ScriptCommunicator() // main function
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Aborted += OnAbort;

            Interval = 0;
        }

        private void OnAbort(object sender, EventArgs e)
        {
            foreach (ModAndMenuItemPair pair in ModMenuItems)
            {
                pair.WaitHandle.Close();
            }
        }

        List<ModAndMenuItemPair> ModMenuItems = new List<ModAndMenuItemPair>();
        void CollectCompatibleMods()
        {
            ModNameReaderClass mnrClass = new ModNameReaderClass();
            mnrClass.LoadCompatibleMods(Path.GetFullPath(Path.Combine(SettingsDirectory, @"..\")), "*.scmod");
            mnrClass.LoadCompatibleMods(SettingsDirectory, "*.scmod");

            foreach (string pathname in mnrClass.GetFullPathNames())
            {
                string scriptTitle;
                string scriptDescription;
                using (StreamReader reader = new StreamReader(pathname))
                {
                    scriptTitle = reader.ReadLine() ?? "";
                    scriptDescription = reader.ReadLine() ?? "";
                }

                string scriptFilename = Path.GetFileNameWithoutExtension(pathname);

                ModMenuItems.Add(new ModAndMenuItemPair(new UIMenuItem(scriptTitle, null, scriptDescription), scriptFilename, scriptFilename, new EventWaitHandle(false, EventResetMode.ManualReset, scriptFilename)));
            }
        }

        void InitMenu()
        {
            _menuPool = new MenuPool();

            LeMainMenu = new UIMenu("Communicator");
            LeMainMenu.TitleBackgroundColor = Color.FromArgb(230, 13, 219, 175);
            LeMainMenu.TitleUnderlineColor = Color.FromArgb(255, 255, 255, 255);
            LeMainMenu.HighlightedItemTextColor = Color.Black;
            LeMainMenu.HighlightedBoxColor = Color.FromArgb(140, 13, 219, 175);
            LeMainMenu.DescriptionBoxColor = Color.FromArgb(230, 13, 219, 175);
            LeMainMenu.CalculateMenuPositioning();
            LeMainMenu.UseEventBasedControls = false;

            _menuPool.AddMenu(LeMainMenu);

            foreach (ModAndMenuItemPair pair in ModMenuItems)
            {
                LeMainMenu.AddMenuItem(pair.MenuItem);
            }
        }

        Keys KeyToggle1;
        Keys KeyToggle2;
        Control buttonToggle1;
        Control buttonToggle2;
        Control buttonToggle3;

        void LoadMergerINI(string filepath)
        {
            ScriptSettings config = ScriptSettings.Load(filepath);

            KeyToggle1 = config.GetValue<Keys>("Keyboard Controls", "Menu Toggle Key 1", Keys.F10);
            KeyToggle2 = config.GetValue<Keys>("Keyboard Controls", "Menu Toggle Key 2", Keys.F10);
            buttonToggle1 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 1", Control.VehicleHandbrake);
            buttonToggle2 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 2", Control.VehicleHandbrake);
            buttonToggle3 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 3", Control.VehicleHorn);
        }

        void SaveMergerINI(string filepath)
        {
            if (!File.Exists(filepath))
            {
                ScriptSettings config = ScriptSettings.Load(filepath);

                config.SetValue<Keys>("Keyboard Controls", "Menu Toggle Key 1", Keys.F10);
                config.SetValue<Keys>("Keyboard Controls", "Menu Toggle Key 2", Keys.F10);
                config.SetValue<Control>("Gamepad Controls", "Menu Toggle Button 1", Control.VehicleHandbrake);
                config.SetValue<Control>("Gamepad Controls", "Menu Toggle Button 2", Control.VehicleHandbrake);
                config.SetValue<Control>("Gamepad Controls", "Menu Toggle Button 3", Control.VehicleHorn);
                config.Save();
            }
        }

        EventWaitHandle SCHandle = new EventWaitHandle(true, EventResetMode.ManualReset, "ScriptCommunicator");
        void SendEventToScript(ModAndMenuItemPair pair)
        {
            /*try
            {
                http://stackoverflow.com/questions/6891752/c-sharp-using-assembly-to-call-a-method-within-a-dll
                Assembly sampleAss = Assembly.LoadFrom(SettingsDirectory + pair.ModFileName + ".dll");
                Type myType = sampleAss.GetType(pair.ModFileName + "." + pair.ModFileName);
                if (myType != null)
                {
                    MethodInfo Method = myType.GetMethod("AccessMod");
                    if (Method != null)
                    {
                        object myInstance = Activator.CreateInstance(myType);
                        Method.Invoke(myInstance, null);
                    }
                }
            }
            catch { UI.ShowSubtitle("This mod is not compatible!"); }*/


            try
            {
                /*
                 * http://stackoverflow.com/questions/4123923/synchronizing-2-processes-using-interprocess-synchronizations-objects-mutex-or
                 * http://www.albahari.com/threading/part2.aspx
                 * http://stackoverflow.com/questions/2590334/creating-a-cross-process-eventwaithandle
                */
                pair.WaitHandle = EventWaitHandle.OpenExisting(pair.ModFileName);
                pair.WaitHandle.Set();
            }
            catch { UI.ShowSubtitle("This mod is not compatible!"); }
        }

        bool OpenMenuEventAllowed()
        {
            return SCHandle.WaitOne(0);
        }

        void OnTick(object sender, EventArgs e) // This is where most of your script goes
        {
            if (initialize) //Add a wait in case there are new scmod files created by other scripts.
            {
                Wait(1000);

                SaveMergerINI(@"scripts\ScriptCommunicator.ini");
                LoadMergerINI(@"scripts\ScriptCommunicator.ini");
                CollectCompatibleMods();
                InitMenu();

                initialize = false;
            }

            _menuPool.ProcessMenus();

            while (!OpenMenuEventAllowed())
            {
                Wait(200);
            }

            if (MenuToggled() && OpenMenuEventAllowed())
            {
                if (!_menuPool.IsAnyMenuOpen())
                {
                    _menuPool.LastUsedMenu.IsVisible = !_menuPool.LastUsedMenu.IsVisible;
                }
                else
                {
                    _menuPool.CloseAllMenus();
                }
                PromptInputWait();

            }

            if (_menuPool.IsAnyMenuOpen())
            {
                foreach (ModAndMenuItemPair pair in ModMenuItems)
                {
                    if (LeMainMenu.JustPressedAccept())
                    {
                        UIMenuItem si = LeMainMenu.SelectedItem;

                        if (si == pair.MenuItem)
                        {
                            SendEventToScript(pair);
                            _menuPool.CloseAllMenus();
                            Wait(100);
                        }
                    }
                }
            }
        }

        bool CanReceiveInput()
        {
            return InputTimer <= Game.GameTime;
        }

        void PromptInputWait()
        {
            InputTimer = Game.GameTime + 400;
        }

        bool MenuToggled()
        {
            if (CanReceiveInput())
            {
                if (IsKeyboard() && KeyPressed(KeyToggle1) && KeyPressed(KeyToggle2))
                {
                    return true;
                }
                else if (ControlPressed(buttonToggle1) && ControlPressed(buttonToggle2) && ControlPressed(buttonToggle3))
                {
                    return true;
                }
            }
            return false;
        }

        bool KeyPressed(Keys key)
        {
            return Game.IsKeyPressed(key);
        }

        bool ControlPressed(Control control)
        {
            return Game.IsControlPressed(2, control);
        }

        bool IsKeyboard()
        {
            return Game.CurrentInputMode == InputMode.MouseAndKeyboard;
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
        }
    }

    class ModAndMenuItemPair
    {
        public UIMenuItem MenuItem { get; set; }
        public string ModTitle { get; set; }
        public string ModFileName { get; set; }
        public EventWaitHandle WaitHandle { get; set; }

        public ModAndMenuItemPair(UIMenuItem menuItem, string modtitle, string modfilename, EventWaitHandle handle)
        {
            MenuItem = menuItem;
            ModTitle = modtitle;
            ModFileName = modfilename;
            WaitHandle = handle;

        }
    }

    class ModNameReaderClass
    {

        List<string> AllSettingsWithFullPath = new List<string>();

        public void LoadCompatibleMods(string directory, string searchpattern)
        {
            string[] files = Directory.GetFiles(directory, searchpattern);

            //AllSettingsWithFullPath.Clear();

            AllSettingsWithFullPath.AddRange(files);
        }

        public List<string> GetCleanFileNames()
        {
            List<string> temp = new List<string>();

            foreach (string s in AllSettingsWithFullPath)
            {
                string name = Path.GetFileNameWithoutExtension(s);
                temp.Add(name);
            }

            return temp;
        }

        public List<string> GetFullPathNames()
        {
            return AllSettingsWithFullPath;
        }
    }
}