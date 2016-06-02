using loader_lib;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace loader_ui
{
    public partial class MainWindow : MetroWindow
    {
        private Proflie profile;
        private Random rng = new Random();
        private bool newuser = false;
        private bool canclose = true;
        private bool havesp = true;
        private bool chinese = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(profile);
            settings.ShowDialog();
            UpdateProfile();
            await Task.Delay(1000);
            label.Content = "准备就绪了呢";
            probar.IsIndeterminate = false;
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DisableAll();
            // 校检文件
            if (!File.Exists("cache.xml"))
            {

            }
            // 检查更新
            await Task.Delay(1000);
            label.Content = "检查更新...";
            probar.IsIndeterminate = true;
            try
            {
                // 地址已迁移至app.config，尚未完成，所以使用连接字符串代替，下一版本完善
                await Task.Factory.StartNew(() => Update.GetUpdateInfo(new Uri(System.Configuration.ConfigurationManager.ConnectionStrings["upgrade_address"].ConnectionString)));

                int build = Update.build;
                bool isforcibly = Update.isforcibly;
                string info = Update.info;

                if (build >= 115) //版本号存储将在下一版本迁移至app.config
                {
                    try
                    {
                        if (isforcibly)
                        {
                            MessageBoxResult result = MessageBox.Show("检测到新版本：" + Update.version + "\n" + info + "\n\n点击确定立即下载新版本！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            DownloadUpdate();

                        }
                        else if (profile.SkipUpdate == false)
                        {
                            MessageBoxResult result = MessageBox.Show("检测到非强制更新版本：" + Update.version + "\n" + info + "\n\n是否立即下载新版本？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes)
                            {
                                DownloadUpdate();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("下载文件时发生错误：" + ex.Message);
                    }
                }
            }
            finally
            {

            }

            // 读取玩家配置文件
            label.Content = "读取玩家配置文件...";
            await Task.Delay(100);
            probar.IsIndeterminate = true;
            try
            {
                LoadProfile();
            }
            catch (Exception)
            {
                MessageBox.Show("启动器读不了配置文件惹！阁下是不是加了访问权限呢？", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            finally
            {
                probar.IsIndeterminate = false;
                label.Content = "准备就绪了呢";

                await Task.Delay(1000);
                if (newuser)
                {
                    MessageBoxResult result = MessageBox.Show("阁下可能是第一次使用 SuperTeknoMW3 呢，本喵强烈建议你看一下使用帮助。\n是否立即查看？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Help help = new Help();
                        help.ShowDialog();
                    }
                }
            }
        }

        private async void DownloadUpdate()
        {
            probar.IsIndeterminate = true;
            label.Content = "获取更新信息...";
            List<string> files = Update.updatefiles;
            string script = Update.updatescript;
            await Task.Delay(1000);

            label.Content = "准备下载...";
            if (Directory.Exists("cache"))
            {
                Directory.CreateDirectory("cache");
            }

            try
            {
                Uri uri = chinese ? new Uri("http://superteknomw3-upgrade.daoapp.io/update_files/chinese/") : new Uri("http://superteknomw3-upgrade.daoapp.io/update_files/english/");
                probar.IsIndeterminate = false;

                foreach (var file in files)
                {
                    label.Content = "正在下载：" + file;

                    FileDownload.ProgressChanged += (sender, e) =>
                    {
                        probar.Value = e.ProgressPercentage;
                    };

                    await FileDownload.DownloadFile(uri, file);
                }

                probar.IsIndeterminate = true;
                label.Content = "准备更新...";
                File.WriteAllText("cache\\script.txt", script);
                await Task.Delay(3000);

                MessageBox.Show("更新文件已下载完毕，即将重启启动器并安装更新！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Process.Start("SuperTeknoMW3-Update.exe");
                Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void LoadProfile()
        {
            try
            {
                if (File.Exists("teknogods.ini"))
                {
                    bool AutoChanged = false;
                    bool NeedChange = false;
                    IniParser ini = new IniParser("teknogods.ini");
                    profile = new Proflie();

                    string _name = ini.GetSetting("Settings", "Name");
                    if (string.IsNullOrWhiteSpace(_name) || _name.Length > 15 || _name.Length < 3)
                    {
                        profile.Name = "Futa Master";
                        NeedChange = true;
                    }
                    else
                    {
                        profile.Name = _name;
                    }

                    string _id = ini.GetSetting("Settings", "ID");
                    if (string.IsNullOrWhiteSpace(_id) || _id.Length != 8)
                    {
                        if (Directory.Exists("dw"))
                        {
                            string[] files = Directory.GetFiles("dw");
                            foreach (var item in files)
                            {
                                string item2 = item.Replace("dw\\", "");
                                if (item2.StartsWith("iw5_") && item2.EndsWith(".stat"))
                                {
                                    var result = MessageBox.Show("你的配置文件里没有ID，但是游戏内似乎包含了有效ID。\n阁下是否想重新创建一个新的ID呢？\n\n 注意创建新ID会丢失以前的存档。", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        var id = Guid.NewGuid();
                                        _id = id.ToString().Substring(0, 8).ToUpper();
                                    }
                                    else
                                    {
                                        _id = item2.Substring(item2.Length - 13, 8).ToUpper();
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var id = Guid.NewGuid();
                            _id = id.ToString().Substring(0, 8).ToUpper();
                        }
                        profile.ID = _id;
                    }
                    else
                    {
                        profile.ID = _id;
                    }

                    string _fov = ini.GetSetting("Settings", "FOV");
                    if (string.IsNullOrWhiteSpace(_fov))
                    {
                        profile.FOV = 75;
                        AutoChanged = true;
                    }
                    else
                    {
                        try
                        {
                            if (Convert.ToInt32(_fov) >= 65 || Convert.ToInt32(_fov) <= 90)
                            {
                                profile.FOV = Convert.ToInt32(_fov);
                            }
                        }
                        catch (Exception)
                        {
                            profile.FOV = 75;
                            AutoChanged = true;
                        }

                    }

                    string _clantag = ini.GetSetting("Settings", "Clantag");
                    if (string.IsNullOrWhiteSpace(_clantag) || _clantag.Length > 4)
                    {
                        profile.Clantag = "^1CN";
                        AutoChanged = true;
                    }
                    else
                    {
                        profile.Clantag = _clantag;
                    }

                    string _title = ini.GetSetting("Settings", "Title");
                    if (string.IsNullOrWhiteSpace(_clantag) || _title.Length > 25)
                    {
                        profile.Title = "";
                        AutoChanged = true;
                    }
                    else
                    {
                        profile.Title = _title;
                    }

                    string _showconsole = ini.GetSetting("Settings", "ShowConsole");
                    if (string.IsNullOrWhiteSpace(_showconsole))
                    {
                        profile.ShowConsole = false;
                        AutoChanged = true;
                    }
                    else
                    {
                        try
                        {
                            profile.SkipUpdate = Convert.ToBoolean(_showconsole);
                        }
                        catch (Exception)
                        {
                            profile.ShowConsole = false;
                            AutoChanged = true;
                        }
                    }

                    string _maxfps = ini.GetSetting("Settings", "Maxfps");
                    if (string.IsNullOrWhiteSpace(_maxfps))
                    {
                        profile.Maxfps = 300;
                        AutoChanged = true;
                    }
                    else
                    {
                        try
                        {
                            if (Convert.ToInt32(_maxfps) >= 30 || Convert.ToInt32(_maxfps) <= 300)
                            {
                                profile.Maxfps = Convert.ToInt32(_maxfps);
                            }
                        }
                        catch (Exception)
                        {
                            profile.Maxfps = 300;
                            AutoChanged = true;
                        }

                    }

                    string _skipupdate = ini.GetSetting("Settings", "SkipUpdate");
                    if (string.IsNullOrWhiteSpace(_skipupdate))
                    {
                        profile.SkipUpdate = false;
                        AutoChanged = true;
                    }
                    else
                    {
                        try
                        {
                            profile.SkipUpdate = Convert.ToBoolean(_skipupdate);
                        }
                        catch (Exception)
                        {
                            profile.SkipUpdate = false;
                            AutoChanged = true;
                        }
                    }

                    if (NeedChange)
                    {
                        MessageBox.Show("你的玩家名不符合规范哦，请阁下先修改一下。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Settings st = new Settings(profile);
                        st.ShowDialog();
                        UpdateProfile();
                    }
                    else if (AutoChanged)
                    {
                        SaveProfile();
                    }
                }
                else
                {
                    MessageBox.Show("配置文件不存在呢，请阁下先设置一下你的玩家信息吧。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateNewProfile();
                    newuser = true;

                    Settings st = new Settings(profile);
                    st.ShowDialog();
                    UpdateProfile();
                }
            }
            catch (Exception)
            {
                File.Delete("teknogods.ini");
                MessageBox.Show("启动器读取配置文件的时候出问题了%>_<%，还请阁下重新设置一下吧。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateNewProfile();

                Settings st = new Settings(profile);
                st.ShowDialog();
                UpdateProfile();
            }
        }

        private void SaveProfile()
        {
            IniParser ini = new IniParser("teknogods.ini");
            ini.AddSetting("Settings", "ID", profile.ID);
            ini.AddSetting("Settings", "FOV", profile.FOV.ToString());
            ini.AddSetting("Settings", "Clantag", profile.Clantag);
            ini.AddSetting("Settings", "Title", profile.Title);
            ini.AddSetting("Settings", "ShowConsole", profile.ShowConsole.ToString().ToLower());
            ini.AddSetting("Settings", "Maxfps", profile.Maxfps.ToString());
            ini.AddSetting("Settings", "SkipUpdate", profile.SkipUpdate.ToString().ToLower());
            ini.SaveSettings();

            UpdateProfile();
        }

        //private async Task<bool> DownloadFile()
        //{

        //}

        private void CreateNewProfile()
        {
            profile = new Proflie();
            profile.Name = "Futa Master";
            profile.ID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            profile.FOV = 75;
            profile.Clantag = "^1CN";
            profile.Title = "SuperTeknoMW3";
            profile.ShowConsole = false;
            profile.Maxfps = 300;
            profile.SkipUpdate = false;

            try
            {
                SaveProfile();
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
            cb_maxlevel.IsEnabled = false;
            cb_skipupdate.IsEnabled = false;
        }

        private void EnableAll()
        {
            textBox.IsEnabled = true;
            btn_about.IsEnabled = true;
            btn_donate.IsEnabled = true;
            btn_github.IsEnabled = true;
            btn_mp.IsEnabled = true;
            btn_settings.IsEnabled = true;
            cb_maxlevel.IsEnabled = true;
            cb_skipupdate.IsEnabled = true;
            if (havesp == true)
            {
                btn_sp.IsEnabled = true;
            }
        }

        private async void StartProcess(string proc, string arguements, string mpclantag = "", string mptitle = "", bool unlockall = false)
        {
            probar.IsIndeterminate = true;
            label.Content = "游戏正在运行desu...";
            canclose = false;
            DisableAll();
            try
            {
                RunProc runproc = new RunProc { ExecutableName = proc, Commandargs = arguements, MPClantag = mpclantag, MPTitle = mptitle, MPUnlockAll = unlockall };
                await runproc.Tick(proc == "iw5mp.exe" ? "teknomw3.dll" : "teknomw3_sp.dll");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        port = 26000;
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
                "SuperTeknoMW3和FUTA服务器离不开阁下的支持呢\n\n" +
                "如果阁下希望为服务器塞钱的话，可以通过以下方式：\n支付宝：18754253278\n微信：the_a2on\n\n" +
                "你也可以加入我们的QQ群哦\nQQ群：498440812", "捐赠", MessageBoxButton.OK, MessageBoxImage.Information);
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
                StartProcess("iw5mp.exe", "", profile.Clantag, profile.Title, cb_maxlevel.IsChecked.Value);
            }
            else
            {
                if (!textBox.Text.Contains(":"))
                {
                    textBox.Text = textBox.Text + ":27016";
                }
                StartProcess("iw5mp.exe", "+server " + textBox.Text, profile.Clantag, profile.Title, cb_maxlevel.IsChecked.Value);
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

        public void UpdateProfile()
        {
            DisableAll();
            probar.IsIndeterminate = true;
            label.Content = "更新配置文件...";
            LoadProfile();
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

        private async void cb_skipupdate_Checked(object sender, RoutedEventArgs e)
        {
            profile.SkipUpdate = cb_skipupdate.IsChecked.Value ? false : true;
            SaveProfile();
            await Task.Delay(1000);
            label.Content = "准备就绪了呢";
            probar.IsIndeterminate = false;
            EnableAll();
        }

        private async void cb_skipupdate_Unchecked(object sender, RoutedEventArgs e)
        {
            profile.SkipUpdate = cb_skipupdate.IsChecked.Value ? false : true;
            SaveProfile();
            await Task.Delay(1000);
            label.Content = "准备就绪了呢";
            probar.IsIndeterminate = false;
            EnableAll();
        }
    }
}
