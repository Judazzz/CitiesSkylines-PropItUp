using UnityEngine;

namespace PropItUp
{
    class InputUtils
    {
        public static bool HotkeyPressed()
        {
            bool validInput = false;
            //  Preferred hotkey: [Shift] + [P]:
            if (((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.P)) && PropItUpTool.config.keyboardshortcut == 0)
            {
                validInput = true;
            }
            //  Preferred hotkey: [Ctrl] + [P]:
            if (((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyUp(KeyCode.P)) && PropItUpTool.config.keyboardshortcut == 1)
            {
                validInput = true;
            }
            //  Preferred hotkey: [Alt] + [P]:
            if (((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyUp(KeyCode.P)) && PropItUpTool.config.keyboardshortcut == 2)
            {
                validInput = true;
            }
            return validInput;
        }
    }
}