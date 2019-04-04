using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace WindowsShutdown
{
    class ViewModel : INotifyPropertyChanged
    {
        readonly List<string> Descriptions = new List<string>()
        {
            $"{Enum.GetName(typeof(WindowsShutdownMode),WindowsShutdownMode.Shutdown)} - Shuts the system down, it will restart when you press the power button on your PC",
            $"{Enum.GetName(typeof(WindowsShutdownMode),WindowsShutdownMode.Hibernate)} - Puts PC into a low power mode, saves the state to HDD",
            $"{Enum.GetName(typeof(WindowsShutdownMode),WindowsShutdownMode.Suspend)} - Puts PC into a low power mode, saves the state to RAM (data loss on power loss)",
            $"{Enum.GetName(typeof(WindowsShutdownMode),WindowsShutdownMode.Restart)} - Shuts the system down and restarts it automatically"
        };

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public ViewModel()
        {
            Timer = new ObservableCollection<string>() { "00", "00", "00" };
            Date = new ObservableCollection<string>() { "00", "00", "00" };
        }

        private DateTime _timeToShutdown;
        /// <summary>
        /// The until system shutdown with shutdownmode
        /// used to display when timer is running
        /// </summary>
        public DateTime TimeToShutdown
        {
            get { return _timeToShutdown; }
            set
            {
                _timeToShutdown = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TimeToShutdown)));
            }
        }

        private DateTime _shutdownDate;
        /// <summary>
        /// The date of the Shutdown, only used when no dayly shutdown
        /// </summary>
        public DateTime ShutdownDate
        {
            get { return _shutdownDate; }
            set
            {
                _shutdownDate = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ShutdownDate)));
            }
        }
        
        private DateTime _diplayOnlyShutdownDate;
        /// <summary>
        /// The date of the Shutdown, only used when no dayly shutdown
        /// </summary>
        public DateTime DisplayOnlyShutdownDate
        {
            get { return _diplayOnlyShutdownDate; }
            set
            {
                _diplayOnlyShutdownDate = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DisplayOnlyShutdownDate)));
            }
        }

        private bool _runCountdown;
        public bool RunCountdown
        {
            get { return _runCountdown; }
            set
            {
                _runCountdown = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(RunCountdown)));
                if (RunCountdown)
                {
                    TimeRemainingVisibility = Visibility.Visible;
                }
                else
                {
                    TimeRemainingVisibility = Visibility.Hidden;
                }
            }
        }

        private Visibility _timeRemainingVisibility;
        public Visibility TimeRemainingVisibility
        {
            get { return _timeRemainingVisibility; }
            set
            {
                _timeRemainingVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TimeRemainingVisibility)));
            }
        }

        private Visibility _dateVisibility;
        public Visibility DateVisibility
        {
            get { return _dateVisibility; }
            set
            {
                _dateVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(DateVisibility)));
            }
        }

        private WindowsShutdownMode _shutdownMode;
        public WindowsShutdownMode ShutdownMode
        {
            get { return _shutdownMode; }
            set
            {
                _shutdownMode = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ShutdownMode)));
                Shutdowndescription = Descriptions[(int)ShutdownMode];
            }
        }

        private string _shutdownDescription;
        public string Shutdowndescription
        {
            get { return _shutdownDescription; }
            set
            {
                _shutdownDescription = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Shutdowndescription)));
            }
        }
                
        private bool _dayly;
        public bool Dayly
        {
            get { return _dayly; }
            set
            {
                _dayly = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Dayly)));
                if (_dayly)
                {
                    DateVisibility = Visibility.Hidden;
                }
                else { DateVisibility = Visibility.Visible; }
            }
        }

        private ObservableCollection<string> _timer;
        /// <summary>
        /// The shutdown timer collection with [HH][mm][ss] used for diplay in TextBoxes
        /// </summary>
        public ObservableCollection<string> Timer
        {
            get { return _timer; }
            set
            {
                _timer = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Timer)));
            }
        }

        private ObservableCollection<string> _date;
        /// <summary>
        /// The shutdown date collection with[HH][mm][ss] used for Display in Textboxes
        /// </summary>
        public ObservableCollection<string> Date
        {
            get { return _date; }
            set
            {
                _date = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Date)));
            }
        }

    }

}
