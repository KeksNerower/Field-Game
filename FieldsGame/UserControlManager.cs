using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FieldsGame
{
    class UserControlManager : Form
    {
        //Кнопка броска кибиков
        public Button diceRollBtn;
        //Кнопка пропуска хода
        public Button skipBtn;
        //Кнопка запуска новой игры
        public Button newGameBtn;
        //Счет красного игрока
        public Label redScoreLbl;
        //Счет синего игрока
        public Label blueScoreLbl;
        //Выпавшее на кубиках значение
        public Label diceScoreLbl;

        //Отрисовка объектов
        public void Draw(List<List<Cell>> cells)
        {
            for(int i = 0; i < cells.Count; i++)
            {
                for(int j = 0; j < cells[i].Count; j++)
                {
                    //Отрисовка ячейки на форме
                    this.Controls.Add(cells[i][j]);
                }
            }
        }
        public void Draw(params Control[] controls)
        {
            foreach(var control in controls)
            {
                //Отрисовка контрола на форме
                this.Controls.Add(control);
            }
        }

        //Изменение текста контрола
        public void DrawObjText(Control control, string str)
        {
            //Если метод вызавается в другом потоке
            if (control.InvokeRequired)
            {
                //Исполняет в потоке, которому пренадлежит элемент
                control.Invoke(new Action(() => control.Text = str));
            }
            else
            {
                //Стандартное изменение текста контрола
                control.Text = str;
            }
        }
        public void DrawObjText(Control control, int number)
        {
            //Если метод вызавается в другом потоке
            if (control.InvokeRequired)
            {
                //Исполняет в потоке, которому пренадлежит элемент
                control.Invoke(new Action(() => control.Text = number.ToString()));
            }
            else
            {
                //Стандартное изменение текста контрола
                control.Text = number.ToString();
            }
        }

        //Окрашивание ячейки
        public void ColorCell(Cell cell, Color color)
        {
            cell.BackColor = color;
        }


    }
}
