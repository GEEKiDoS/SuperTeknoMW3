using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace loader_lib
{
    public class RunProc
    {
        private IntPtr thread;
        private PROCESS_INFORMATION pi;

        public string Commandargs { get; set; }
        public string ExecutableName { get; set; }
        public string MPClantag { get; set; }
        public string MPTitle { get; set; }
        public bool MPUnlockAll { get; set; }

        public static int BytesToInt(byte[] input)
        {
            string s = input[3].ToString("X2") + input[2].ToString("X2") + input[1].ToString("X2")
                       + input[0].ToString("X2");
            return int.Parse(s, NumberStyles.HexNumber);
        }

        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
            {
                return -1;
            }

            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int Round1000(int a)
        {
            return (a / 0x1000 + ((a % 0x1000) > 0 ? 1 : 0)) * 0x1000;
        }

        private void threadi(string dllpath)
        {
            var si = new STARTUPINFO();
            var pi = new PROCESS_INFORMATION();

            byte[] array = File.ReadAllBytes(ExecutableName);

            var peptr = new byte[4];

            Array.Copy(array, 0x3C, peptr, 0, 2);

            var temparray = new byte[4];

            int peLocation = BytesToInt(peptr);

            int virtualSize = peLocation + 0x128;

            int virtualAddress = peLocation + 0x12C;
            int rawSize = peLocation + 0x130;
            int rawLocation = peLocation + 0x134;

            Array.Copy(array, rawSize, temparray, 0, 4);
            rawSize = BytesToInt(temparray);
            Array.Copy(array, rawLocation, temparray, 0, 4);
            rawLocation = BytesToInt(temparray);
            Array.Copy(array, virtualAddress, temparray, 0, 4);
            virtualAddress = BytesToInt(temparray) + 0x400000;
            Array.Copy(array, virtualSize, temparray, 0, 4);
            virtualSize = Round1000(BytesToInt(temparray));

            int steamapi = IndexOf(array, Encoding.ASCII.GetBytes("steam_api.dll"));

            if (steamapi < rawLocation && steamapi > rawLocation + rawSize)
            {
                throw new Exception("未找到steam_api.dll！请确认你的游戏根目录是否存在此文件！");
            }

            int location = steamapi - rawLocation;

            if (
                !Win32Apis.CreateProcess(
                    ExecutableName,
                    Commandargs,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0x4 | 0x200,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi))
            {
                throw new Exception("创建进程失败！");
            }
            else
            {
                this.pi = pi;
            }

            thread = pi.hThread;
            uint oldprot;
            Win32Apis.VirtualProtectEx(
                pi.hProcess, new IntPtr(virtualAddress), new UIntPtr((uint)virtualSize), 0x40, out oldprot);

            UIntPtr ptr;
            Win32Apis.WriteProcessMemory(
                pi.hProcess, new IntPtr(virtualAddress + location), Encoding.ASCII.GetBytes(dllpath), (uint)dllpath.Length + 1, out ptr);

            uint newprot;
            Win32Apis.VirtualProtectEx(
                pi.hProcess, new IntPtr(virtualAddress), new UIntPtr((uint)virtualSize), oldprot, out newprot);

            if (ptr == (UIntPtr)0)
            {
                Win32Apis.TerminateProcess(pi.hProcess, 0);
                Win32Apis.TerminateThread(pi.hThread, 0);
                throw new Exception("无法修改进程内存！");
            }
            Win32Apis.ResumeThread(pi.hThread);
        }

        public async Task Tick(string DllPath)
        {
            await Task.Delay(3000);
            await Task.Factory.StartNew(() => threadi(DllPath));
            if (ExecutableName == "iw5mp.exe")
            {
                await Task.Delay(5000);
                await Task.Factory.StartNew(() =>
                {
                    var pi = Win32Apis.OpenProcess(0x40 | 0x20 | 8, true, (int)this.pi.dwProcessId);

                    if (!string.IsNullOrWhiteSpace(MPClantag))
                    {
                        UIntPtr clantagptr;
                        Win32Apis.WriteProcessMemory(pi, new IntPtr(0x1328d54), new byte[8], 8, out clantagptr);
                        if (clantagptr != (UIntPtr)0)
                        {
                            Win32Apis.WriteProcessMemory(pi, new IntPtr(0x1328d54), Encoding.ASCII.GetBytes(MPClantag), (uint)Encoding.ASCII.GetBytes(MPClantag).Length, out clantagptr);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(MPTitle))
                    {
                        UIntPtr titleptr;
                        Win32Apis.WriteProcessMemory(pi, new IntPtr(0x1328d35), new byte[25], 25, out titleptr);
                        if (titleptr != (UIntPtr)0)
                        {
                            UIntPtr titleptr2;
                            Win32Apis.WriteProcessMemory(pi, new IntPtr(0x1328d34), new byte[] { 0xff }, 1, out titleptr2);
                            Win32Apis.WriteProcessMemory(pi, new IntPtr(0x1328d35), Encoding.ASCII.GetBytes(MPTitle), (uint)Encoding.ASCII.GetBytes(MPTitle).Length, out titleptr);
                        }
                    }
                    if (MPUnlockAll)
                    {
                        IntPtr rank = (IntPtr)0x1cdba54;
                        IntPtr prestige = rank + 0x210;
                        IntPtr tokens = rank + 0x206f;
                        IntPtr perkspointer = rank + 0xf9a;
                        IntPtr classpointer = rank + 0x2077;

                        UIntPtr unlockallptr;
                        WriteInt(pi, rank, 0x19dfd4, 4, out unlockallptr);
                        WriteInt(pi, prestige, 20, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x2c, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x58, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x20, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x80, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x74, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x7c, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xb0, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xd8, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 20, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 40, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x30, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x44, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x24, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x38, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x34, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x40, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 8, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x18, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x10, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x48, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x4c, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x5c, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 100, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 160, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xac, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xb0, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xa8, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xa4, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x98, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x9c, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x94, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x90, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 140, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 60, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 80, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x54, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 180, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x60, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x68, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x6c, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 12, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x1c, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 120, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x84, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x88, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x100, 0x2bd91, 4, out unlockallptr);
                        WriteInt(pi, rank + 220, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xe4, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0xbc, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, rank + 0x10c, 0x2f44, 4, out unlockallptr);
                        WriteInt(pi, perkspointer, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 1, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 2, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 3, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 4, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 5, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 6, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 7, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 8, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 9, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 10, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 11, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 12, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 13, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, perkspointer + 14, 0x7070707, 4, out unlockallptr);
                        WriteInt(pi, classpointer, 10, 4, out unlockallptr);
                    }

                    string str = "^5SuperTeknoMW3 1.1.5 \n^7By A2ON";
                    
                    UIntPtr outptr;
                    Win32Apis.WriteProcessMemory(pi, (IntPtr)0x1004cb18, Encoding.ASCII.GetBytes(str), (uint)Encoding.ASCII.GetBytes(str).Length, out outptr); //Not Working

                    Win32Apis.CloseHandle(pi);
                });
            }

            while (true)
            {
                var context = new CONTEXT();
                try
                {
                    if (!Win32Apis.GetThreadContext(thread, ref context))
                    {
                        return;
                    }

                    await Task.Delay(5000);
                }
                catch (Exception)
                {
                    try
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        private void WriteInt(IntPtr pi, IntPtr pointer, int value, uint size, out UIntPtr outptr)
        {
            int v = value;
            Win32Apis.WriteProcessMemory(pi, pointer, ref value, size, out outptr);
        }
    }
}
