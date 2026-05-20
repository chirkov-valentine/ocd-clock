using ReactiveUI;
using System;

namespace AnalogClockAvalonia.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private TimeOnly _currentTime;
        private bool _isRunning;
        private bool _isDiscrete;
        private TimeOnly _lastDisplayedTime;

        public TimeOnly CurrentTime
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
            _currentTime = new TimeOnly(10, 30, 0);
            _isRunning = true;
            _isDiscrete = true;
            _lastDisplayedTime = TimeOnly.MinValue;
        }

        public void UpdateTime(TimeOnly newTime)
        {
            if (!IsDiscrete || newTime.Second != _lastDisplayedTime.Second)
            {
                CurrentTime = newTime;
                _lastDisplayedTime = newTime;
            }
        }
    }
}
