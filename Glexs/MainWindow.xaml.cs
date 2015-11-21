using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using MahApps.Metro.Controls;

namespace Glexs
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            currentTrack = null;
            PreviousTrack = null;
        }
        
        #region 窗口加载时设置 MediaElementVolume 值
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            mediaelement.Volume = 1; // 设置默认音量
        }
        #endregion

        #region 窗口关闭释放 MediaElement
        private void MainWindowClosed(object sender, EventArgs e)
        {
            mediaelement.Stop();
            mediaelement.Close();
        }
        #endregion

        #region 播放暂停
        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlist.HasItems)
            {
                PlayTrack();
                PauseButtonIsEnabled();
            }
            else
            {
                MessageBox.Show("No music! Please drop some file to ListBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlist.HasItems)
            {
                mediaelement.Pause();
                PlayButtonIsEnabled();
            }
        }
        #endregion

        #region 上一曲下一曲
        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlist.HasItems)
            {
                MoveToNextTrack();
            }
        }

        private void PrecedentButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlist.HasItems)
            {
                MoveToPrecedentTrack();
            }
        }
        #endregion

        #region 显示 Play 或 Pause 按钮

        // PlayButton 显示
        private void PlayButtonIsEnabled()
        {
            PauseButton.Visibility = Visibility.Collapsed;
            PauseButton.IsEnabled = false;

            PlayButton.Visibility = Visibility.Visible;
            PlayButton.IsEnabled = true;
        }

        // PauseButton 显示
        private void PauseButtonIsEnabled()
        {
            PlayButton.Visibility = Visibility.Collapsed;
            PlayButton.IsEnabled = false;

            PauseButton.Visibility = Visibility.Visible;
            PauseButton.IsEnabled = true;
        }

        #endregion

        #region 静音
        private void VolumeThreeButtonClick(object sender, RoutedEventArgs e)
        {
            mediaelement.IsMuted = true;
            VolumeZeroButtonIsEnabled(sender, e);
        }
        #endregion

        #region 取消静音
        private void VolumeZeroButtonClick(object sender, RoutedEventArgs e)
        {
            mediaelement.IsMuted = false;
            VolumeThreeButtonIsEnabled(sender, e);
        }
        #endregion

        #region 调整 VolumeSlider 静音和取消静音
        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeSlider.Value != 0)
            {
                VolumeThreeButtonIsEnabled(sender, e);
            }
            else
            {
                if (VolumeSlider.Value == 0)
                {
                    VolumeZeroButtonIsEnabled(sender, e);
                }
            }
        }
        #endregion

        #region VolumeButton 的显示
        private void VolumeThreeButtonIsEnabled(object sender, RoutedEventArgs e)
        {
            VolumeZeroButton.Visibility = Visibility.Collapsed;
            VolumeZeroButton.IsEnabled = false;

            VolumeThreeButton.Visibility = Visibility.Visible;
            VolumeThreeButton.IsEnabled = true;
        }

        private void VolumeZeroButtonIsEnabled(object sender, RoutedEventArgs e)
        {
            VolumeThreeButton.Visibility = Visibility.Collapsed;
            VolumeThreeButton.IsEnabled = false;

            VolumeZeroButton.Visibility = Visibility.Visible;
            VolumeZeroButton.IsEnabled = true;
        }
        #endregion

        #region MediaElement Opened 与 Ended, TimeLine 的实现
        private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            //mediaelement.Stop(); // 停止播放
            //timer.Stop(); // 定时器停止
            TimeLineBar.Value = 0; // TimeLine 值归零
            MoveToNextTrack();
        }

        private DispatcherTimer timer;
        private void MediaElementMediaOpened(object sender, RoutedEventArgs e) // MediaElement Opened 事件
        {
            if (mediaelement.NaturalDuration.HasTimeSpan)
            {
                TimeLineBar.Maximum = mediaelement.NaturalDuration.TimeSpan.TotalSeconds; // 设置 TimeLine 最大值为媒体文件总时间
                Timer_Tick(sender, e);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer = new DispatcherTimer(); // 定时器
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            TimeLineBar.Value = mediaelement.Position.TotalSeconds; // TimeLine 值为 Position
        }

        #endregion

        #region ListBox 拖放
        ListBoxItem currentTrack;
        ListBoxItem PreviousTrack;

        private void ListBoxDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void ListBoxDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (string fileName in s)
            {
                if (System.IO.Path.GetExtension(fileName) == ".mp3" ||
                    System.IO.Path.GetExtension(fileName) == ".MP3")
                {
                    ListBoxItem listItem = new ListBoxItem();
                    listItem.Content = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    listItem.Tag = fileName;
                    playlist.Items.Add(listItem);
                }
            }
            if (currentTrack == null)
            {
                playlist.SelectedIndex = 0;
                PlayTrack();
            }
        }
        #endregion

        #region 播放文件
        private void PlayTrack() // 播放
        {
            if (playlist.SelectedItem != currentTrack)
            {
                if (currentTrack != null)
                {
                    PreviousTrack = currentTrack;
                }
                currentTrack = (ListBoxItem)playlist.SelectedItem;
                mediaelement.Source = new Uri(currentTrack.Tag.ToString());
                TimeLineBar.Value = 0;
                mediaelement.Play();
                PauseButtonIsEnabled();
                AlbumImage.Source = GetAlbumCover();
            }
            else
            {
                mediaelement.Play();
            }
        }
        #endregion

        #region 列表上一曲
        private void MoveToPrecedentTrack() // 上一曲
        {
            if (playlist.Items.IndexOf(currentTrack) > 0)
            {
                mediaelement.Close();
                timer.Stop();
                TimeLineBar.Value = 0;
                playlist.SelectedIndex = playlist.Items.IndexOf(currentTrack) - 1;
                PlayTrack();
            }
            else if (playlist.Items.IndexOf(currentTrack) == 0)
            {
                mediaelement.Close();
                timer.Stop();
                TimeLineBar.Value = 0;
                playlist.SelectedIndex = playlist.Items.Count - 1;
                PlayTrack();
            }
        }
        #endregion

        #region 列表下一曲
        private void MoveToNextTrack() // 下一曲
        {
            if (playlist.Items.IndexOf(currentTrack) < playlist.Items.Count - 1)
            {
                mediaelement.Close();
                timer.Stop();
                TimeLineBar.Value = 0;
                playlist.SelectedIndex = playlist.Items.IndexOf(currentTrack) + 1;
                PlayTrack();
                return;
            }
            else if (playlist.Items.IndexOf(currentTrack) == playlist.Items.Count - 1)
            {
                mediaelement.Close();
                timer.Stop();
                TimeLineBar.Value = 0;
                playlist.SelectedIndex = 0;
                PlayTrack();
                return;
            }
        }
        #endregion

        #region 点选播放
        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayTrack();
        }
        #endregion

        #region 获取专辑图片
        private ImageSource GetAlbumCover()
        {
            BitmapImage image = null;
            TagLib.File tlFile = TagLib.File.Create(currentTrack.Tag.ToString());
            if (tlFile.Tag.Pictures.Count() > 0)
            {
                MemoryStream ms = new MemoryStream(tlFile.Tag.Pictures[0].Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();
            }
            return image;
        }
        #endregion

        #region 专辑列表显示
        private void ListHiddenButtonChecked(object sender, RoutedEventArgs e)
        {
            playlist.Visibility = Visibility.Collapsed;
        }

        private void ListHiddenButtonUnchecked(object sender, RoutedEventArgs e)
        {
            playlist.Visibility = Visibility.Visible;
        }
        #endregion
    }

}
