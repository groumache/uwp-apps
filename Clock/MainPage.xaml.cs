using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Clock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly DispatcherTimer Timer = new DispatcherTimer();

        private void DrawCanvasAnalogClock()
        {
            // big white rectangle every 3 hours
            for (int i = 0; i < 4; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(Windows.UI.Colors.White),
                    Width = 6,
                    Height = 25,

                    // Translation.x = AnalogClock.Center.x - (Rectangle.Width / 2)
                    Translation = new System.Numerics.Vector3(147, 0, 0),

                    // CenterPoint.x = Rectangle.Width / 2
                    // CenterPoint.y = AnalogClock.Center.y
                    CenterPoint = new System.Numerics.Vector3(3, 150, 0),

                    Rotation = i * (360 / 4),
                };
                AnalogClock.Children.Add(rect);
            }

            // small white rectangle every hour
            for (int i = 0; i < 12; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(Windows.UI.Colors.White),
                    Width = 2,
                    Height = 20,
                    
                    // Translation.x = AnalogClock.Center.x - (Rectangle.Width / 2)
                    Translation = new System.Numerics.Vector3(149, 0, 0),

                    // CenterPoint.x = Rectangle.Width / 2
                    // CenterPoint.y = AnalogClock.Center.y
                    CenterPoint = new System.Numerics.Vector3(1, 150, 0),

                    Rotation = i * (360 / 12),
                };
                AnalogClock.Children.Add(rect);
            }

            // clock hands
            Rectangle seconds = new Rectangle
            {
                Name = "SecondHand",
                Fill = new SolidColorBrush(Windows.UI.Colors.White),
                Width = 2,
                Height = 120,

                // Translation.x = AnalogClock.Center.x - (Rectangle.Width / 2)
                // Translation.y = AnalogClock.Center.y + "something"
                Translation = new System.Numerics.Vector3(149, 145, 0),

                // CenterPoint.x = Rectangle.Width / 2
                // CenterPoint.y = "something"
                CenterPoint = new System.Numerics.Vector3(1, 5, 0),

                Rotation = -135,
            };
            AnalogClock.Children.Add(seconds);

            Rectangle minute = new Rectangle
            {
                Name = "MinuteHand",
                Fill = new SolidColorBrush(Windows.UI.Colors.White),
                Width = 4,
                Height = 100,

                // Translation.x = AnalogClock.Center.x - (Rectangle.Width / 2)
                // Translation.y = AnalogClock.Center.y + "something"
                Translation = new System.Numerics.Vector3(148, 145, 0),

                // CenterPoint.x = Rectangle.Width / 2
                // CenterPoint.y = "something"
                CenterPoint = new System.Numerics.Vector3(2, 5, 0),

                Rotation = -90,
            };
            AnalogClock.Children.Add(minute);

            Rectangle hour = new Rectangle
            {
                Name = "HourHand",
                Fill = new SolidColorBrush(Windows.UI.Colors.White),
                Width = 4,
                Height = 80,

                // Translation.x = AnalogClock.Center.x - (Rectangle.Width / 2)
                // Translation.y = AnalogClock.Center.y + "something"
                Translation = new System.Numerics.Vector3(148, 145, 0),

                // CenterPoint.x = Rectangle.Width / 2
                // CenterPoint.y = "something"
                CenterPoint = new System.Numerics.Vector3(2, 5, 0),

                Rotation = -50,
            };
            AnalogClock.Children.Add(hour);
        }

        public MainPage()
        {
            this.InitializeComponent();

            DrawCanvasAnalogClock();

            Timer.Tick += TimerTick;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, milliseconds: 100);
            Timer.Start();
        }

        private void TimerTick(object sender, object e)
        {
            var time = DateTime.Now;
            int h = time.Hour;
            int m = time.Minute;
            int s = time.Second;
            DigitalClock.Text = time.ToString("h:mm:ss tt");

            float degreeS = -180 + (360 / 60) * s;
            float degreeM = -180 + (360 / 60) * m + (360 / 60 / 60) * s;
            float degreeH = -180 + (360 / 12) * h + (360 /24 / 60) * m + (360 / 24 / 60 / 60) * s;

            // modify the clock hand
            var clockHand = AnalogClock.Children.TakeLast(3).ToList();
            clockHand[0].Rotation = degreeS;
            clockHand[1].Rotation = degreeM;
            clockHand[2].Rotation = degreeH;
        }
    }
}
