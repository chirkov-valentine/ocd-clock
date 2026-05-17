
using ReactiveUI;
using System;

namespace AnalogClockAvalonia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DateTime _currentTime;
        private bool _isRunning;
        private bool _isDiscrete;

        public DateTime CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        public bool IsDiscrete
        {
            get => _isDiscrete;
            set => this.RaiseAndSetIfChanged(ref _isDiscrete, value);
        }

        public MainViewModel()
        {
            _currentTime = DateTime.Now;
            _isRunning = true;
            _isDiscrete = true;
        }
    }
}
