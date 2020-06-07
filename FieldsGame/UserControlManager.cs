using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldsGame
{
    class UserControlManager : Form
    {
        public Button diceRollBtn;
        public Label redScoreLbl;
        public Label blueScoreLbl;
        public Label diceScoreLbl;

        public void Draw(List<List<Cell>> cells)
        {
            for(int i = 0; i < cells.Count; i++)
            {
                for(int j = 0; j < cells[i].Count; j++)
                {
                    this.Controls.Add(cells[i][j]);
                }
            }
        }
        public void Draw(params Control[] controls)
        {
            foreach(var control in controls)
            {
                this.Controls.Add(control);
            }
        }
       
        public void DrawObjTextInvoke(Control control, int number)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => control.Text = number.ToString()));
            }
        }

        public void DrawObjText(Control control, string str)
        {
            control.Text = str;
        }
        public void DrawObjText(Control control, int number)
        {
            control.Text = number.ToString();
        }

        public void ColorCell(Cell cell, Color color)
        {
            cell.BackColor = color;
        }


    }
}
