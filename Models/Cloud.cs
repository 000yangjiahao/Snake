using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeRemake.Models
{
    public class Cloud : Caliburn.Micro.PropertyChangedBase
    {
        public Point CloudPosition { get; set; }
        public SolidColorBrush CloudRandomColor { get; set; }
    }
}
