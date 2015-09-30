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
            TextWriter writer = new StreamWriter("teknogods.ini");

            try
            {
                writer.WriteLine("[Settings]");
                writer.WriteLine("Name=" + profile.Name);
                //writer.WriteLine("ID=" + profile.ID);
                writer.WriteLine("FOV=" + profile.FOV);
                writer.WriteLine("Clantag=" + profile.Clantag);
                writer.WriteLine("Title=" + profile.Title);
                writer.WriteLine("ShowConsole=" + profile.ShowConsole.ToString().ToLower());
            }
            catch (Exception)
            {
                MessageBox.Show("创建配置文件失败！请检查磁盘是否有写保护，以及是否有写入权限！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            finally
            {
                writer.Close();
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

            if ((string.IsNullOrEmpty(profile.Name) || string.IsNullOrWhiteSpace(profile.Name)) || (profile.Name.Length > 15 || profile.Name.Length < 3))
            {
                MessageBox.Show("游戏昵称不符合要求！请重新输入。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //profile.ID = Convert.ToInt64(ini.GetSetting("Settings", "ID"));
            //if (string.IsNullOrWhiteSpace(profile.ID.ToString()) || profile.ID.ToString() == "0")
            //{
            //}

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

            //profile.ID = Convert.ToInt64(ini.GetSetting("Settings", "ID"));
            //if (string.IsNullOrWhiteSpace(profile.ID.ToString()) || profile.ID.ToString() == "0")
            //{
            //}

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

        private void MetroWindow_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
