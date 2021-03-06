﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WCompose
{
    public class ComposeKeyboardHook : KeyboardHook
    {
        private Trie<char, string> _map = new Trie<char, string>();
 
        private Trie<char, string> _current;

        private Prompts _prompts = new Prompts();

        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
            public static extern int ToUnicodeEx(
                uint wVirtKey,
                uint wScanCode,
                byte[] lpKeyState,
                StringBuilder pwszBuff,
                int cchBuff,
                uint wFlags = 0,
                UIntPtr dwhkl = default(UIntPtr)
                );
        }

        private readonly StringBuilder _keyBuffer = new StringBuilder(10);
        private readonly StringBuilder _buffer = new StringBuilder(10);
        private readonly byte[] _keyState = new byte[256];
        protected override bool TryHandle(EventType eventType, KBDLLHOOKSTRUCT keyCodes)
        {
            // don't try to compose injected keys
            if (keyCodes.flags.HasFlag(KeyFlags.Injected)) return false;

            switch ((Keys) keyCodes.vkCode)
            {
            case Keys.RMenu: // Right Alt key
            {
                if (eventType == EventType.SysKeyDown)
                {
                    // start to compose!
                    _current = _map;

                    // TODO: can we start this on background thread?
                    // as it seems it can block for a while if we've been paged out,
                    // and we get messed up
                    {
                        UpdatePrompts();
                        _prompts.Show();
                    }

                    // consumed the key
                    return true;
                }
                else
                {
                    // allow key-up events through
                    // this allows us to reset after we hang for a bit
                    // which can happen if we're paged out...
                    return false;
                }
            }
            case Keys.ShiftKey:
            case Keys.Shift:
            case Keys.LShiftKey:
            case Keys.RShiftKey:
                // we have to maintain this ourselves as GetKeyboardState doesn't work in this situation
                _keyState[16] = eventType == EventType.KeyDown ? (byte)0x80 : (byte)0;
                return false;
            }

            if (_current == null)
            {
                // we're not composing
                return false;
            }
            
            if (eventType == EventType.KeyDown)
            {
                // try to convert the keystate to a character
                var numChars = SafeNativeMethods.ToUnicodeEx(
                    keyCodes.vkCode,
                    keyCodes.scanCode,
                    _keyState,
                    _buffer,
                    _buffer.Capacity);

                if (numChars <= 0)
                    return false;

                // navigate the trie
                for (int i = 0; i < numChars && _current != null; ++i)
                {
                    _current = _current.Step(_buffer[i]);
                    _keyBuffer.Append(_buffer[i]);

                    if (_current?.Value != null)
                    {
                        _keyBuffer.Clear();
                        _keyBuffer.Append(_current.Value);
                    }
                }

                // if we have an end state, send it
                if (_current != null && _current.Value != null && !_current.HasNext)
                {
                    _prompts.Hide();

                    var value = _current.Value;
                    _keyBuffer.Clear();
                    _current = null;
                    SendKeys.SendWait(EscapeSendKeys(value));
                }
                // if we failed to find a match send any previous match and keys since then
                else if (_current == null)
                {
                    _prompts.Hide();
                                       
                    var value = _keyBuffer.ToString();
                    _keyBuffer.Clear();
                    
                    SendKeys.SendWait(EscapeSendKeys(value));
                }
                else
                {
                    // otherwise we update the prompts
                    if (_prompts.IsVisible)
                    {
                        UpdatePrompts();
                    }
                }
            }

            // never send composed keys onwards
            return true;
        }

        private void UpdatePrompts()
        {
            var result = new List<Tuple<string, string>>();

            foreach (var key in _current.Keys)
            {
                var next = _current.Step(key);
                result.Add(Tuple.Create(key.ToString(), next.Value));
            }

            result.Sort();
            
            _prompts.CurrentInfo = _keyBuffer.ToString();
            _prompts.SetItems(result);
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
        
        public Trie<char, string> Trie 
        {
            // reference assignment is threadsafe
            get { return _map; }
            set { _map = value; }
        }
    }
}