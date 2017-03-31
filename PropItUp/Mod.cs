using ICities;
using System;

namespace PropItUp
{
    public class Mod : IUserMod
    {
        public const string version = "1.2.2";

        public string Name
        {
            get { return "Prop it Up! " + version; }
        }

        public string Description
        {
            get { return "Customize trees and props in C:SL buildings."; }
        }

        //  Keyboard Shortcut:
        private void OnKeyboardShortcutChanged(int c)
        {
            PropItUpTool.config.keyboardshortcut = c;
            PropItUpTool.SaveConfig();
        }
        //  Toggle Option Runtime Reload:
        private void OnEnableRuntimeReloadChanged(bool c)
        {
            PropItUpTool.config.enable_runtimereload = c;
            PropItUpTool.SaveConfig();
        }

        //  Toggle Option Global Free-standing:
        private void OnEnableGlobalFreestandingChanged(bool c)
        {
            PropItUpTool.config.enable_globalfreestanding = c;
            PropItUpTool.SaveConfig();
        }
        //  Toggle Option Apply Global Tree Replacements on Load:
        private void OnEnableApplyGlobalOnLoadChanged(bool c)
        {
            PropItUpTool.config.enable_applyglobalonload = c;
            PropItUpTool.SaveConfig();
        }
        //  Toggle Option Apply Building Prop/Tree Replacements on Load:
        private void OnEnableApplyBuildingOnLoadChanged(bool c)
        {
            PropItUpTool.config.enable_applybuildingonload = c;
            PropItUpTool.SaveConfig();
        }

        //  Toggle Option Extreme Mode:
        private void OnEnableExtremeModeChanged(bool c)
        {
            PropItUpTool.config.enable_extrememode = c;
            PropItUpTool.SaveConfig();
        }

        //  Toggle Option Debug Output:
        private void OnEnableDebugChanged(bool c)
        {
            PropItUpTool.config.enable_debug = c;
            PropItUpTool.SaveConfig();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                PropItUpTool.LoadConfig();

                //  Mod options:
                UIHelperBase group = helper.AddGroup(Name);
                group.AddSpace(10);
                //  Keyboard Shortcut:
                group.AddDropdown("Select your preferred keyboard shortcut for toggling the mod panel", new[] { "Shift + P", "Ctrl + P", "Alt + P" }, PropItUpTool.config.keyboardshortcut, OnKeyboardShortcutChanged);
                group.AddSpace(10);
                //  Toggle Option Runtime Reload:
                group.AddCheckbox("Replace trees/props at runtime (default: on)", PropItUpTool.config.enable_runtimereload, new OnCheckChanged(OnEnableRuntimeReloadChanged));
                group.AddSpace(15);

                //  Toggle Option Global Free-standing:
                group.AddCheckbox("Enable global free-standing tree replacement feature (default: on)", PropItUpTool.config.enable_globalfreestanding, new OnCheckChanged(OnEnableGlobalFreestandingChanged));
                group.AddSpace(10);
                //  Toggle Option Apply Global Building Tree Replacements on Load:
                group.AddCheckbox("Apply global building tree replacements on load (default: on)", PropItUpTool.config.enable_applyglobalonload, new OnCheckChanged(OnEnableApplyGlobalOnLoadChanged));
                group.AddSpace(5);
                group.AddGroup("WARNING: When disabled, creating new global building tree replacements \n may result in mix-ups and unexpected behavior. So don't!");
                group.AddSpace(10);
                //  Toggle Option Apply Building Prop/Tree Replacements on Load:
                group.AddCheckbox("Apply building prop/tree replacements on load (default: on)", PropItUpTool.config.enable_applybuildingonload, new OnCheckChanged(OnEnableApplyBuildingOnLoadChanged));
                group.AddSpace(5);
                group.AddGroup("WARNING: When disabled, creating new per-building prop/tree replacements \n may result in mix-ups and unexpected behavior. So don't!");
                group.AddSpace(15);

                //  Toggle Option Extreme Mode (no restrictions on allowed props):
                group.AddCheckbox("Enable Extreme Mode to allow untested props (default: off)", PropItUpTool.config.enable_extrememode, new OnCheckChanged(OnEnableExtremeModeChanged));
                group.AddSpace(5);
                group.AddGroup("WARNING: Extreme Mode is a beta feature that is NOT eligeble for support.\nUse this feature at your own risk!");
                group.AddSpace(15);

                //  Toggle Option Debug Output:
                group.AddCheckbox("Write additional data to debug log (default: off)", PropItUpTool.config.enable_debug, new OnCheckChanged(OnEnableDebugChanged));
                group.AddSpace(5);
                group.AddGroup("WARNING: enabling debug data may increase loading times!\nEnabling this setting is only recommended when you experience problems.");
                //group.AddSpace(20);
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);
            }
        }
    }
}