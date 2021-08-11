using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CubeKing.Core
{
    public delegate double EasingEquation(double t, double b, double c, double d);

    public class Eqs
    {
        public static EasingEquation OutBounce = (t, b, c, d) =>
        {
            if ((t /= d) < (1 / 2.75)) return c * (7.5625 * t * t) + b;
            else if (t < (2 / 2.75)) return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
            else if (t < (2.5 / 2.75)) return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
            else return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
        };

        public static EasingEquation InBounce = (t, b, c, d) =>
        {
            if ((t /= d) < (1 / 2.75)) return c * (7.5625 * t * t) + b;
            else if (t < (2 / 2.75)) return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
            else if (t < (2.5 / 2.75)) return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
            else return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
            //return c - InBounce(d - t, 0, c, d) + b;
        };

        public static EasingEquation InOutBounce = (t, b, c, d) =>
        {
            if (t < d / 2) return InBounce(t * 2, 0, c, d) * .5 + b;
            else return InOutBounce(t * 2 - d, 0, c, d) * .5 + c * .5 + b;
        };

        public static EasingEquation Linear = (t, b, c, d) =>
        {
            return c * t / d + b;
        };

        public static EasingEquation InQuart = (t, b, c, d) =>
        {
            return c * (t /= d) * t * t * t + b;
        };

        public static EasingEquation InOutQuart = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
        };

        public static EasingEquation OutQuart = (t, b, c, d) =>
        {
            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        };

        public static EasingEquation OutBack = (t, b, c, d) =>
        {
            return c * ((t = t / d - 1) * t * ((1.70158 + 1) * t + 1.70158) + 1) + b;
        };

        public static EasingEquation InBack = (t, b, c, d) =>
        {
            return c * (t /= d) * t * ((1.70158 + 1) * t - 1.70158) + b;
        };

        public static EasingEquation InOutBack = (t, b, c, d) =>
        {
            double s = 1.70158;
            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
        };

        public static EasingEquation OutElastic = (t, b, c, d) =>
        {
            if ((t /= d) == 1) return b + c;
            double p = d * .3;
            double s = p / 4;
            return (c * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b);
        };

        public static EasingEquation InElastic = (t, b, c, d) =>
        {
            if ((t /= d) == 1) return b + c;
            double p = d * .3;
            double s = p / 4;
            return -(c * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
        };

        public static EasingEquation InOutElastic = (t, b, c, d) =>
        {
            if ((t /= d / 2) == 2) return b + c;
            double p = d * (.3 * 1.5);
            double s = p / 4;
            if (t < 1) return -.5 * (c * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
            return c * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p) * .5 + c + b;
        };

        public static EasingEquation OutCubic = (t, b, c, d) =>
        {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        };

        public static EasingEquation InCubic = (t, b, c, d) =>
        {
            return c * (t /= d) * t * t + b;
        };

        public static EasingEquation InOutCubic = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t + 2) + b;
        };

        public static EasingEquation OutExpo = (t, b, c, d) =>
        {
            return (t == d) ? b + c : c * (-Math.Pow(2, -10 * t / d) + 1) + b;
        };

        public static EasingEquation InExpo = (t, b, c, d) =>
        {
            return (t == 0) ? b : c * Math.Pow(2, 10 * (t / d - 1)) + b;
        };

        public static EasingEquation InOutExpo = (t, b, c, d) =>
        {
            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d / 2) < 1) return c / 2 * Math.Pow(2, 10 * (t - 1)) + b;
            return c / 2 * (-Math.Pow(2, -10 * --t) + 2) + b;
        };

        public static EasingEquation InQuad = (t, b, c, d) =>
        {
            return -c * (t /= d) * (t - 2) + b;
        };

        public static EasingEquation OutQuad = (t, b, c, d) =>
        {
            return c * (t /= d) * t + b;
        };

        public static EasingEquation InOutQuad = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return c / 2 * t * t + b;
            return -c / 2 * ((--t) * (t - 2) - 1) + b;
        };

        public static EasingEquation OutSine = (t, b, c, d) =>
        {
            return c * Math.Sin(t / d * (Math.PI / 2)) + b;
        };

        public static EasingEquation InSine = (t, b, c, d) =>
        {
            return -c * Math.Cos(t / d * (Math.PI / 2)) + c + b;
        };

        public static EasingEquation InOutSine = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return c / 2 * (Math.Sin(Math.PI * t / 2)) + b;
            return -c / 2 * (Math.Cos(Math.PI * --t / 2) - 2) + b;
        };

        public static EasingEquation OutCirc = (t, b, c, d) =>
        {
            return c * Math.Sqrt(1 - (t = t / d - 1) * t) + b;
        };

        public static EasingEquation InCirc = (t, b, c, d) =>
        {
            return -c * (Math.Sqrt(1 - (t /= d) * t) - 1) + b;
        };

        public static EasingEquation InOutCirc = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return -c / 2 * (Math.Sqrt(1 - t * t) - 1) + b;
            return c / 2 * (Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
        };


        public static EasingEquation OutQuint = (t, b, c, d) =>
        {
            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        };

        public static EasingEquation InQuint = (t, b, c, d) =>
        {
            return c * (t /= d) * t * t * t * t + b;
        };

        public static EasingEquation InOutQuint = (t, b, c, d) =>
        {
            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
        };


    }
}
