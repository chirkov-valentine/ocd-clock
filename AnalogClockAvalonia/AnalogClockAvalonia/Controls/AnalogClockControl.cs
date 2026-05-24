using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using System;

namespace AnalogClockAvalonia.Controls
{
    /// <summary>
    /// An Avalonia analog clock control that displays time with hour, minute, and second hands.
    /// </summary>
    public class AnalogClockControl : Control
    {
        private DispatcherTimer? _timer;
        private TimeOnly _baseTime;
        private DateTime _baseDateTime;
        private TimeOnly _lastDisplayedTime;
        private const double ClockRadius = 50;
        private const double CenterX = 50;
        private const double CenterY = 50;
        private const double NumeralRadius = 36;

        public static readonly StyledProperty<TimeOnly> TimeProperty =
            AvaloniaProperty.Register<AnalogClockControl, TimeOnly>(
                nameof(Time), defaultValue: TimeOnly.FromDateTime(DateTime.Now));

        public static readonly StyledProperty<bool> IsRunningProperty =
            AvaloniaProperty.Register<AnalogClockControl, bool>(
                nameof(IsRunning), defaultValue: true);

        public static readonly StyledProperty<bool> IsDiscreteProperty =
            AvaloniaProperty.Register<AnalogClockControl, bool>(
                nameof(IsDiscrete), defaultValue: true);

        /// <summary>
        /// Gets or sets the time displayed on the clock.
        /// When IsRunning is true, the clock continues from this time.
        /// When IsRunning is false, the clock displays this time.
        /// </summary>
        public TimeOnly Time
        {
            get => GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public bool IsRunning
        {
            get => GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public bool IsDiscrete
        {
            get => GetValue(IsDiscreteProperty);
            set => SetValue(IsDiscreteProperty, value);
        }

        public AnalogClockControl()
        {
            Width = 100;
            Height = 100;

            _baseTime = TimeOnly.FromDateTime(DateTime.Now);
            _baseDateTime = DateTime.Now;
            _lastDisplayedTime = _baseTime;

            TimeProperty.Changed.AddClassHandler<AnalogClockControl>(
                (s, e) => {
                    var newTime = (TimeOnly)e.NewValue!;
                    // When Time property changes, reset the base time
                    s._baseTime = newTime;
                    s._baseDateTime = DateTime.Now;
                    s._lastDisplayedTime = newTime;
                    s.InvalidateVisual();
                });

            IsRunningProperty.Changed.AddClassHandler<AnalogClockControl>(
                (s, e) => {
                    // When IsRunning changes, update the base DateTime to keep time continuous
                    s._baseDateTime = DateTime.Now;
                    s.OnIsRunningChanged((bool)e.NewValue!);
                });

            IsDiscreteProperty.Changed.AddClassHandler<AnalogClockControl>(
                (s, e) => {
                    s.InvalidateVisual();
                });

            Loaded += (s, e) =>
            {
                InitTimer(IsRunning);
                InvalidateVisual();
            };
        }

        private void OnIsRunningChanged(bool isRunning)
        {
            InitTimer(isRunning);
        }

        private void InitTimer(bool create)
        {
            if (create && _timer == null)
            {
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(10)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            else if (!create && _timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!IsRunning)
                return;

            // Calculate elapsed time since we set the base time
            var elapsed = DateTime.Now - _baseDateTime;
            var newTime = _baseTime.Add(elapsed);

            // Handle day overflow
            while (newTime.Ticks >= TimeSpan.TicksPerDay)
            {
                newTime = new TimeOnly(newTime.Ticks - TimeSpan.TicksPerDay);
            }

            // In discrete mode, only update when seconds change
            if (IsDiscrete)
            {
                var discreteTime = new TimeOnly(newTime.Hour, newTime.Minute, newTime.Second);
                if (discreteTime.Second != _lastDisplayedTime.Second)
                {
                    Time = discreteTime;
                    _lastDisplayedTime = discreteTime;
                }
            }
            else
            {
                // In continuous mode, update every timer tick
                Time = newTime;
                _lastDisplayedTime = newTime;
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Draw clock face with radial gradient brush (like WPF)
            var gradientStops = new GradientStops
     {
         new GradientStop(Colors.White, 0),
         new GradientStop(Color.FromRgb(255, 255, 239), 0.65),
         new GradientStop(Color.FromRgb(188, 188, 172), 1)
     };
            var radialGradient = new RadialGradientBrush { GradientStops = gradientStops };

            context.DrawEllipse(
                radialGradient,
                new Pen(new SolidColorBrush(Colors.Black), 5),
                new Point(CenterX, CenterY),
                ClockRadius - 2.5,
                ClockRadius - 2.5);

            DrawTickMarks(context);
            DrawNumerals(context);
            DrawClockHands(context);

            // Draw center circle
            context.DrawEllipse(
                new SolidColorBrush(Colors.Black),
                null,
                new Point(CenterX, CenterY),
                2,
                2);
        }

        private void DrawTickMarks(DrawingContext context)
        {
            var pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            var majorPen = new Pen(new SolidColorBrush(Colors.Black), 3);

            for (int i = 0; i < 60; i++)
            {
                double angle = (i * 6) - 90;
                double radians = angle * Math.PI / 180;

                if (i % 5 == 0)
                {
                    // Major tick mark (hour)
                    double x1 = CenterX + (ClockRadius - 10) * Math.Cos(radians);
                    double y1 = CenterY + (ClockRadius - 10) * Math.Sin(radians);
                    double x2 = CenterX + (ClockRadius - 2.5) * Math.Cos(radians);
                    double y2 = CenterY + (ClockRadius - 2.5) * Math.Sin(radians);
                    context.DrawLine(majorPen, new Point(x1, y1), new Point(x2, y2));
                }
                else
                {
                    // Minor tick mark (minute)
                    double x1 = CenterX + (ClockRadius - 8) * Math.Cos(radians);
                    double y1 = CenterY + (ClockRadius - 8) * Math.Sin(radians);
                    double x2 = CenterX + (ClockRadius - 2.5) * Math.Cos(radians);
                    double y2 = CenterY + (ClockRadius - 2.5) * Math.Sin(radians);
                    context.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
                }
            }
        }

        private void DrawNumerals(DrawingContext context)
        {
            var brush = new SolidColorBrush(Colors.DarkRed);
            var typeface = new Typeface("Arial");

            for (int i = 1; i <= 12; i++)
            {
                double angle = (i * 30) - 90;
                double radians = angle * Math.PI / 180;

                double x = CenterX + NumeralRadius * Math.Cos(radians);
                double y = CenterY + NumeralRadius * Math.Sin(radians);

                var text = new FormattedText(
                    i.ToString(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    12,
                    brush);

                context.DrawText(text, new Point(x - text.Width / 2, y - text.Height / 2));
            }
        }

        private void DrawClockHands(DrawingContext context)
        {
            var time = Time;
            int milliseconds = time.Millisecond;

            // Hour hand: 30° per hour + 0.5° per minute
            double hourAngle = (time.Hour % 12 * 30 + time.Minute * 0.5) - 90;
            DrawPointedHand(context, hourAngle, 25, 3.3, Colors.Black);

            // Minute hand: 6° per minute + 0.1° per second
            double minuteAngle = (time.Minute * 6 + time.Second * 0.1) - 90;
            DrawPointedHand(context, minuteAngle, 36, 2.6, Colors.DarkGray);

            // Second hand: 6° per second
            double secondAngle;
            if (IsDiscrete)
            {
                secondAngle = (time.Second * 6) - 90;
            }
            else
            {
                // Include milliseconds for smooth continuous movement
                secondAngle = (time.Second * 6 + (milliseconds * 0.006)) - 90;
            }
            DrawPointedHand(context, secondAngle, 40, 1.3, Colors.Red);
        }

        private void DrawPointedHand(DrawingContext context, double angle, double length, double thickness, Color color)
        {
            double radians = angle * Math.PI / 180;
            double halfWidth = thickness / 2;
            double perpRadians = radians + Math.PI / 2;

            double tipX = CenterX + length * Math.Cos(radians);
            double tipY = CenterY + length * Math.Sin(radians);

            double taperStart = length * 0.9;
            double taperX = CenterX + taperStart * Math.Cos(radians);
            double taperY = CenterY + taperStart * Math.Sin(radians);

            double backLength = 5;
            double backX = CenterX - backLength * Math.Cos(radians);
            double backY = CenterY - backLength * Math.Sin(radians);

            var points = new Points
            {
                new Point(backX + halfWidth * Math.Cos(perpRadians), backY + halfWidth * Math.Sin(perpRadians)),
                new Point(taperX + halfWidth * Math.Cos(perpRadians), taperY + halfWidth * Math.Sin(perpRadians)),
                new Point(tipX, tipY),
                new Point(taperX - halfWidth * Math.Cos(perpRadians), taperY - halfWidth * Math.Sin(perpRadians)),
                new Point(backX - halfWidth * Math.Cos(perpRadians), backY - halfWidth * Math.Sin(perpRadians))
            };

            var geometry = new PolylineGeometry(points, true);

            context.DrawGeometry(
                new SolidColorBrush(color),
                new Pen(new SolidColorBrush(color), 0.5),
                geometry);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == TimeProperty ||
                change.Property == IsRunningProperty ||
                change.Property == IsDiscreteProperty)
            {
                InvalidateVisual();
            }
        }
    }
}
