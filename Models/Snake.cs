using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeRemake.Models
{
    public class Snake : Caliburn.Micro.PropertyChangedBase
    {
        public Point SnakePosition { get; set; }
        public int InitialLength { get; set; } = 2;
        public static int SnakeSquareSize { get; set; } = 20;

        private SolidColorBrush colorBrush;

        public SolidColorBrush SnakeBodyColor {
            get 
            {
                return colorBrush;
            }
            set 
            {
                if (colorBrush != value) 
                {
                    colorBrush = value;
                    NotifyOfPropertyChange(() => SnakeBodyColor);
                }
            }
        }
    }
}
