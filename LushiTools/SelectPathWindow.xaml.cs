using Microsoft.Win32;
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

namespace LushiTools
{
    /// <summary>
    /// SelectPathWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectPathWindow : Window
    {
        public string SelectedPath { get; set; }

        public SelectPathWindow(string initialPath)
        {
            InitializeComponent();
            PathTextBox.Text = initialPath;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "选择启动程序"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
                SelectedPath = openFileDialog.FileName;
                DialogResult = true;
            }
        }
    }
}
