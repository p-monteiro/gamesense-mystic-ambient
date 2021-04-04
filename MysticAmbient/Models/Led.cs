using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MysticAmbient.Models
{
    public class LedLight : ObservableObject
    {
        public int Number { get; private set; }

        private SolidColorBrush ledColor;
        public SolidColorBrush LedColor { get => ledColor; set => SetProperty(ref ledColor, value); }

        public LedLight(int number)
        {
            Number = number;
        }
    }


    public class LedZone : ObservableObject
    {
        public LedLight[] Leds { get; private set; }

        private SolidColorBrush zoneColor;
        public SolidColorBrush ZoneColor { get => zoneColor; set => SetProperty(ref zoneColor, value); }

        public int R { get => c.R; }
        public int G { get => c.G; }
        public int B { get => c.B; }

        Color c = Color.FromRgb(0, 0, 0);

        public LedZone(LedLight[] leds)
        {
            zoneColor = new SolidColorBrush(c);

            this.Leds = leds;
            foreach(LedLight led in this.Leds)
            {
                led.LedColor = ZoneColor;
            }
        }

        public void SetZoneColor(byte red, byte green, byte blue)
        {
            c.R = red;
            c.G = green;
            c.B = blue;

            ZoneColor.Color = c;
        }
    }
}
