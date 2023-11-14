using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clipboard_LLM
{
    public partial class Form_Main : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_X = 0x58;
        private const int KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int HOTKEY_ID = 31197; //Any number to use to identify the hotkey instance

        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        const int WM_HOTKEY = 0x0312;
        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case WM_HOTKEY:
                    Keys key = (Keys)(((int)message.LParam >> 16) & 0xFFFF);
                    KeyModifiers modifier = (KeyModifiers)((int)message.LParam & 0xFFFF);

                    if (modifier == KeyModifiers.Alt && key == Keys.C)
                    {
                        generate();
                    }
                    break;
            }
            base.WndProc(ref message);
        }

        static string[] ExtractStrings(string input)
        {
            Regex regex = new Regex(@"`{3}(\w+)`{3}");
            MatchCollection matches = regex.Matches(input);
            string[] results = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                results[i] = matches[i].Groups[1].Value;
            }
            return results;
        }

        static string ExtractCode(string input)
        {
            int startIndex = input.IndexOf("```");
            if (startIndex == -1) return string.Empty;

            startIndex += 3; // Move past the opening "```"
            int endIndex = input.IndexOf("```", startIndex);
            if (endIndex == -1) return string.Empty;

            return input.Substring(startIndex, endIndex - startIndex).Trim();
        }

        async void generate()
        {
            notifyIcon1.Icon = Properties.Resources.icon_nocolor;
            string text = Clipboard.GetText();
            Debug.WriteLine($"Clipboard text: {text}");
            string lang = "en";
            try
            {
                lang = Translator.detectLangs(text);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return;
            }            

            Debug.WriteLine($"{lang}: {text}");

            if (lang == "id")
            {
                Debug.WriteLine("올바르지 않은 언어");
                return;
            }
            else if (lang != "en")
            {
                try {
                    text = Translator.translate(text, lang);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    return;
                }
                Debug.WriteLine($"translated: {text}");
            }
            try
            {
                string answer = await Palm2.GetAnswer(text);
                Debug.WriteLine($"Answer {answer}");
                string code = ExtractCode(answer);
                if(code.Length > 0)
                    answer.Replace(code, "");
                try
                {
                    answer = Translator.translate(answer, "en", "ko");
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                Debug.WriteLine($"Answer {answer}");
                Clipboard.SetText(answer);
                if (code.Length > 0)
                {
                    Debug.WriteLine($"Code: {code}");
                    Clipboard.SetText(code);
                }
                notifyIcon1.Icon = Properties.Resources.icon;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return;
            }           
        }

        public Form_Main()
        {
            InitializeComponent();

            RegisterHotKey(this.Handle, 0, KeyModifiers.Alt, Keys.C);
            this.Visible = false;
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 질문ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            generate();
        }
    }
}
