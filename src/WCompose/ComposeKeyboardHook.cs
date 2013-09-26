using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WCompose
{
    public class ComposeKeyboardHook : KeyboardHook
    {
        private Trie<char, string> _map = new Trie<char, string>();
 
        private Trie<char, string> _current; 

        
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags = 0,
            UIntPtr dwhkl = default(UIntPtr)
            );

        private readonly StringBuilder _keyBuffer = new StringBuilder(10);
        private readonly StringBuilder _buffer = new StringBuilder(10);
        private readonly byte[] _keyState = new byte[256];
        protected override bool TryHandle(EventType eventType, KBDLLHOOKSTRUCT keyCodes)
        {
            // don't try to compose injected keys
            if (keyCodes.flags.HasFlag(KeyFlags.Injected)) return false;

            switch ((Keys) keyCodes.vkCode)
            {
                case Keys.Apps:
                {
                    if (eventType == EventType.KeyDown)
                    {
                        // start to compose!
                        _current = _map;
                    }

                    // ignore up-moves of the Apps key as well or else the menu still pops up    
                    return true;
                }
                case Keys.ShiftKey:
                case Keys.Shift:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    // we have to maintain this ourselves as GetKeyboardState doesn't work in this situation
                    _keyState[16] = eventType == EventType.KeyDown ? (byte)0x80 : (byte)0;
                    return false;
            }


            if (_current != null) // if we're composing
            {
                if (eventType == EventType.KeyDown)
                {
                    // try to convert the keystate to a character
                    var numChars = ToUnicodeEx(keyCodes.vkCode, keyCodes.scanCode, _keyState, _buffer, _buffer.Capacity);
                    if (numChars <= 0)
                        return false;

                    // navigate the trie
                    for (int i = 0; i < numChars && _current != null; ++i)
                    {
                        _current = _current.Step(_buffer[i]);
                        _keyBuffer.Append(_buffer[i]);
                    }

                    // if we have an end state, send it
                    if (_current != null && _current.Value != null)
                    {
                        var value = _current.Value;
                        _keyBuffer.Clear();
                        _current = null;
                        SendKeys.SendWait(EscapeSendKeys(value));
                    }
                        // if we failed to find a match just send all the keys
                    else if (_current == null)
                    {
                        var value = _keyBuffer.ToString();
                        _keyBuffer.Clear();
                        SendKeys.SendWait(EscapeSendKeys(value));
                    }
                }

                // never sent composed keys onwards
                return true;
            }

            return false;
        }

        private readonly StringBuilder sendKeysEscape = new StringBuilder(10);
        private string EscapeSendKeys(string input)
        {
            sendKeysEscape.Clear();
            foreach (var c in input)
            {
                switch (c)
                {
                    case '+':
                    case '%':
                    case '^':
                    case '~':
                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                        sendKeysEscape.Append('{').Append(c).Append('}');
                        break;
                    default:
                        sendKeysEscape.Append(c);
                        break;
                }
            }
            return sendKeysEscape.ToString();
        }

        public void SetTrie(Trie<char, string> trie)
        {
            // reference assignment is threadsafe
            _map = trie;
        }
    }
}