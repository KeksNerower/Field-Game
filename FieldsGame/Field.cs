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
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public List<List<Cell>> cells { get; private set; }

        public Field(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;

            cells = new List<List<Cell>>();
        }

        public void FieldInit(EventHandler OnClick)
        {
            
            for (int i = 0; i < Rows; i++)
            {
                cells.Add(new List<Cell>());
                for (int j = 0; j < Columns; j++)
                {
                    cells[i].Add(new Cell(i, j));
                    cells[i][j].Click += OnClick;
                }
            }
        }
    }
}
