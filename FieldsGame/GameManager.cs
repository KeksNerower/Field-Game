using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldsGame
{
    class GameManager : UserControlManager
    {
        private string url = "ws://93.100.235.43:1234";

        public WebSocketClient ws;
        public Field field;

        public List<Cell> square;

        public int Number { get; private set; }
        public bool Pressed { get; private set; }
        public Color PlayerColor { get; set; }

        public GameManager()
        {
            ws = new WebSocketClient(url, this);

            field = new Field(10, 10);
            field.FieldInit(OnClick);

            square = new List<Cell>();

            Text = "FieldGame";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Height = field.Rows * Cell.side + 50;
            Width = field.Columns * Cell.side + 255;

            InitControls();

            Number = 0;
            Pressed = false;
        }

        public void GetPlayerColor(Color color)
        {
            PlayerColor = color;
            diceRollBtn.ForeColor = color;

            if (color == Color.Red)
                diceRollBtn.Enabled = true;
        }
        
        public void ChangeNumber(int number)
        {
            DrawObjTextInvoke(diceScoreLbl, number);
        }
        public void ChangePlayersScore(int redScore, int blueScore)
        {
            diceRollBtn.Enabled = true;

            DrawObjTextInvoke(redScoreLbl, redScore);
            DrawObjTextInvoke(blueScoreLbl, blueScore);

        }

        public void RollDice(object sender, EventArgs e)
        {
            var rnd = new Random();
            int a = rnd.Next(1, 6);
            int b = rnd.Next(1, 6);

            Number = a * b;
            diceRollBtn.Enabled = false;

            DrawObjText(diceScoreLbl, Number);
            ws.SendToServer("Number", Number);
        }

        public void OnClick(object sender, EventArgs e)
        {
            //Событие при нажатие на ячейку cell
            //1ое нажатие - выбор 1го угла для построения прямоугольника
            //2ое нажатие на ту же ячейку - снятие выбора
            //нажатие на 2ую ячейку - закрашивание выделенной области, отправление картыField на сервер
            //

            Cell cellSender = (Cell)sender;

            if (!Pressed)
            {
                FirstCellClick(cellSender);
            }
            else
            {
                if (!(square[0] == sender))
                {
                    SecondCellClick(cellSender);
                }
                else
                {
                    ChangeCellClick(cellSender);
                }
            }

        }

        public void FirstCellClick(Cell cellSender)
        {
            square.Add(cellSender);
            Pressed = true;

            ColorCell(cellSender, Color.DarkGray);

            if (Number == 1)
            {
                cellSender.BackColor = PlayerColor;

                Pressed = false;
                square.Clear();
                Number = 0;

                SendMap();
            }

        }
        public void ChangeCellClick(Cell cellSender)
        {
            square.Remove(cellSender);
            Pressed = false;

            ColorCell(cellSender, Color.Gainsboro);
        }

        public void SecondCellClick(Cell cellSender)
        {
            int minX;
            int maxX;
            int minY;
            int maxY;

            if (square[0].X < cellSender.X)
            {
                minX = square[0].X;
                maxX = cellSender.X;
            }
            else
            {
                minX = cellSender.X;
                maxX = square[0].X;
            }
            if (square[0].Y < cellSender.Y)
            {
                minY = square[0].Y;
                maxY = cellSender.Y;
            }
            else
            {
                minY = cellSender.Y;
                maxY = square[0].Y;
            }

            if ((maxX - minX + 1)*(maxY - minY + 1) != Number)
            {
                return;
            }
            else
            {
                for (int i = minY; i <= maxY; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        ColorCell(field.cells[j][i], PlayerColor);
                        field.cells[j][i].Click -= OnClick;
                    }
                }
                Pressed = false;
                square.Clear();
                Number = 0;

                SendMap();
            }

        }

        private void SendMap()
        {
            //Отправка получившейся карты Field через ws.SendToSerwer
            //FieldMap - двумерный массив с int-цветами
            Dictionary<Color, int> reCellColors = new Dictionary<Color, int>();
            reCellColors.Add(Color.Gainsboro, 0);
            reCellColors.Add(Color.Red, 1);
            reCellColors.Add(Color.Blue, 2);

            int[,] fieldMap = new int[field.Rows, field.Columns];
            for (int i = 0; i < field.Rows; i++)
            {
                for (int j = 0; j < field.Columns; j++)
                {
                    fieldMap[i, j] = reCellColors[field.cells[i][j].BackColor];
                }
            }

            ws.SendToServer("Move", fieldMap);
        }

        public void EndOfGame(string str)
        {
            DrawObjText(diceRollBtn, str);

            field.FieldInit(OnClick);
            Draw(field.cells);

            SendMap();
        }


        public void InitControls()
        {

            diceRollBtn = new Button()
            {
                Height = 60,
                Width = 100,
                Location = new Point(this.Width - 175, this.Height - 100),
                BackColor = Color.Gainsboro,
                Text = "Roll the dice",
                Enabled = false
            };
            diceRollBtn.Click += RollDice;

            redScoreLbl = new Label()
            {
                Height = 60,
                Width = 60,
                Location = new Point(this.Width - 70 * 2 - 35, 5),
                ForeColor = Color.Red,
                BackColor = Color.Gainsboro,
                Text = "0"
            };

            blueScoreLbl = new Label()
            {
                Height = 60,
                Width = 60,
                Location = new Point(this.Width - 95, 5),
                ForeColor = Color.Blue,
                BackColor = Color.Gainsboro,
                Text = "0"
            };

            diceScoreLbl = new Label()
            {
                Height = 60,
                Width = 120,
                Location = new Point(this.Width - 185, 150),
                BackColor = Color.Gainsboro,
                Text = "Dise Score"
            };

            base.Draw(diceRollBtn, redScoreLbl, blueScoreLbl, diceScoreLbl);
            base.Draw(field.cells);

        }
    }
}
