using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loader_updateinstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Delay(3000);

            if (File.Exists("cache\\script.txt"))
            {
                try
                {
                    string lines = File.ReadAllText("cache\\script.txt");
                    foreach (var item in lines.Split(new char[] { ';' }))
                    {
                        var temp = item.TrimStart().Split(new char[] { ' ' }, 3);
                        switch (temp[0])
                        {
                            case "delete":
                                if (File.Exists(temp[1]))
                                {
                                    File.Delete(temp[1]);
                                }
                                Console.WriteLine("删除：" + temp[1]);
                                break;
                            case "install":
                                File.Copy("cache\\" + temp[1], temp[1]);
                                Console.WriteLine("安装：" + temp[1]);
                                break;
                            case "rename":
                                File.Move(temp[1], temp[2]);
                                Console.WriteLine("重命名：" + temp[1] + " 为 " + temp[2]);
                                break;
                            default:
                                break;
                        }
                    }
                    Directory.Delete("cache");
                    Console.WriteLine("已完成更新！");
                    Task.Delay(3000);
                    Process.Start("SuperTeknoMW3.exe");
                }
                catch (Exception)
                {
                    Console.WriteLine("安装更新时发生错误！请前往下载站手动下载更新并安装！");
                    Console.ReadKey();
                    Process.Start("http://bbs.3dmgame.com/thread-5030431-1-1.html");
                }
            }
        }
    }
}
