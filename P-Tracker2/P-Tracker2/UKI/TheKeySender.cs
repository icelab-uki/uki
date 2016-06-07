using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using InputManager;

namespace P_Tracker2
{
    //http://www.codeproject.com/Articles/117657/InputManager-library-Track-user-input-and-simulate
    //Thread.Sleep(2000);
    //Keyboard.KeyDown(System.Windows.Forms.Keys.W);
    //Thread.Sleep(100);
    //Keyboard.KeyUp(System.Windows.Forms.Keys.W);
    //Mouse.Move(50, 50);
    //Mouse.PressButton(Mouse.MouseKeys.Left); //Performing a left click

    class keyCode_String
    {
        public String name { get; set; }
        public String def { get; set; }
        public System.Windows.Forms.Keys key { get; set; }
    }

    class TheKeySender
    {
        public static List<keyCode_String> key_list = new List<keyCode_String>
        {
            new keyCode_String{ name = "0" , def = "The 0 key." , key = System.Windows.Forms.Keys.D0 },
            new keyCode_String{ name = "1" , def = "The 1 key." , key = System.Windows.Forms.Keys.D1 },
            new keyCode_String{ name = "2" , def = "The 2 key." , key = System.Windows.Forms.Keys.D2 },
            new keyCode_String{ name = "3" , def = "The 3 key." , key = System.Windows.Forms.Keys.D3 },
            new keyCode_String{ name = "4" , def = "The 4 key." , key = System.Windows.Forms.Keys.D4 },
            new keyCode_String{ name = "5" , def = "The 5 key." , key = System.Windows.Forms.Keys.D5 },
            new keyCode_String{ name = "6" , def = "The 6 key." , key = System.Windows.Forms.Keys.D6 },
            new keyCode_String{ name = "7" , def = "The 7 key." , key = System.Windows.Forms.Keys.D7 },
            new keyCode_String{ name = "8" , def = "The 8 key." , key = System.Windows.Forms.Keys.D8 },
            new keyCode_String{ name = "9" , def = "The 9 key." , key = System.Windows.Forms.Keys.D9 },
            new keyCode_String{ name = "A" , def = "The A key." , key = System.Windows.Forms.Keys.A },
            new keyCode_String{ name = "Add" , def = "The add key." , key = System.Windows.Forms.Keys.Add },
            new keyCode_String{ name = "Alt" , def = "The ALT modifier key." , key = System.Windows.Forms.Keys.Alt },
            new keyCode_String{ name = "Apps" , def = "The application key (Microsoft Natural Keyboard)." , key = System.Windows.Forms.Keys.Apps },
            new keyCode_String{ name = "Attn" , def = "The ATTN key." , key = System.Windows.Forms.Keys.Attn },
            new keyCode_String{ name = "B" , def = "The B key." , key = System.Windows.Forms.Keys.B },
            new keyCode_String{ name = "Back" , def = "The BACKSPACE key." , key = System.Windows.Forms.Keys.Back },
            new keyCode_String{ name = "BrowserBack" , def = "The browser back key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserBack },
            new keyCode_String{ name = "BrowserFavorites" , def = "The browser favorites key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserFavorites },
            new keyCode_String{ name = "BrowserForward" , def = "The browser forward key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserForward },
            new keyCode_String{ name = "BrowserHome" , def = "The browser home key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserHome },
            new keyCode_String{ name = "BrowserRefresh" , def = "The browser refresh key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserRefresh },
            new keyCode_String{ name = "BrowserSearch" , def = "The browser search key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserSearch },
            new keyCode_String{ name = "BrowserStop" , def = "The browser stop key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.BrowserStop },
            new keyCode_String{ name = "C" , def = "The C key." , key = System.Windows.Forms.Keys.C },
            new keyCode_String{ name = "Cancel" , def = "The CANCEL key." , key = System.Windows.Forms.Keys.Cancel },
            new keyCode_String{ name = "Capital" , def = "The CAPS LOCK key." , key = System.Windows.Forms.Keys.Capital },
            new keyCode_String{ name = "CapsLock" , def = "The CAPS LOCK key." , key = System.Windows.Forms.Keys.CapsLock },
            new keyCode_String{ name = "Clear" , def = "The CLEAR key." , key = System.Windows.Forms.Keys.Clear },
            new keyCode_String{ name = "Control" , def = "The CTRL modifier key." , key = System.Windows.Forms.Keys.Control },
            new keyCode_String{ name = "ControlKey" , def = "The CTRL key." , key = System.Windows.Forms.Keys.ControlKey },
            new keyCode_String{ name = "Crsel" , def = "The CRSEL key." , key = System.Windows.Forms.Keys.Crsel },
            new keyCode_String{ name = "D" , def = "The D key." , key = System.Windows.Forms.Keys.D },
            new keyCode_String{ name = "Decimal" , def = "The decimal key." , key = System.Windows.Forms.Keys.Decimal },
            new keyCode_String{ name = "Delete" , def = "The DEL key." , key = System.Windows.Forms.Keys.Delete },
            new keyCode_String{ name = "Divide" , def = "The divide key." , key = System.Windows.Forms.Keys.Divide },
            new keyCode_String{ name = "Down" , def = "The DOWN ARROW key." , key = System.Windows.Forms.Keys.Down },
            new keyCode_String{ name = "E" , def = "The E key." , key = System.Windows.Forms.Keys.E },
            new keyCode_String{ name = "End" , def = "The END key." , key = System.Windows.Forms.Keys.End },
            new keyCode_String{ name = "Enter" , def = "The ENTER key." , key = System.Windows.Forms.Keys.Enter },
            new keyCode_String{ name = "EraseEof" , def = "The ERASE EOF key." , key = System.Windows.Forms.Keys.EraseEof },
            new keyCode_String{ name = "Escape" , def = "The ESC key." , key = System.Windows.Forms.Keys.Escape },
            new keyCode_String{ name = "Execute" , def = "The EXECUTE key." , key = System.Windows.Forms.Keys.Execute },
            new keyCode_String{ name = "Exsel" , def = "The EXSEL key." , key = System.Windows.Forms.Keys.Exsel },
            new keyCode_String{ name = "F" , def = "The F key." , key = System.Windows.Forms.Keys.F },
            new keyCode_String{ name = "F1" , def = "The F1 key." , key = System.Windows.Forms.Keys.F1 },
            new keyCode_String{ name = "F10" , def = "The F10 key." , key = System.Windows.Forms.Keys.F10 },
            new keyCode_String{ name = "F11" , def = "The F11 key." , key = System.Windows.Forms.Keys.F11 },
            new keyCode_String{ name = "F12" , def = "The F12 key." , key = System.Windows.Forms.Keys.F12 },
            new keyCode_String{ name = "F13" , def = "The F13 key." , key = System.Windows.Forms.Keys.F13 },
            new keyCode_String{ name = "F14" , def = "The F14 key." , key = System.Windows.Forms.Keys.F14 },
            new keyCode_String{ name = "F15" , def = "The F15 key." , key = System.Windows.Forms.Keys.F15 },
            new keyCode_String{ name = "F16" , def = "The F16 key." , key = System.Windows.Forms.Keys.F16 },
            new keyCode_String{ name = "F17" , def = "The F17 key." , key = System.Windows.Forms.Keys.F17 },
            new keyCode_String{ name = "F18" , def = "The F18 key." , key = System.Windows.Forms.Keys.F18 },
            new keyCode_String{ name = "F19" , def = "The F19 key." , key = System.Windows.Forms.Keys.F19 },
            new keyCode_String{ name = "F2" , def = "The F2 key." , key = System.Windows.Forms.Keys.F2 },
            new keyCode_String{ name = "F20" , def = "The F20 key." , key = System.Windows.Forms.Keys.F20 },
            new keyCode_String{ name = "F21" , def = "The F21 key." , key = System.Windows.Forms.Keys.F21 },
            new keyCode_String{ name = "F22" , def = "The F22 key." , key = System.Windows.Forms.Keys.F22 },
            new keyCode_String{ name = "F23" , def = "The F23 key." , key = System.Windows.Forms.Keys.F23 },
            new keyCode_String{ name = "F24" , def = "The F24 key." , key = System.Windows.Forms.Keys.F24 },
            new keyCode_String{ name = "F3" , def = "The F3 key." , key = System.Windows.Forms.Keys.F3 },
            new keyCode_String{ name = "F4" , def = "The F4 key." , key = System.Windows.Forms.Keys.F4 },
            new keyCode_String{ name = "F5" , def = "The F5 key." , key = System.Windows.Forms.Keys.F5 },
            new keyCode_String{ name = "F6" , def = "The F6 key." , key = System.Windows.Forms.Keys.F6 },
            new keyCode_String{ name = "F7" , def = "The F7 key." , key = System.Windows.Forms.Keys.F7 },
            new keyCode_String{ name = "F8" , def = "The F8 key." , key = System.Windows.Forms.Keys.F8 },
            new keyCode_String{ name = "F9" , def = "The F9 key." , key = System.Windows.Forms.Keys.F9 },
            new keyCode_String{ name = "FinalMode" , def = "The IME final mode key." , key = System.Windows.Forms.Keys.FinalMode },
            new keyCode_String{ name = "G" , def = "The G key." , key = System.Windows.Forms.Keys.G },
            new keyCode_String{ name = "H" , def = "The H key." , key = System.Windows.Forms.Keys.H },
            new keyCode_String{ name = "HanguelMode" , def = "The IME Hanguel mode key. (maintained for compatibility; use HangulMode)" , key = System.Windows.Forms.Keys.HanguelMode },
            new keyCode_String{ name = "HangulMode" , def = "The IME Hangul mode key." , key = System.Windows.Forms.Keys.HangulMode },
            new keyCode_String{ name = "HanjaMode" , def = "The IME Hanja mode key." , key = System.Windows.Forms.Keys.HanjaMode },
            new keyCode_String{ name = "Help" , def = "The HELP key." , key = System.Windows.Forms.Keys.Help },
            new keyCode_String{ name = "Home" , def = "The HOME key." , key = System.Windows.Forms.Keys.Home },
            new keyCode_String{ name = "I" , def = "The I key." , key = System.Windows.Forms.Keys.I },
            new keyCode_String{ name = "IMEAccept" , def = "The IME accept key, replaces IMEAceept." , key = System.Windows.Forms.Keys.IMEAccept },
            new keyCode_String{ name = "IMEAceept" , def = "The IME accept key. Obsolete, use IMEAccept instead." , key = System.Windows.Forms.Keys.IMEAceept },
            new keyCode_String{ name = "IMEConvert" , def = "The IME convert key." , key = System.Windows.Forms.Keys.IMEConvert },
            new keyCode_String{ name = "IMEModeChange" , def = "The IME mode change key." , key = System.Windows.Forms.Keys.IMEModeChange },
            new keyCode_String{ name = "IMENonconvert" , def = "The IME nonconvert key." , key = System.Windows.Forms.Keys.IMENonconvert },
            new keyCode_String{ name = "Insert" , def = "The INS key." , key = System.Windows.Forms.Keys.Insert },
            new keyCode_String{ name = "J" , def = "The J key." , key = System.Windows.Forms.Keys.J },
            new keyCode_String{ name = "JunjaMode" , def = "The IME Junja mode key." , key = System.Windows.Forms.Keys.JunjaMode },
            new keyCode_String{ name = "K" , def = "The K key." , key = System.Windows.Forms.Keys.K },
            new keyCode_String{ name = "KanaMode" , def = "The IME Kana mode key." , key = System.Windows.Forms.Keys.KanaMode },
            new keyCode_String{ name = "KanjiMode" , def = "The IME Kanji mode key." , key = System.Windows.Forms.Keys.KanjiMode },
            new keyCode_String{ name = "KeyCode" , def = "The bitmask to extract a key code from a key value." , key = System.Windows.Forms.Keys.KeyCode },
            new keyCode_String{ name = "L" , def = "The L key." , key = System.Windows.Forms.Keys.L },
            new keyCode_String{ name = "LaunchApplication1" , def = "The start application one key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.LaunchApplication1 },
            new keyCode_String{ name = "LaunchApplication2" , def = "The start application two key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.LaunchApplication2 },
            new keyCode_String{ name = "LaunchMail" , def = "The launch mail key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.LaunchMail },
            new keyCode_String{ name = "LButton" , def = "The left mouse button." , key = System.Windows.Forms.Keys.LButton },
            new keyCode_String{ name = "LControlKey" , def = "The left CTRL key." , key = System.Windows.Forms.Keys.LControlKey },
            new keyCode_String{ name = "Left" , def = "The LEFT ARROW key." , key = System.Windows.Forms.Keys.Left },
            new keyCode_String{ name = "LineFeed" , def = "The LINEFEED key." , key = System.Windows.Forms.Keys.LineFeed },
            new keyCode_String{ name = "LMenu" , def = "The left ALT key." , key = System.Windows.Forms.Keys.LMenu },
            new keyCode_String{ name = "LShiftKey" , def = "The left SHIFT key." , key = System.Windows.Forms.Keys.LShiftKey },
            new keyCode_String{ name = "LWin" , def = "The left Windows logo key (Microsoft Natural Keyboard)." , key = System.Windows.Forms.Keys.LWin },
            new keyCode_String{ name = "M" , def = "The M key." , key = System.Windows.Forms.Keys.M },
            new keyCode_String{ name = "MButton" , def = "The middle mouse button (three-button mouse)." , key = System.Windows.Forms.Keys.MButton },
            new keyCode_String{ name = "MediaNextTrack" , def = "The media next track key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.MediaNextTrack },
            new keyCode_String{ name = "MediaPlayPause" , def = "The media play pause key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.MediaPlayPause },
            new keyCode_String{ name = "MediaPreviousTrack" , def = "The media previous track key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.MediaPreviousTrack },
            new keyCode_String{ name = "MediaStop" , def = "The media Stop key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.MediaStop },
            new keyCode_String{ name = "Menu" , def = "The ALT key." , key = System.Windows.Forms.Keys.Menu },
            new keyCode_String{ name = "Modifiers" , def = "The bitmask to extract modifiers from a key value." , key = System.Windows.Forms.Keys.Modifiers },
            new keyCode_String{ name = "Multiply" , def = "The multiply key." , key = System.Windows.Forms.Keys.Multiply },
            new keyCode_String{ name = "N" , def = "The N key." , key = System.Windows.Forms.Keys.N },
            new keyCode_String{ name = "Next" , def = "The PAGE DOWN key." , key = System.Windows.Forms.Keys.Next },
            new keyCode_String{ name = "NoName" , def = "A constant reserved for future use." , key = System.Windows.Forms.Keys.NoName },
            new keyCode_String{ name = "None" , def = "No key pressed." , key = System.Windows.Forms.Keys.None },
            new keyCode_String{ name = "NumLock" , def = "The NUM LOCK key." , key = System.Windows.Forms.Keys.NumLock },
            new keyCode_String{ name = "NumPad0" , def = "The 0 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad0 },
            new keyCode_String{ name = "NumPad1" , def = "The 1 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad1 },
            new keyCode_String{ name = "NumPad2" , def = "The 2 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad2 },
            new keyCode_String{ name = "NumPad3" , def = "The 3 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad3 },
            new keyCode_String{ name = "NumPad4" , def = "The 4 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad4 },
            new keyCode_String{ name = "NumPad5" , def = "The 5 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad5 },
            new keyCode_String{ name = "NumPad6" , def = "The 6 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad6 },
            new keyCode_String{ name = "NumPad7" , def = "The 7 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad7 },
            new keyCode_String{ name = "NumPad8" , def = "The 8 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad8 },
            new keyCode_String{ name = "NumPad9" , def = "The 9 key on the numeric keypad." , key = System.Windows.Forms.Keys.NumPad9 },
            new keyCode_String{ name = "O" , def = "The O key." , key = System.Windows.Forms.Keys.O },
            new keyCode_String{ name = "Oem1" , def = "The OEM 1 key." , key = System.Windows.Forms.Keys.Oem1 },
            new keyCode_String{ name = "Oem102" , def = "The OEM 102 key." , key = System.Windows.Forms.Keys.Oem102 },
            new keyCode_String{ name = "Oem2" , def = "The OEM 2 key." , key = System.Windows.Forms.Keys.Oem2 },
            new keyCode_String{ name = "Oem3" , def = "The OEM 3 key." , key = System.Windows.Forms.Keys.Oem3 },
            new keyCode_String{ name = "Oem4" , def = "The OEM 4 key." , key = System.Windows.Forms.Keys.Oem4 },
            new keyCode_String{ name = "Oem5" , def = "The OEM 5 key." , key = System.Windows.Forms.Keys.Oem5 },
            new keyCode_String{ name = "Oem6" , def = "The OEM 6 key." , key = System.Windows.Forms.Keys.Oem6 },
            new keyCode_String{ name = "Oem7" , def = "The OEM 7 key." , key = System.Windows.Forms.Keys.Oem7 },
            new keyCode_String{ name = "Oem8" , def = "The OEM 8 key." , key = System.Windows.Forms.Keys.Oem8 },
            new keyCode_String{ name = "OemBackslash" , def = "The OEM angle bracket or backslash key on the RT 102 key keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemBackslash },
            new keyCode_String{ name = "OemClear" , def = "The CLEAR key." , key = System.Windows.Forms.Keys.OemClear },
            new keyCode_String{ name = "OemCloseBrackets" , def = "The OEM close bracket key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemCloseBrackets },
            new keyCode_String{ name = "Oemcomma" , def = "The OEM comma key on any country/region keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.Oemcomma },
            new keyCode_String{ name = "OemMinus" , def = "The OEM minus key on any country/region keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemMinus },
            new keyCode_String{ name = "OemOpenBrackets" , def = "The OEM open bracket key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemOpenBrackets },
            new keyCode_String{ name = "OemPeriod" , def = "The OEM period key on any country/region keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemPeriod },
            new keyCode_String{ name = "OemPipe" , def = "The OEM pipe key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemPipe },
            new keyCode_String{ name = "Oemplus" , def = "The OEM plus key on any country/region keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.Oemplus },
            new keyCode_String{ name = "OemQuestion" , def = "The OEM question mark key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemQuestion },
            new keyCode_String{ name = "OemQuotes" , def = "The OEM singled/double quote key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemQuotes },
            new keyCode_String{ name = "OemSemicolon" , def = "The OEM Semicolon key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.OemSemicolon },
            new keyCode_String{ name = "Oemtilde" , def = "The OEM tilde key on a US standard keyboard (Windows 2000 or later)." , key = System.Windows.Forms.Keys.Oemtilde },
            new keyCode_String{ name = "P" , def = "The P key." , key = System.Windows.Forms.Keys.P },
            new keyCode_String{ name = "Pa1" , def = "The PA1 key." , key = System.Windows.Forms.Keys.Pa1 },
            new keyCode_String{ name = "Packet" , def = "Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods." , key = System.Windows.Forms.Keys.Packet },
            new keyCode_String{ name = "PageDown" , def = "The PAGE DOWN key." , key = System.Windows.Forms.Keys.PageDown },
            new keyCode_String{ name = "PageUp" , def = "The PAGE UP key." , key = System.Windows.Forms.Keys.PageUp },
            new keyCode_String{ name = "Pause" , def = "The PAUSE key." , key = System.Windows.Forms.Keys.Pause },
            new keyCode_String{ name = "Play" , def = "The PLAY key." , key = System.Windows.Forms.Keys.Play },
            new keyCode_String{ name = "Print" , def = "The PRINT key." , key = System.Windows.Forms.Keys.Print },
            new keyCode_String{ name = "PrintScreen" , def = "The PRINT SCREEN key." , key = System.Windows.Forms.Keys.PrintScreen },
            new keyCode_String{ name = "Prior" , def = "The PAGE UP key." , key = System.Windows.Forms.Keys.Prior },
            new keyCode_String{ name = "ProcessKey" , def = "The PROCESS KEY key." , key = System.Windows.Forms.Keys.ProcessKey },
            new keyCode_String{ name = "Q" , def = "The Q key." , key = System.Windows.Forms.Keys.Q },
            new keyCode_String{ name = "R" , def = "The R key." , key = System.Windows.Forms.Keys.R },
            new keyCode_String{ name = "RButton" , def = "The right mouse button." , key = System.Windows.Forms.Keys.RButton },
            new keyCode_String{ name = "RControlKey" , def = "The right CTRL key." , key = System.Windows.Forms.Keys.RControlKey },
            new keyCode_String{ name = "Return" , def = "The RETURN key." , key = System.Windows.Forms.Keys.Return },
            new keyCode_String{ name = "Right" , def = "The RIGHT ARROW key." , key = System.Windows.Forms.Keys.Right },
            new keyCode_String{ name = "RMenu" , def = "The right ALT key." , key = System.Windows.Forms.Keys.RMenu },
            new keyCode_String{ name = "RShiftKey" , def = "The right SHIFT key." , key = System.Windows.Forms.Keys.RShiftKey },
            new keyCode_String{ name = "RWin" , def = "The right Windows logo key (Microsoft Natural Keyboard)." , key = System.Windows.Forms.Keys.RWin },
            new keyCode_String{ name = "S" , def = "The S key." , key = System.Windows.Forms.Keys.S },
            new keyCode_String{ name = "Scroll" , def = "The SCROLL LOCK key." , key = System.Windows.Forms.Keys.Scroll },
            new keyCode_String{ name = "Select" , def = "The SELECT key." , key = System.Windows.Forms.Keys.Select },
            new keyCode_String{ name = "SelectMedia" , def = "The select media key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.SelectMedia },
            new keyCode_String{ name = "Separator" , def = "The separator key." , key = System.Windows.Forms.Keys.Separator },
            new keyCode_String{ name = "Shift" , def = "The SHIFT modifier key." , key = System.Windows.Forms.Keys.Shift },
            new keyCode_String{ name = "ShiftKey" , def = "The SHIFT key." , key = System.Windows.Forms.Keys.ShiftKey },
            new keyCode_String{ name = "Sleep" , def = "The computer sleep key." , key = System.Windows.Forms.Keys.Sleep },
            new keyCode_String{ name = "Snapshot" , def = "The PRINT SCREEN key." , key = System.Windows.Forms.Keys.Snapshot },
            new keyCode_String{ name = "Space" , def = "The SPACEBAR key." , key = System.Windows.Forms.Keys.Space },
            new keyCode_String{ name = "Subtract" , def = "The subtract key." , key = System.Windows.Forms.Keys.Subtract },
            new keyCode_String{ name = "T" , def = "The T key." , key = System.Windows.Forms.Keys.T },
            new keyCode_String{ name = "Tab" , def = "The TAB key." , key = System.Windows.Forms.Keys.Tab },
            new keyCode_String{ name = "U" , def = "The U key." , key = System.Windows.Forms.Keys.U },
            new keyCode_String{ name = "Up" , def = "The UP ARROW key." , key = System.Windows.Forms.Keys.Up },
            new keyCode_String{ name = "V" , def = "The V key." , key = System.Windows.Forms.Keys.V },
            new keyCode_String{ name = "VolumeDown" , def = "The volume down key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.VolumeDown },
            new keyCode_String{ name = "VolumeMute" , def = "The volume mute key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.VolumeMute },
            new keyCode_String{ name = "VolumeUp" , def = "The volume up key (Windows 2000 or later)." , key = System.Windows.Forms.Keys.VolumeUp },
            new keyCode_String{ name = "W" , def = "The W key." , key = System.Windows.Forms.Keys.W },
            new keyCode_String{ name = "X" , def = "The X key." , key = System.Windows.Forms.Keys.X },
            new keyCode_String{ name = "XButton1" , def = "The first x mouse button (five-button mouse)." , key = System.Windows.Forms.Keys.XButton1 },
            new keyCode_String{ name = "XButton2" , def = "The second x mouse button (five-button mouse)." , key = System.Windows.Forms.Keys.XButton2 },
            new keyCode_String{ name = "Y" , def = "The Y key." , key = System.Windows.Forms.Keys.Y },
            new keyCode_String{ name = "Z" , def = "The Z key." , key = System.Windows.Forms.Keys.Z },
            new keyCode_String{ name = "Zoom" , def = "The ZOOM ke" , key = System.Windows.Forms.Keys.Zoom },
        };

        public static List<String> key_list_basicOnly = new List<String>
        {
            "Left","Up","Right","Down", 
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S", 
            "T","U","V","W","X","Y","Z", 
            "0","1","2","3","4","5","6","7","8","9", 
            "NumPad0","NumPad1","NumPad2","NumPad3","NumPad4","NumPad5", 
            "NumPad6","NumPad7","NumPad8","NumPad9", 
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10", 
            "F11","F12","F13","F14","F15","F16","F17","F18","F19","F20","F21","F22","F23","F24", 
            "LShiftKey","RShiftKey","LControlKey","RControlKey","Shift","Control","Alt" ,
            "LButton","RButton","Back","Tab","Enter","ShiftKey","ControlKey", 
            "CapsLock","Escape","Space"
        };

        public static List<String> key_list_specialOnly = new List<String>
        {
            "Multiply", "Add", "Separator", "Subtract", "Decimal", "Divide", "NumLock", "Scroll", 
            "Modifiers", "None", "Cancel", "MButton", "XButton1", "XButton2", 
            "LineFeed", "Clear", "Return", "Menu", "Pause", "Capital", 
            "KanaMode", "HanguelMode", "HangulMode", "JunjaMode", "FinalMode", 
            "KanjiMode", "HanjaMode", "IMEConvert", "IMENonconvert", "IMEAceept", "IMEAccept", "IMEModeChange", 
            "Prior", "PageUp", "Next", "PageDown", "End", "Home", "Select", "Print", "Execute", "PrintScreen", 
            "Snapshot", "Insert", "Delete", "Help", "LWin", "RWin", "Apps", "Sleep", 
            "LMenu", "RMenu", 
            "BrowserBack", "BrowserForward", "BrowserRefresh", "BrowserStop", "BrowserSearch", 
            "BrowserFavorites", "BrowserHome", "VolumeMute", "VolumeDown", "VolumeUp", 
            "MediaNextTrack", "MediaPreviousTrack", "MediaStop", "MediaPlayPause", "LaunchMail", 
            "SelectMedia", "LaunchApplication1", "LaunchApplication2", "Oem1", "OemSemicolon", 
            "Oemplus", "Oemcomma", "OemMinus", "OemPeriod", "OemQuestion", "Oem2", "Oemtilde", "Oem3", "Oem4", 
            "OemOpenBrackets", "OemPipe", "Oem5", "Oem6", "OemCloseBrackets", "Oem7", 
            "OemQuotes", "Oem8", "Oem102", "OemBackslash", "ProcessKey", 
            "Packet", "Attn", "Crsel", "Exsel", "EraseEof", "Play", "Zoom", "NoName", "Pa1", "OemClear", "KeyCode"
        };

        public static Keys getKey(String s)
        {
            Keys k = Keys.Space;//default
            foreach (keyCode_String a in key_list)
            {
                if (String.Equals(a.name, s)) { k = a.key; break; }
            }
            return k;
        }

        public static String getKeyDef(String s)
        {
            String k = "";//default
            foreach (keyCode_String a in key_list)
            {
                if (String.Equals(a.name, s)) { k = a.def; break; }
            }
            return k;
        }


        //-------------------------------------------------------
        public static int keyPress_delay = 10;

        //Should be run in thread because it sleep
        public static void KeyPress(Keys k)
        {
            Keyboard.KeyDown(k);
            Thread.Sleep(keyPress_delay);
            Keyboard.KeyUp(k);
        }

        public static void KeyPress(String s)
        {
            KeyPress(getKey(s));
        }


    }

    

    
}
