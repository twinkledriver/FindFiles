using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_FindFiles
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private FolderBrowserDialog folderBrowserDialog = null;

        private ObservableCollection<FoundFile> FoundFiles = null;

        private CancellationTokenSource cts = null;
        private ShowFileCopyOrMove win = null;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        Action<String> showInfo = null;
        Action<bool> EnableSearchButton = null;

        private void Init()
        {
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "选择要搜索的位置";

            FoundFiles = new ObservableCollection<FoundFile>();
            dgFiles.ItemsSource = FoundFiles;

            showInfo = (info) =>
            {
                lbllnfo.Text = info;

            };
            EnableSearchButton = (Enabled) =>
            {
                btnBeginSearch.IsEnabled = Enabled;
            };

        }


        private void searchFiles(String Location, long fileLength)
        {
            DirectoryInfo dir = null;
            List<FileInfo> files = null;
            FoundFile foundFile = null;
            try
            {
                dir = new DirectoryInfo(Location);
                Dispatcher.Invoke(showInfo, Location);
                if (cts.IsCancellationRequested)
                {
                    return;
                }
                var query = (from file in dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).AsParallel()
                             where file.Length >= fileLength
                             select file).WithCancellation(cts.Token);

                files = query.ToList();

                if (files != null)
                {
                    foreach (var item in files)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return;
                        }
                        foundFile = new FoundFile()
                        {
                            Name = item.Name,
                            Length = item.Length,
                            Location = item.DirectoryName     //逗号，为什么？
                        };
                        Action<FoundFile> addFileDelegte = (file) =>
                            {
                                FoundFiles.Add(file);
                            };
                        Dispatcher.BeginInvoke(addFileDelegte, foundFile);
                    }
                }
                foreach (var directory in dir.GetDirectories())
                {
                    searchFiles(directory.FullName, fileLength);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Dispatcher.Invoke(showInfo, dir.Name + "无权限访问");

            }

        }

        private void btnChooseLocation_Click(object sender, RoutedEventArgs e)
        {
            ChooseLocation();
        }

        private void ChooseLocation()
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtLocation.Text = folderBrowserDialog.SelectedPath;
            }
        }



        private void btnBeginSearch_Click(object sender, RoutedEventArgs e)
        {
            lbllnfo.Text = "正在查找";
            FoundFiles.Clear();
            cts = new CancellationTokenSource();

            btnBeginSearch.IsEnabled = false;
            String Location = txtLocation.Text;
            long length = Convert.ToInt32(txtFileSize.Text) * 1024 * 1024;
            Task tsk = new Task(() =>
                {
                    searchFiles(Location, length);
                    if (!cts.IsCancellationRequested)

                        Dispatcher.Invoke(showInfo, "搜索完成");

                    else
                        Dispatcher.Invoke(showInfo, "搜索被取消");
                    Dispatcher.Invoke(EnableSearchButton, true);

                });
            try
            {
                tsk.Start();

            }
            catch (Exception ex)
            {
                lbllnfo.Text = ex.Message;
                //如果收到取消请求
                if (cts.IsCancellationRequested)
                {
                    Dispatcher.Invoke(showInfo, "搜索已取消");
                    Dispatcher.Invoke(EnableSearchButton, true);
                    return;
                }
                else
                    Dispatcher.Invoke(EnableSearchButton, true);
            }
        }

        private void btnCancelSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
                cts.Cancel();

            EnableSearchButton(true);
            showInfo("搜索己取消");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedFile();
        }

        private void DeleteSelectedFile()
        {
            int index = dgFiles.SelectedIndex;
            if (index != -1 && index < FoundFiles.Count)
            {
                try
                {
                    String FileName = FoundFiles[index].Name;
                    String FileToBeDelete = FoundFiles[index].Location + @"\\" + FileName;
                    File.Delete(FileToBeDelete);
                    FoundFiles.RemoveAt(index);
                    lbllnfo.Text = "文件\"" + FileName + "\"已删除";
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            MoveFile();
        }

        private void MoveFile()
        {
            int index = dgFiles.SelectedIndex;
            if (index == -1 || index >= FoundFiles.Count)
            {
                return;
            }
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    String FileName = FoundFiles[index].Name;
                    String ChoosedFile = FoundFiles[index].Location + @"\\" + FileName;
                    String toDir = folderBrowserDialog.SelectedPath;
                    String toDirFile = toDir.EndsWith("\\") ? toDir + FileName : toDir + @"\\" + FileName;

                    //不允许再进行文件操作
                    //EnableButton(false);
                    //异步执行文件移动
                    Task tsk = new Task(() =>
                    {

                        Action del = () =>
                        {
                            win = new ShowFileCopyOrMove();
                            win.Information = "\"" + FileName + "\"正在移动中...";
                            win.Show();
                        };
                        Dispatcher.Invoke(del);
                        File.Move(ChoosedFile, toDirFile);
                        FoundFiles[index].Location = toDir;
                        // Dispatcher.Invoke(showInfo, "\"" + FileName + "\"文件移动完成");
                        del = () =>
                        {

                            win.Close();
                            win = null;
                        };
                        //Action<bool> del = EnableButton;
                        //Dispatcher.Invoke(del, true);
                        Dispatcher.Invoke(del);
                    });
                    tsk.Start();
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }
            }
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void OpenFolder()
        {
            int index = dgFiles.SelectedIndex;
            if (index != -1 && index < FoundFiles.Count)
            {
                try
                {
                    String ChoosedFile = FoundFiles[index].Location;
                    Process.Start(ChoosedFile);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (win != null)
                win.Close();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        private void OpenFile()
        {
            int index = dgFiles.SelectedIndex;
            if (index != -1 && index < FoundFiles.Count)
            {
                try
                {

                    String ChoosedFile = FoundFiles[index].Location + @"\\" + FoundFiles[index].Name;
                    Process.Start(ChoosedFile);

                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }


        }

        private void txtFileSize_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
