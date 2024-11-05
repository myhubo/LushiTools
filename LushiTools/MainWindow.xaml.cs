using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LushiTools
{
    public partial class FloatingWindow : Window
    {
        private bool isMaximized = false;
        private string _gamePath = string.Empty;

        public FloatingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Width - this.Width - 50;
            this.Top = 50;

            // 检查防火墙是否打开，未打开则自动打开
            if (!IsFirewallEnabled())
                EnableFirewall();

            // 读取炉石传说游戏的执行路径
            if (!CheckGamePath())
                return;

            // 检查是否存在名为 lushi 的出站策略
            if (!IsOutboundRuleExists("lushi"))
                CreateOutboundRule("lushi", _gamePath);
            else
                DisableOutboundRule("lushi");
        }

        /// <summary>
        /// 检查防火墙是否开启
        /// </summary>
        /// <returns></returns>
        private bool IsFirewallEnabled()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("State ON");
        }

        /// <summary>
        ///  启动防火墙
        /// </summary>
        private void EnableFirewall()
        {
            ExecuteCommand("netsh advfirewall set allprofiles state on");
        }

        /// <summary>
        /// 注册表读取游戏路径
        /// </summary>
        /// <returns></returns>
        private string? GetHearthstoneGamePath()
        {
            // 获取当前运行的进程
            var process = Process.GetProcessesByName("Hearthstone");
            if (process.Length > 0)
                return process[0].MainModule.FileName;

            return null;
        }

        private bool IsOutboundRuleExists(string ruleName)
        {
            // 检查出站规则是否存在
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall firewall show rule name=\"" + ruleName + "\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Rule Name:") || output.Contains("规则名称");
        }

        /// <summary>
        /// 创建出站规则
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="programPath"></param>
        private void CreateOutboundRule(string ruleName, string programPath)
        {
            ExecuteCommand($"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block program=\"{programPath}\" enable=yes");
        }

        /// <summary>
        /// 禁用出站规则
        /// </summary>
        /// <param name="ruleName"></param>
        private void DisableOutboundRule(string ruleName)
        {
            ExecuteCommand($"netsh advfirewall firewall set rule name=\"{ruleName}\" new enable=no");
        }

        /// <summary>
        /// 启用出站规则
        /// </summary>
        /// <param name="ruleName"></param>
        private void EnableOutboundRule(string ruleName)
        {
            ExecuteCommand($"netsh advfirewall firewall set rule name=\"{ruleName}\" new enable=yes");
        }

        private void ExecuteCommand(string command)
        {
            // 执行命令
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private bool CheckGamePath()
        {
            // 读取炉石传说游戏的执行路径
            if (string.IsNullOrEmpty(_gamePath))
                _gamePath = GetHearthstoneGamePath();

            if (string.IsNullOrEmpty(_gamePath))
            {
                MessageBox.Show("未找到炉石传说，请手动设置游戏启动路径。");
                return false;
            }

            return true;
        }

        // 一键拔线按钮点击事件
        private async void Window_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (CheckGamePath())
            {
                try
                {
                    this.IsEnabled = false;
                    textBlock.Text = "处理中...";
                    EnableOutboundRule("lushi");
                    await Task.Delay(3000); 
                    DisableOutboundRule("lushi");
                }
                finally
                {
                    textBlock.Text = "一键拔线";
                    this.IsEnabled = true;
                }
            }
        }


        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 右键弹出菜单
            ContextMenu contextMenu = new ContextMenu();
            MenuItem item1 = new MenuItem { Header = "设置" };
            item1.Click += SelectProgramItem_Click;
            contextMenu.Items.Add(item1);
            MenuItem item2 = new MenuItem { Header = "退出" };
            item2.Click += ExitMenuItem_Click;
            contextMenu.Items.Add(item2);
            contextMenu.IsOpen = true;
        }

        private void SelectProgramItem_Click(object sender, RoutedEventArgs e)
        {
            SelectPathWindow selectPathWindow = new SelectPathWindow(_gamePath);
            if (selectPathWindow.ShowDialog() == true)
            {
                _gamePath = selectPathWindow.SelectedPath;
                // 可以在此添加代码更新 UI 或其他逻辑
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}