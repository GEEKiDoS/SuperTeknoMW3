using System;
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
        private bool canclose = true;

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
                MessageBox.Show("启动器读不了配置文件惹！阁下是不是加了访问权限呢？", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            try
            {
                InitDeleteFakeFile();
            }
            catch (Exception)
            {
                MessageBox.Show("启动器出错了呢，请阁下检查一下游戏根目录有没有以前版本的启动器和其他乱七八糟的文件呢，如果有的话先清理一下吧。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            label.Content = "检查更新...";
            try
            {
                if (await CheckUpdate.CheckVersion("1.1.0"))
                {
                    MessageBoxResult result = MessageBox.Show("检测到新版本：" + CheckUpdate.version + "！请阁下先去下载新的版本。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start("http://bbs.3dmgame.com/thread-5030431-1-1.html");
                    Close();
                }
                else
                {
                    label.Content = "准备就绪了呢";
                    probar.IsIndeterminate = false;
                }
            }
            catch (Exception)
            {
                label.Content = "检查更新失败了%>_<%，请阁下亲自到3DM看看吧。";
                Process.Start("http://bbs.3dmgame.com/thread-5030431-1-1.html");
                probar.IsIndeterminate = false;
            }
            finally
            {
                EnableAll();
            }
        }

        private void InitDeleteFakeFile()
        {
            string[] _fakefiles = new string[]
            {
                // SB TK Files
                "client.wyc",
                "TeknoMW3.exe",
                "TeknoMW3_Update.exe",
                "devraw\\video\\startup.bik",

                //Other
                "开始游戏.exe",
                "开始游戏1.exe",
                "开始游戏2.exe",
                "d3d9.dll",
                "lpk.dll"
            };

            foreach (var item in _fakefiles)
            {
                if (File.Exists(item))
                {
                    File.Delete(item);
                }
            }
        }

        private void LoadProfile()
        {
            try
            {
                if (File.Exists("teknogods.ini"))
                {
                    bool AutoChanged = false;
                    bool NeedChange = false;
                    IniParser ini = new IniParser("teknogods.ini");
                    profile = new Proflie();

                    profile.Name = ini.GetSetting("Settings", "Name");
                    if ((string.IsNullOrEmpty(profile.Name) || string.IsNullOrWhiteSpace(profile.Name)) || (profile.Name.Length > 15 || profile.Name.Length < 3))
                    {
                        profile.Name = "Futa's Pet";
                        NeedChange = true;
                    }

                    profile.FOV = Convert.ToInt32(ini.GetSetting("Settings", "FOV"));
                    if (profile.FOV > 90 || profile.FOV < 65)
                    {
                        profile.FOV = 75;
                        AutoChanged = true;
                    }

                    profile.Clantag = ini.GetSetting("Settings", "Clantag");
                    if (string.IsNullOrWhiteSpace(profile.Clantag) || profile.Clantag.Length > 4)
                    {
                        profile.Clantag = "^1CN";
                        AutoChanged = true;
                    }

                    profile.Title = ini.GetSetting("Settings", "Title");
                    if (profile.Title.Length > 25)
                    {
                        profile.Title = "";
                        AutoChanged = true;
                    }

                    if (NeedChange)
                    {
                        MessageBox.Show("你的玩家名不符合规范哦，请先修改一下。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Settings st = new Settings(profile);
                        st.ShowDialog();
                        UpdateProfile();
                    }
                    else if (AutoChanged)
                    {
                        ini.AddSetting("Settings", "FOV", profile.FOV.ToString());
                        ini.AddSetting("Settings", "Clantag", profile.Clantag);
                        ini.AddSetting("Settings", "Title", profile.Title);
                        ini.AddSetting("Settings", "Maxfps", "0");

                        ini.SaveSettings();
                    }
                    else
                    {
                        textBlock.Text = "欢迎阁下！" + profile.Name;
                    }
                }
                else
                {
                    MessageBox.Show("配置文件不存在呢，请阁下先设置一下你的玩家信息吧。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateNewProfile();

                    Settings st = new Settings(profile);
                    st.ShowDialog();
                    UpdateProfile();
                }
            }
            catch (Exception)
            {
                File.Delete("teknogods.ini");
                MessageBox.Show("启动器读取阁下的配置文件的时候出问题了%>_<%，还请阁下重新设置一下吧。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateNewProfile();

                Settings st = new Settings(profile);
                st.ShowDialog();
                UpdateProfile();
            }
        }

        private void CreateNewProfile()
        {
            profile.Name = "Futa's Pet";
            profile.FOV = 75;
            profile.Clantag = "^1CN";
            profile.Title = "^5SuperTeknoMW3";

            try
            {
                File.WriteAllLines("teknogods.ini", new string[]
                {
                    "[Settings]",
                    "Name=" + profile.Name,
                    "FOV=" + profile.FOV,
                    "Clantag=" + profile.Clantag,
                    "Title=" + profile.Title,
                    "Maxfps=" + 0
                });
            }
            catch (Exception)
            {
                MessageBox.Show("创建配置文件出现问题了，阁下给游戏设置只读了么？", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void StartProcess(string proc, string arguements)
        {
            probar.IsIndeterminate = true;
            label.Content = "游戏正在运行desu...";
            canclose = false;
            DisableAll();
            try
            {
                RunProc runproc = new RunProc { ExecutableName = proc, Commandargs = arguements };
                await runproc.Tick(proc == "iw5mp.exe" ? "teknomw3.dll" : "teknomw3_sp.dll");
            }
            catch (Exception)
            {
                MessageBox.Show("游戏打开失败了，阁下的游戏貌似有问题呢，请重新安装下破解吧。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableAll();
                probar.IsIndeterminate = false;
                label.Content = "准备就绪了呢";
                canclose = true;
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
            MessageBox.Show("感谢阁下点开了这里\n" +
                "SuperTeknoMW3和FUTA服务器上的插件都是A2ON大大制作的说\n\n" +
                "支付宝：18754253278\n\n" +
                "A2ON很希望和阁下一起玩耍的说！\nQQ群：161151287", "捐赠", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void btn_mp_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("iw5mp.exe"))
            {
                MessageBox.Show("游戏打开失败了，阁下的游戏貌似有问题呢，请重新安装下破解吧。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(textBox.Text))
            {
                StartProcess("iw5mp.exe", "");
            }
            else
            {
                if (!textBox.Text.Contains(":"))
                {
                    textBox.Text = textBox.Text + ":27016";
                }
                StartProcess("iw5mp.exe", "+server " + textBox.Text);
            }
        }

        private void btn_sp_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("iw5sp.exe"))
            {
                MessageBox.Show("游戏打开失败了，阁下的游戏貌似有问题呢，请重新安装下破解吧。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(textBox.Text))
            {
                StartProcess("iw5sp.exe", "");
            }
            else
            {
                if (textBox.Text.Contains(":"))
                {
                    textBox.Text = textBox.Text.Split(new char[] { ':' }, 2)[0];
                }
                StartProcess("iw5sp.exe", "+server " + textBox.Text + ":0");
            }
        }



        public async void UpdateProfile()
        {
            probar.IsIndeterminate = true;
            label.Content = "更新配置文件...";
            LoadProfile();
            await Task.Delay(1000);
            label.Content = "准备就绪了呢";
            probar.IsIndeterminate = false;
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckIp(textBox.Text);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canclose)
            {
                e.Cancel = true;
            }
        }
    }
}
