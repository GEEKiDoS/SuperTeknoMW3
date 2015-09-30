﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using loader_lib;

namespace loader_ui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Proflie profile;
        private Random rng = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(profile);
            settings.ShowDialog();
            UpdateProfile();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DisableAll();
            await Task.Delay(1000);
            try
            {
                LoadProfile();
            }
            catch (Exception)
            {
                MessageBox.Show("无法载入配置文件！请检查是否有足够的权限读取和修改此文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            label.Content = "检查更新...";
            try
            {
                string version = "1.0.0";
                string newversion = await CheckUpdate.GetVersion();

                if (!version.Equals(newversion))
                {
                    MessageBoxResult result = MessageBox.Show("检测到新版本：" + newversion + "！你是否想下载更新？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start("http://bbs.3dmgame.com/thread-4461802-1-1.html");
                        Close();
                    }
                    else
                    {
                        label.Content = "我们建议你及时升级新版本以获得最佳体验！";
                        probar.IsIndeterminate = false;
                    }
                }
                else
                {
                    label.Content = "就绪";
                    probar.IsIndeterminate = false;
                }
            }
            catch (Exception ex)
            {
                label.Content = "检查更新失败:" + ex.Message;
                probar.IsIndeterminate = false;
            }
            finally
            {
                EnableAll();
            }
        }

        private void LoadProfile()
        {
            try
            {
                if (File.Exists("teknogods.ini"))
                {
                    bool isChanged = false;
                    IniParser ini = new IniParser("teknogods.ini");
                    profile = new Proflie();

                    profile.Name = ini.GetSetting("Settings", "Name");
                    if ((string.IsNullOrEmpty(profile.Name) || string.IsNullOrWhiteSpace(profile.Name)) || (profile.Name.Length > 15 || profile.Name.Length < 3))
                    {
                        profile.Name = "CHN_TeknoPlayer";
                        isChanged = true;
                    }

                    //profile.ID = Convert.ToInt64(ini.GetSetting("Settings", "ID"));
                    //if (string.IsNullOrWhiteSpace(profile.ID.ToString()) || profile.ID.ToString() == "0")
                    //{
                    //    var low = (long)rng.Next(0x1000, 0xFFFF);
                    //    var high = (long)rng.Next(0x1000, 0xFFFF);
                    //    profile.ID = Convert.ToInt64(low + string.Empty + high);
                    //    isChanged = true;
                    //}

                    profile.FOV = Convert.ToInt32(ini.GetSetting("Settings", "FOV"));
                    if (profile.FOV > 90 || profile.FOV < 65)
                    {
                        profile.FOV = 75;
                        isChanged = true;
                    }

                    profile.Clantag = ini.GetSetting("Settings", "Clantag");
                    if ((string.IsNullOrEmpty(profile.Clantag) || string.IsNullOrWhiteSpace(profile.Clantag)) || (profile.Clantag.Length > 4 || profile.Clantag.Length < 2))
                    {
                        profile.Clantag = "SXXM";
                        isChanged = true;
                    }

                    profile.Title = ini.GetSetting("Settings", "Title");
                    if ((string.IsNullOrEmpty(profile.Title) || string.IsNullOrWhiteSpace(profile.Title)) || profile.Title.Length > 15)
                    {
                        profile.Title = "^5SuperTeknoMW3";
                        isChanged = true;
                    }

                    profile.ShowConsole = Convert.ToBoolean(ini.GetSetting("Settings", "ShowConsole"));
                    if (profile.ShowConsole == true)
                    {
                        profile.ShowConsole = false;
                        isChanged = true;
                    }

                    if (isChanged)
                    {
                        MessageBox.Show("检测到配置文件存在异常，请重新调整你的玩家信息。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Settings st = new Settings(profile);
                        st.ShowDialog();
                        UpdateProfile();
                    }
                    else
                    {
                        textBlock.Text = "欢迎！" + profile.Name;
                    }
                }
                else
                {
                    MessageBox.Show("未检测到任何配置文件，你需要先设置你的玩家信息。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateNewProfile();

                    Settings st = new Settings(profile);
                    st.ShowDialog();
                    UpdateProfile();
                }
            }
            catch (Exception)
            {
                File.Delete("teknogods.ini");
                MessageBox.Show("配置文件无效，已创建新的配置文件，请重新调整你的玩家信息。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateNewProfile();

                Settings st = new Settings(profile);
                st.ShowDialog();
                UpdateProfile();
            }
        }

        private void CreateNewProfile()
        {
            var low = (long)rng.Next(0x1000, 0xFFFF);
            var high = (long)rng.Next(0x1000, 0xFFFF);

            profile.Name = "CHN_TeknoPlayer";
            //profile.ID = Convert.ToInt64(low + string.Empty + high);
            profile.FOV = 75;
            profile.Clantag = "SXXM";
            profile.Title = "^5SuperTeknoMW3";
            profile.ShowConsole = false;

            try
            {
                File.WriteAllLines("teknogods.ini", new string[]
                {
                    "[Settings]",
                    "Name=" + profile.Name,
                    //"ID=" + profile.ID,
                    "FOV=" + profile.FOV,
                    "Clantag=" + profile.Clantag,
                    "Title=" + profile.Title,
                    "ShowConsole=" + profile.ShowConsole.ToString().ToLower()
                });
            }
            catch (Exception)
            {
                MessageBox.Show("创建配置文件失败！请检查磁盘是否有写保护，以及是否有写入权限！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void DisableAll()
        {
            textBox.IsEnabled = false;
            btn_about.IsEnabled = false;
            btn_donate.IsEnabled = false;
            btn_github.IsEnabled = false;
            btn_mp.IsEnabled = false;
            btn_settings.IsEnabled = false;
            btn_sp.IsEnabled = false;
        }

        private void EnableAll()
        {
            textBox.IsEnabled = true;
            btn_about.IsEnabled = true;
            btn_donate.IsEnabled = true;
            btn_github.IsEnabled = true;
            btn_mp.IsEnabled = true;
            btn_settings.IsEnabled = true;
            btn_sp.IsEnabled = true;
        }

        private async Task StartProcess(string proc, string arguements)
        {
            RunProc runproc = new RunProc { ExecutableName = proc, Commandargs = arguements };
            Task tick = runproc.Tick(proc == "iw5mp.exe" ? "teknomw3.dll" : "teknomw3_sp.dll");
            tick.Start();

            probar.IsIndeterminate = true;
            label.Content = "游戏启动中...";

            await Task.Delay(3000);

            try
            {
                while (true)
                {
                    await Task.Delay(5000);
                    if (tick.IsCompleted || tick.IsCanceled)
                    {
                        probar.IsIndeterminate = false;
                        label.Content = "就绪";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                probar.IsIndeterminate = false;
                label.Content = "发生错误：" + ex.Message;
            }
        }

        private void CheckIp(string domain)
        {
            try
            {
                int port = 1;

                if (domain.Contains(":"))
                {
                    port = Convert.ToInt32(domain.Split(new char[] { ':' }, 2)[1]);
                    if (port > 65536 || port < 1)
                    {
                        port = 27016;
                    }

                    UriHostNameType result = Uri.CheckHostName(domain.Split(new char[] { ':' }, 2)[0]);

                    if (result == UriHostNameType.Unknown || result == UriHostNameType.Basic)
                    {
                        textBox.Text = "";
                    }
                    else
                    {
                        textBox.Text = domain.Split(new char[] { ':' }, 2)[0] + ":" + port;
                    }
                }
                else
                {
                    UriHostNameType result2 = Uri.CheckHostName(domain);

                    if (result2 == UriHostNameType.Unknown || result2 == UriHostNameType.Basic)
                    {
                        textBox.Text = "";
                    }
                }
            }
            catch (Exception)
            {
                textBox.Text = "";
            }
        }

        private void btn_donate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SuperTeknoMW3 的开发以及 SXXM 服务器的运行离不开你的支持！\n" +
                "你可以通过捐赠的方式来维持 SuperTeknoMW3 的开发和 SXXM 服务器的运行\n\n" +
                "支付宝：15807770106\n\n" +
                "如果您有宝贵的意见与建议，欢迎与我们交流！\nQQ群：195343722", "捐赠", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/A2ON/");
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private async void btn_mp_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("iw5mp.exe"))
            {
                MessageBox.Show("未找到iw5mp.exe！请检查你的游戏是否完整！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DisableAll();
            if (string.IsNullOrEmpty(textBox.Text))
            {
                await StartProcess("iw5mp.exe", "");
            }
            else
            {
                if (!textBox.Text.Contains(":"))
                {
                    textBox.Text = textBox.Text + ":27016";
                }
                await StartProcess("iw5mp.exe", "+server " + textBox.Text);
            }
        }

        private async void btn_sp_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("iw5sp.exe"))
            {
                MessageBox.Show("未找到iw5sp.exe！请检查你的游戏是否完整！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DisableAll();
            if (string.IsNullOrEmpty(textBox.Text))
            {
                await StartProcess("iw5sp.exe", "");
            }
            else
            {
                if (textBox.Text.Contains(":"))
                {
                    textBox.Text = textBox.Text.Split(new char[] { ':' }, 2)[0];
                }
                await StartProcess("iw5sp.exe", "+server " + textBox.Text);
            }
        }



        public async void UpdateProfile()
        {
            probar.IsIndeterminate = true;
            label.Content = "更新配置文件...";
            LoadProfile();
            await Task.Delay(1000);
            label.Content = "就绪";
            probar.IsIndeterminate = false;
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIp(textBox.Text);
        }
    }
}
