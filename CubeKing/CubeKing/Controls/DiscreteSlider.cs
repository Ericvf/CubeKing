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

namespace CubeKing.Controls
{
    public class DiscreteSlider : Slider
    {

        protected override void OnValueChanged(double oldValue, double newValue)
        {

            if (!m_busy)
            {

                m_busy = true;



                if (SmallChange != 0)
                {

                    double newDiscreteValue = (int)(Math.Round(newValue / SmallChange)) * SmallChange;



                    //if (newDiscreteValue != m_discreteValue)
                    {

                        Value = newDiscreteValue;

                        base.OnValueChanged(m_discreteValue, newDiscreteValue);

                        m_discreteValue = newDiscreteValue;

                    }

                }

                else
                {

                    base.OnValueChanged(oldValue, newValue);

                }



                m_busy = false;

            }

        }



        bool m_busy;

        double m_discreteValue;

    }

}
