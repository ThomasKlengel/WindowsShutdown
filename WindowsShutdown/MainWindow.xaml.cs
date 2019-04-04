using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsShutdown
{

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string CONFIGPATH = "Config.xml";
        ViewModel _vm;
        System.Timers.Timer secondsTimer;
        System.Windows.Forms.NotifyIcon ni;

        public MainWindow()
        {
            CloseIfAlreadyRunning();
            InitializeComponent();
            _vm = XmlHelper.ReadConfig(CONFIGPATH);
            _vm.TimeRemainingVisibility = Visibility.Hidden;
            CB_Mode.ItemsSource = Enum.GetValues(typeof(WindowsShutdownMode));

            this.DataContext = _vm;
            DP_ShutdownDate.DisplayDateStart = DateTime.Now.Date;


            secondsTimer = new System.Timers.Timer(1000);
            secondsTimer.Elapsed += T_Elapsed;

            #region create icon for minimizing to tray
            ni = new System.Windows.Forms.NotifyIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/WindowsShutdown;component/Resources/exit_17902.ico")).Stream;
            ni.Icon = new System.Drawing.Icon(iconStream);
            ni.Visible = false;
            ni.MouseClick += TrayIcon_MouseClick;
            ni.Text = "WindowsShutdown.exe";
            ni.Text = $"Time until {Enum.GetName(typeof(WindowsShutdownMode), _vm.ShutdownMode)}: {_vm.TimeToShutdown}";
            #endregion

            Microsoft.Win32.SystemEvents.PowerModeChanged += OnPowerChange;

            if (_vm.Dayly)
            {
                ButtonStart_Click(new Button(), null);
            }

        }

        #region Events

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    {

                        this.Show();
                        this.WindowState = WindowState.Normal;
                        ni.Visible = false;
                        break;
                    }
                case System.Windows.Forms.MouseButtons.Right: break;
                default: break;
            }
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_vm.TimeRemainingVisibility = Visibility.Collapsed;
            try
            {
                _vm.TimeToShutdown = new DateTime() + (_vm.DisplayOnlyShutdownDate - DateTime.Now);
                ni.Text = $"Time until {Enum.GetName(typeof(WindowsShutdownMode), _vm.ShutdownMode)}: {_vm.TimeToShutdown.ToString("HH:mm:ss")}";
            }
            catch (ArgumentOutOfRangeException AoorEx)
            {

            }
            finally
            {
                switch ((Convert.ToInt32((_vm.DisplayOnlyShutdownDate - DateTime.Now).TotalSeconds)))
                {
                    default: break;
                    case 300:
                        {
                            MessageBox.Show("There are 5 minutes remaining until shutdown.\r\nYou can cancel until there is 1 second remaining.",
                            "5 minutes until shutdown", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    case 0: InitiateShutdown(); break;

                }

                //_vm.TimeRemainingVisibility = Visibility.Visible;
            }

        }

        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    _vm = XmlHelper.ReadConfig(CONFIGPATH);
                    if (_vm.Dayly)
                    {
                        ButtonStart_Click(new Button(), null);
                        if (!ni.Visible)
                        {
                            ni.Visible = true;
                        }
                    }
                    break;
                //case PowerModes.Suspend:
                //    ni.Visible = false;
                //    break;
                default: break;
            }
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Button startButton = (Button)sender;

            if (!_vm.RunCountdown)
            {
                SetShutdownTime(startButton);
            }
            else
            {
                if (MessageBox.Show("Ein Shutdown Timer läuft bereits.\r\nWollen sie wirklich einen neuen starten?",
                    "Timer läuft bereits", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    _vm.RunCountdown = false;
                    SetShutdownTime(startButton);

                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            _vm.RunCountdown = false;
            //_vm.TimeRemainingVisibility = Visibility.Collapsed;
            secondsTimer.Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XmlHelper.SaveConfig(_vm, CONFIGPATH);
            ni.Visible = false;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                if (!ni.Visible)
                {
                    ni.Visible = true;
                }
            }
        }
        #endregion

        #region Methods
           
        private void SetShutdownTime(Button startButton)
        {
            _vm.DisplayOnlyShutdownDate = DateTime.Now;
            if (startButton.Name == "B_StartTimer")
            {
                _vm.DisplayOnlyShutdownDate = _vm.DisplayOnlyShutdownDate.AddHours(Convert.ToDouble(_vm.Timer[0]))
                    .AddMinutes(Convert.ToDouble(_vm.Timer[1]))
                    .AddSeconds(Convert.ToDouble(_vm.Timer[2]));
            }
            else
            {
                if (!_vm.Dayly)
                {
                    _vm.DisplayOnlyShutdownDate = _vm.ShutdownDate;
                }
                else
                {
                    _vm.DisplayOnlyShutdownDate = DateTime.Now.Date;
                }
                _vm.DisplayOnlyShutdownDate = _vm.DisplayOnlyShutdownDate.Date.AddHours(Convert.ToDouble(_vm.Date[0]))
                    .AddMinutes(Convert.ToDouble(_vm.Date[1]))
                    .AddSeconds(Convert.ToDouble(_vm.Date[2]));
                
                while (_vm.DisplayOnlyShutdownDate < DateTime.Now)
                {
                    _vm.DisplayOnlyShutdownDate = _vm.DisplayOnlyShutdownDate.AddDays(1);
                }
            }

            Task.Run(async ()=>
            {  
                await Task.Delay(1000);
                StartCountdown();
            });

        }
        private void StartCountdown()
        {

            _vm.RunCountdown = true;
            Task.Run(async ()=>
            {
                while (_vm.DisplayOnlyShutdownDate > DateTime.Now && _vm.RunCountdown)
                {
                    _vm.TimeToShutdown = DateTime.Today + (_vm.DisplayOnlyShutdownDate - DateTime.Now);
                    ni.Text = $"Time until {Enum.GetName(typeof(WindowsShutdownMode), _vm.ShutdownMode)}: {_vm.TimeToShutdown.ToString("HH:mm:ss")}";
                    int remaining = Convert.ToInt32((_vm.DisplayOnlyShutdownDate - DateTime.Now).TotalSeconds);                    
                    if (remaining == 300)
                    {
                        Task.Run(() =>
                        {
                            MessageBox.Show($"5 min remaining");
                        });
                    }
                    await Task.Delay(1000);
                }

                if (_vm.RunCountdown)
                {
                    InitiateShutdown();
                }

            }
            );

        }
        private void InitiateShutdown()
        {
            _vm.RunCountdown = false;
            //ButtonStop_Click(null, null);
            XmlHelper.SaveConfig(_vm, CONFIGPATH);

            switch (_vm.ShutdownMode)
            {
                case WindowsShutdownMode.Shutdown: System.Diagnostics.Process.Start("shutdown", "/s /t 0"); break;
                case WindowsShutdownMode.Restart: System.Diagnostics.Process.Start("shutdown", "/r /t 0"); break;
                case WindowsShutdownMode.Hibernate: System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, false, false); break;
                case WindowsShutdownMode.Suspend: System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, false, false); break;
                default: break;
            }

        }

        
        public void CloseIfAlreadyRunning()
        {
            var appProcess = Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (appProcess.Count() > 1)
            {
                MessageBox.Show("Another instance is alread running, check system tray");
                this.Close();
            }
        }        

        #endregion

    }

    public enum WindowsShutdownMode
    {
        Shutdown,        
        Hibernate, // low power save state to HDD
        Suspend, // low power save state to RAM (data loss on power loss)
        Restart
    }
        
}
