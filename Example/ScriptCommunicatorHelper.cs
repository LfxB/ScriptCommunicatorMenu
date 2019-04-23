﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ScriptCommunicatorHelper
{
    class ScriptCommunicator
    {
        EventWaitHandle MainHandle { get; set; }
        EventWaitHandle ScriptCommunicatorModMenuHandle;
        bool ScriptCommunicatorModMenuIsBlocked;

        public ScriptCommunicator(string EventName)
        {
            MainHandle = new EventWaitHandle(false, EventResetMode.ManualReset, EventName);
            ScriptCommunicatorModMenuHandle = new EventWaitHandle(true, EventResetMode.ManualReset, "ScriptCommunicator");
        }

        public void CreateSCModFile(string path = @"scripts\MyDllFilename.scmod", string menuTitle = "My New Script", string menuDescription = "This is a cool script.")
        {
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine(menuTitle);
                    writer.WriteLine(menuDescription);
                }
            }
        }

        public bool IsEventTriggered()
        {
            return MainHandle.WaitOne(0);
        }

        public void TriggerEvent()
        {
            MainHandle.Set();
        }

        public void ResetEvent()
        {
            MainHandle.Reset();
        }

        public void BlockScriptCommunicatorModMenu()
        {
            if (!ScriptCommunicatorModMenuIsBlocked)
            {
                bool waitHandleExists = EventWaitHandle.TryOpenExisting("ScriptCommunicator", out ScriptCommunicatorModMenuHandle);
                if (waitHandleExists) { ScriptCommunicatorModMenuHandle.Reset(); ScriptCommunicatorModMenuIsBlocked = true; }
            }
            else
            {
                ScriptCommunicatorModMenuHandle.Reset();
            }
        }

        public void UnblockScriptCommunicatorModMenu()
        {
            if (ScriptCommunicatorMenuIsBlocked() && ScriptCommunicatorModMenuIsBlocked)
            {
                ScriptCommunicatorModMenuHandle.Set();
                ScriptCommunicatorModMenuIsBlocked = false;
            }
        }

        public bool ScriptCommunicatorMenuIsBlocked()
        {
            return !ScriptCommunicatorModMenuHandle.WaitOne(0);
        }

        private static bool ScriptCommunicatorDllExists;
        public bool ScriptCommunicatorMenuDllExists()
        {
            if (ScriptCommunicatorDllExists)
            {
                return true;
            }
            ScriptCommunicatorDllExists = File.Exists(@"scripts\ScriptCommunicator.dll");
            return ScriptCommunicatorDllExists;
        }
    }
}
