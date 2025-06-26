using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.DrawImage.Controls
{
    public static class UtilImage
    {
        public enum DataSize
        {
            _byte = 1,
            _short = 2,
        }

        private static int ValueMax = (int)Math.Pow(2, 8 * (int)DataSize._short - 1) - 1;
        private static int ColorMax = (int)byte.MaxValue;

        private static int[] valueRange = new int[]
            {
                (int)((ValueMax / 6.0) * 0),    //White
                (int)((ValueMax / 6.0) * 1),    //Red
                (int)((ValueMax / 6.0) * 2),    //Yellow
                (int)((ValueMax / 6.0) * 3),    //Green
                (int)((ValueMax / 6.0) * 4),    //SkyBlue
                (int)((ValueMax / 6.0) * 5),    //Blue
                (int)((ValueMax / 6.0) * 6)     //Black
            };

        public static void SetValueType(DataSize size)
        {
            ValueMax = (int)Math.Pow(2, 4 * (int)size);

            valueRange = new int[]
            {
                (int)((ValueMax / 6.0) * 0),    //White
                (int)((ValueMax / 6.0) * 1),    //Red
                (int)((ValueMax / 6.0) * 2),    //Yellow
                (int)((ValueMax / 6.0) * 3),    //Green
                (int)((ValueMax / 6.0) * 4),    //SkyBlue
                (int)((ValueMax / 6.0) * 5),    //Blue
                (int)((ValueMax / 6.0) * 6)     //Black
            };
        }

        public static Color ValueToColor(int value)
        {
            byte r = (byte)ColorMax, g = (byte)ColorMax, b = (byte)ColorMax;

            //R
            //White ~ Red ~ Yellow
            if (valueRange[0] <= value && value <= valueRange[2])
                r = (byte)ColorMax;
            //Yellow ~ Green
            else if (valueRange[2] < value && value < valueRange[3])
                r = (byte)(ColorMax - (ColorMax * ((value - valueRange[2]) / (float)(valueRange[3] - valueRange[2]))));
            //Green ~ SkyBlue ~ Blue ~ Black
            else if (valueRange[3] <= value)
                r = 0;

            //G
            //White
            if (valueRange[0] == value)
                g = (byte)ColorMax;
            //White ~ Red
            else if (valueRange[0] < value && value < valueRange[1])
                g = (byte)(ColorMax - (ColorMax * ((value - valueRange[0]) / (float)(valueRange[1] - valueRange[0]))));
            //Red
            else if (valueRange[1] == value)
                g = 0;
            //Red ~ Yellow
            else if (valueRange[1] < value && value < valueRange[2])
                g = (byte)(ColorMax * ((value - valueRange[1]) / (float)(valueRange[2] - valueRange[1])));
            //Yellow ~ Green ~ Skyblue
            else if (valueRange[2] <= value && value <= valueRange[4])
                g = (byte)ColorMax;
            //Skyblue ~ Black
            else if (valueRange[4] < value && value < valueRange[5])
                g = (byte)(ColorMax - (ColorMax * ((value - valueRange[4]) / (float)(valueRange[5] - valueRange[4]))));
            //Black
            else if (valueRange[5] <= value)
                g = 0;

            //B
            //White
            if (valueRange[0] == value)
                b = (byte)ColorMax;
            //White ~ Red
            else if (valueRange[0] < value && value < valueRange[1])
                b = (byte)(ColorMax - (ColorMax * ((value - valueRange[0]) / (float)(valueRange[1] - valueRange[0]))));
            //Red ~ Yellow ~ Green
            else if (valueRange[1] <= value && value <= valueRange[3])
                b = 0;
            //Green ~ Skyblue
            else if (valueRange[3] < value && value < valueRange[4])
                b = (byte)(ColorMax * ((value - valueRange[3]) / (float)(valueRange[4] - valueRange[3])));
            //Skyblue ~ Blue
            else if (valueRange[4] <= value && value <= valueRange[5])
                b = (byte)ColorMax;
            //Blue ~ Black
            else if (valueRange[5] < value && value < valueRange[6])
                b = (byte)(ColorMax - (ColorMax * ((value - valueRange[5]) / (float)(valueRange[6] - valueRange[5]))));
            //Black
            else if (value == valueRange[6])
                b = 0;

            return Color.FromArgb(r, g, b);

            //하강 그래프
            //colorMax - (colormax * ((value - valueRange[n]) / (valueRange[n+1] - valueRange[n])))
            //상승 그래프
            //colorMax * ((value - valueRange[n]) / (valueRange[n+1] - valueRange[n]))
        }

        public static int ColorToValue(Color color)
        {
            int value = 0;

            if(color.R == ColorMax)
            {
                //[0] ~ [1]
                if ((0 < color.G && color.G < ColorMax)
                    && (0 < color.B && color.B < ColorMax))
                    value = (int)(((valueRange[1] - valueRange[0]) * ((ColorMax - color.G) / (float)ColorMax)) + valueRange[0]);
                //[1]
                else if (color.G == 0
                    && color.B == 0)
                    value = (int)valueRange[1];
                //[1] ~ [2]
                else if ((0 < color.G && color.G < ColorMax)
                    && color.B == 0)
                    value = (int)(((valueRange[2] - valueRange[1]) * (color.G / ColorMax)) + valueRange[1]);
                //[2]
                else if (color.G == ColorMax
                    && color.B == 0)
                    value = (int)valueRange[2];
            }
            else
            {
                //[2] ~ [3]
                if(0 < color.R && color.R < ColorMax)
                {
                    value = (int)(((valueRange[3] - valueRange[2]) * ((ColorMax - color.R) / (float)ColorMax)) + valueRange[2]);
                }
                else if(color.R == 0)
                {
                    //[3]
                    if(color.G == ColorMax
                        &&  color.B == 0)
                        value = (int)valueRange[3];
                    //[3] ~ [4]
                    else if (color.G == ColorMax
                        && (0 < color.B && color.B < ColorMax))
                        value = (int)(((valueRange[4] - valueRange[3]) * (color.G / ColorMax)) + valueRange[3]);
                    //[4]
                    else if(color.G == ColorMax
                        && color.B == ColorMax)
                        value = (int)valueRange[4];
                    //[4] ~ [5]
                    else if ((0 < color.G && color.G < ColorMax)
                        && color.B == ColorMax)
                        value = (int)(((valueRange[5] - valueRange[4]) * ((ColorMax - color.R) / (float)ColorMax)) + valueRange[4]);
                    //[5]
                    else if (color.G == 0
                        && color.B == ColorMax)
                        value = (int)valueRange[5];
                    //[5] ~ [6]
                    else if(color.G == 0
                        && (0 < color.B && color.B < ColorMax))
                        value = (int)(((valueRange[6] - valueRange[5]) * ((ColorMax - color.R) / (float)ColorMax)) + valueRange[5]);
                    //[6]
                    else if (color.G == 0
                        && color.B == 0)
                        value = (int)valueRange[6];
                }
            }

            return value;
            //하강 그래프
            //(valueRange[N + 1] - valueRange[N]) * ((ColorMax - c) / ColorMax) + valueRange[N]
            //상승 그래프
            //(valueRange[N + 1] - valueRange[N]) * (c / ColorMax) + valueRange[N]
        }
    }
}
