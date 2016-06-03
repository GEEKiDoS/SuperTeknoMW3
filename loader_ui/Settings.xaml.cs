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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.IO;
using loader_lib;

namespace loader_ui
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : MetroWindow
    {
        private Proflie profile;
        private bool canclose = false;

        public Settings(Proflie profile)
        {
            this.profile = profile;
            InitializeComponent();
        }

        private void SaveProfile()
        {
            try
            {
                using (var file = File.Create("teknogods.ini"))
                {

                }

                IniParser ini = new IniParser("teknogods.ini");
                ini.AddSetting("Settings", "Name", profile.Name);
                ini.AddSetting("Settings", "ID", profile.ID);
                ini.AddSetting("Settings", "FOV", profile.FOV.ToString());
                ini.AddSetting("Settings", "Clantag", profile.Clantag);
                ini.AddSetting("Settings", "Title", profile.Title);
                ini.AddSetting("Settings", "ShowConsole", profile.ShowConsole.ToString().ToLower());
                ini.AddSetting("Settings", "Maxfps", profile.Maxfps.ToString());
                ini.AddSetting("Settings", "SkipUpdate", profile.SkipUpdate.ToString().ToLower());
                ini.SaveSettings();
            }
            catch (Exception)
            {
                MessageBox.Show("创建配置文件失败！请检查磁盘是否有写保护，以及是否有写入权限！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txt_nickname.Text = profile.Name;
            txt_fov.Text = profile.FOV.ToString();
            txt_clan.Text = profile.Clantag;
            txt_title.Text = profile.Title;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txt_nickname.Text))
            {
                MessageBox.Show("请输入游戏昵称！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrEmpty(txt_fov.Text))
            {
                MessageBox.Show("请输入视野大小！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if ((string.IsNullOrEmpty(txt_nickname.Text) || string.IsNullOrWhiteSpace(txt_nickname.Text)) || (txt_nickname.Text.Length > 15 || txt_nickname.Text.Length < 3))
            {
                MessageBox.Show("游戏昵称长度不能低于3位和高于15位！\n不能为空或者只用空白字符，也不能使用特殊字符！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Convert.ToInt32(txt_fov.Text) > 90 || Convert.ToInt32(txt_fov.Text) < 65)
            {
                MessageBox.Show("视野大小不能低于65和高于90！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (txt_clan.Text.Length > 4)
            {
                MessageBox.Show("战队标签字符数不可超过4位！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (txt_title.Text.Length > 25)
            {
                MessageBox.Show("个人标签字符数不可超过25位！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            profile.Name = txt_nickname.Text;
            profile.FOV = Convert.ToInt32(txt_fov.Text);
            profile.Clantag = txt_clan.Text;
            profile.Title = txt_title.Text;
            profile.ShowConsole = false;

            SaveProfile();

            canclose = true;
            Close();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txt_nickname.Text))
            {
                MessageBox.Show("请输入游戏昵称！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrEmpty(txt_fov.Text))
            {
                MessageBox.Show("请输入视野大小！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if ((string.IsNullOrEmpty(profile.Name) || string.IsNullOrWhiteSpace(profile.Name)) || (profile.Name.Length > 15 || profile.Name.Length < 3))
            {
                MessageBox.Show("游戏昵称不符合要求！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (profile.FOV > 90 || profile.FOV < 65)
            {
                MessageBox.Show("视野大小不符合要求！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (profile.Clantag.Length > 4)
            {
                MessageBox.Show("战队不符合要求！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (profile.Title.Length > 25)
            {
                MessageBox.Show("标签文本不符合要求！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            profile.Name = txt_nickname.Text;
            profile.FOV = Convert.ToInt32(txt_fov.Text);
            profile.Clantag = txt_clan.Text;
            profile.Title = txt_title.Text;
            profile.ShowConsole = false;

            SaveProfile();

            canclose = true;
            if (!canclose)
            {
                e.Cancel = true;
            }
        }
    }
}