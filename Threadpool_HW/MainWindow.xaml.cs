using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Threadpool_HW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource? cts;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "(*.txt)|*.txt";
            fileDialog.ShowDialog();
            TbFile.Text = fileDialog.FileName ?? default;

        }


        public void Encryption(string file, string password)
        {
            cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
            {
                int len = 1;
                var buffer = new byte[len];
                Dispatcher.Invoke(() =>
                {
                    ProgresBar.Maximum = fs.Length;
                });
                for (int i = 0; i < fs.Length; i++)
                {

                    if (cts.IsCancellationRequested)
                    {
                        for (int k = i - 1; k >= 0; k--)
                        {
                            fs.Position = k;
                            fs.Read(buffer, 0, buffer.Length);
                            fs.Position -= len;
                            byte.TryParse((buffer[0] ^ password[k % password.Length]).ToString(), out buffer[0]);
                            fs.Write(buffer, 0, len);
                            Dispatcher.Invoke(() =>
                            {
                                ProgresBar.Value = k;
                            });
                            Thread.Sleep(10);
                        }
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProgresBar.Value = i + 1;
                    });

                    fs.Read(buffer, 0, buffer.Length);
                    fs.Position -= len;
                    byte.TryParse((buffer[0] ^ password[i % password.Length]).ToString(), out buffer[0]);
                    fs.Write(buffer, 0, len);
                    Thread.Sleep(3);
                }
                MessageBox.Show("Encrypted");
            }
        }



        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string file = TbFile.Text;
            string password = TBpassword.Text;
            if (!File.Exists(file))
            {
                return;
            }
            if (password == "")
            {
                MessageBox.Show("Enter password");
                return;
            }
            if (cts != null) return;
            ThreadPool.QueueUserWorkItem(f =>
            {
                Encryption(file, password);
                Dispatcher.Invoke(() =>
                {
                    cts = null;
                    TbFile.Text = default;
                    TBpassword.Text = default;
                    ProgresBar.Value = 0;
                });
            });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (cts?.IsCancellationRequested == false) cts.Cancel();
        }
    }


}
