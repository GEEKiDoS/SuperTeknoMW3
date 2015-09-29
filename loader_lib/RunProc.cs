using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace loader_lib
{
    public class RunProc
    {
        private bool initialized;

        private Mutex mutex;
        private IntPtr thread;

        public string Commandargs { get; set; }
        public string ExecutableName { get; set; }

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

            mutex = new Mutex(false, "TeknoMW3" + (pi.dwProcessId ^ 0x57).ToString("X8"));
            Win32Apis.ResumeThread(pi.hThread);
        }

        public async Task Tick(string DllPath)
        {
            await Task.Delay(3000);
            await Task.Run(() => threadi(DllPath));

            while (true)
            {
                var context = new CONTEXT();
                try
                {
                    if (!Win32Apis.GetThreadContext(this.thread, ref context))
                    {
                        mutex.Close();
                        return;
                    }

                    await Task.Delay(5000);
                }
                catch (Exception)
                {
                    try
                    {
                        mutex.Close();
                        throw;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
