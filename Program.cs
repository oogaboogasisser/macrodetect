using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

class MacroDetect
{
    static Stopwatch sw = Stopwatch.StartNew();
    static long? lastTs;
    static int count;
    static StreamWriter log;
    static bool run = true, ctrlHeld = false;
    static IntPtr hKb, hMs;
    delegate IntPtr HP(int n, IntPtr w, IntPtr l);

    [DllImport("user32.dll", SetLastError = true, PreserveSig = true)]
    static extern IntPtr SetWindowsHookEx(int id, HP f, IntPtr m, uint t);
    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr h);
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr h, int n, IntPtr w, IntPtr l);
    [DllImport("user32.dll", PreserveSig = true)]
    static extern bool GetMessage(out MSG m, IntPtr h, uint a, uint b);
    [DllImport("user32.dll", PreserveSig = true)]
    static extern bool TranslateMessage(ref MSG m);
    [DllImport("user32.dll", PreserveSig = true)]
    static extern IntPtr DispatchMessage(ref MSG m);

    [StructLayout(LayoutKind.Sequential)]
    struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; }
    [StructLayout(LayoutKind.Sequential)]
    struct KBS { public uint vkCode, scanCode, flags; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct MSS { public uint mouseData, flags; public IntPtr dwExtraInfo; }

    static IntPtr KCb(int n, IntPtr w, IntPtr l)
    {
        if (run && n == 0 && l != IntPtr.Zero)
        {
            int wm = w.ToInt32();
            KBS kb = (KBS)Marshal.PtrToStructure(l, typeof(KBS));
            uint vk = kb.vkCode;
            string nm = VN(vk);
            if (wm == 0x100 || wm == 0x104)
            {
                if (vk == 162 || vk == 163) ctrlHeld = true;
                if (ctrlHeld && (vk == 0x45 || vk == 0x4F))
                    Log("key", nm, "CTRL+" + nm);
                else
                    Log("key", nm);
            }
            else if (wm == 0x101 || wm == 0x105)
            {
                if (vk == 162 || vk == 163) ctrlHeld = false;
            }
        }
        return CallNextHookEx(hKb, n, w, l);
    }

    static IntPtr MCb(int n, IntPtr w, IntPtr l)
    {
        if (!run || n != 0 || l == IntPtr.Zero) return CallNextHookEx(hMs, n, w, l);
        int wm = w.ToInt32();
        if (wm == 0x201) Log("mouse", "LClick");
        else if (wm == 0x204) Log("mouse", "RClick");
        else if (wm == 0x207) Log("mouse", "MClick");
        else if (wm == 0x20B) { int b = (w.ToInt32() >> 16) & 0xFFFF; Log("mouse", b == 1 ? "X1Click" : "X2Click"); }
        else if (wm == 0x20A) { MSS ms = (MSS)Marshal.PtrToStructure(l, typeof(MSS)); Log("mouse", ms.mouseData > 0x7FFFFFFFU ? "ScDn" : "ScUp"); }
        return CallNextHookEx(hMs, n, w, l);
    }

    static void Log(string t, string k)
    {
        long now = sw.ElapsedTicks;
        string d = lastTs == null ? "" : DS(now - lastTs.Value);
        lastTs = now;
        count++;
        log.Write("{0,6:D6},{1},{2},{3},,\n", count, t, k, d);
        log.Flush();
    }
    static void Log(string t, string k, string hit)
    {
        long now = sw.ElapsedTicks;
        string d = lastTs == null ? "" : DS(now - lastTs.Value);
        lastTs = now;
        count++;
        log.Write("{0,6:D6},{1},{2},{3},{4},\n", count, t, k, d, hit);
        log.Flush();
    }

    static string DS(long ticks)
    {
        long u = (long)((double)ticks / Stopwatch.Frequency * 1e6);
        return u.ToString();
    }

    static string VN(uint vk)
    {
        if (vk >= 65 && vk <= 90) return ((char)vk).ToString();
        if (vk >= 48 && vk <= 57) return ((char)vk).ToString();
        if (vk >= 96 && vk <= 105) return "Numpad" + (char)(vk - 66);
        switch (vk)
        {
            case 1: return "LB"; case 2: return "RB"; case 4: return "MB";
            case 8: return "Backsp"; case 9: return "Tab"; case 13: return "Enter";
            case 16: return "Shift"; case 17: return "Ctrl"; case 18: return "Alt";
            case 20: return "Caps"; case 27: return "Esc"; case 32: return "Space";
            case 33: return "PgUp"; case 34: return "PgDn"; case 35: return "End";
            case 36: return "Home"; case 37: return "Left"; case 38: return "Up";
            case 39: return "Right"; case 40: return "Down";
            case 44: return "PrtScr"; case 45: return "Ins"; case 46: return "Del";
            case 112: return "F1"; case 113: return "F2"; case 114: return "F3";
            case 115: return "F4"; case 116: return "F5"; case 117: return "F6";
            case 118: return "F7"; case 119: return "F8"; case 120: return "F9";
            case 121: return "F10"; case 122: return "F11"; case 123: return "F12";
            case 144: return "NumLk";
            case 160: return "LShift"; case 161: return "RShift";
            case 162: return "LCtrl"; case 163: return "RCtrl";
            case 164: return "LAlt"; case 165: return "RAlt";
            default: return "VK_" + vk.ToString("X2");
        }
    }

    static void Main()
    {
        Console.CancelKeyPress += delegate { run = false; };
        string dl = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string lp = Path.Combine(dl, "inputlog_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
        log = new StreamWriter(lp, false, Encoding.UTF8);
        log.WriteLine("count,type,keybutton,delta_us,ANOMALY,HIT");
        log.Flush();
        HP kp = KCb, mp = MCb;
        hKb = SetWindowsHookEx(13, kp, IntPtr.Zero, 0);
        hMs = SetWindowsHookEx(14, mp, IntPtr.Zero, 0);
        MSG m;
        while (run && GetMessage(out m, IntPtr.Zero, 0, 0)) { TranslateMessage(ref m); DispatchMessage(ref m); }
        UnhookWindowsHookEx(hKb);
        UnhookWindowsHookEx(hMs);
        log.Flush();
        log.Close();
    }
}
