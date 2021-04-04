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
        public int R { get => c.R; }
        public int G { get => c.G; }
        public int B { get => c.B; }

        private SolidColorBrush ledColor;
        public SolidColorBrush LedColor { get => ledColor; set => SetProperty(ref ledColor, value); }

        Color c = Color.FromRgb(0, 0, 0);

        public LedLight()
        {
            ledColor = new SolidColorBrush(c);
        }

        public void SetLedColor(byte red, byte green, byte blue)
        {
            c.R = red;
            c.G = green;
            c.B = blue;
            LedColor.Color = c;
        }
    }


    public class LedZone
    {
        LedLight[] leds;

        public LedZone(LedLight[] leds)
        {
            this.leds = leds;
        }

        public void SetZoneColor(byte red, byte green, byte blue)
        {
            foreach (LedLight led in leds)
            {
                led.SetLedColor(red, green, blue);
            }
        }
    }
}
