using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace FieldsGame
{
    class Cell : Button
    {
        //Размер стороны ячейки
        public static int side { get; private set; } = 30;
        //Координаты нахождения ячейки
        public int X { get; private set; }
        public int Y { get; private set; }

        //Инициализация ячейки
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Height = side;
            Width = side;
            Location = new Point(x * side + 5, y * side + 5);
            BackColor = Color.Gainsboro;

        }
    }
}
