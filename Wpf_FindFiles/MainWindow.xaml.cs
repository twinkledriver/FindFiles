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
        private ObservableCollection<FoundFile> FoundFiles = null;

        private CancellationTokenSource cts = null;

        Action<String> showInfo = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtFileSize_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void searchFiles(String Location, long fileLength)
        {
            DirectoryInfo dir = null;
            List<FileInfo> files = null;
            FoundFile foundFile=null;
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

             foreach (var directory in dir.GetDirectories())
            {
                searchFiles(directory.FullName, fileLength);
            }
        }
          catch (UnauthorizedAccessException ex)
    {
        Dispatcher.Invoke(showInfo,Directory.Name+"无权限访问");
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
            Task tsk=new Task(()=>
                {
                    searchFiles(Location,length);
                
                }


        }
    }
}
