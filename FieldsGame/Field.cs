using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldsGame
{
    class Field
    {
        //Число строк поля
        public int Rows { get; private set; }
        //Число столбцов поля
        public int Columns { get; private set; }
        //Лист листов ячеек, представляющийй само поле field
        public List<List<Cell>> cells { get; private set; }

        public Field(int rows, int columns)
        {
            //Инициализация 
            Rows = rows;
            Columns = columns;

            cells = new List<List<Cell>>();
        }

        //Инициализация ячеек поля field
        public void FieldInit(EventHandler OnClick)
        {
            for (int i = 0; i < Rows; i++)
            {
                //Добавление очередного листа в лист
                cells.Add(new List<Cell>());
                for (int j = 0; j < Columns; j++)
                {
                    //Добавление новых ячеек в лист
                    cells[i].Add(new Cell(i, j));
                    //Пописка ячеек на нажатие
                    cells[i][j].Click += OnClick;
                }
            }
        }
    }
}
