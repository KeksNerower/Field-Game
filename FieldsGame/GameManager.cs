using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FieldsGame
{
    class GameManager : UserControlManager
    {
        //Адрес сервера через который происходит связь клиентов
        private string url = "ws://93.100.235.43:1234";
        //Размер стороны поля field
        private int fieldSide = 10;

        //WebSocketClient осуществляет взаимодействеи клиента и сервера
        public WebSocketClient ws;
        //Объект поля field
        public Field field;

        //Используется для выделения площади под закрашивание
        public List<Cell> square;

        //Словарь для составления fieldMap по цветам ячеек
        Dictionary<Color, int> reCellColors = new Dictionary<Color, int>();
        //Словарь для корректного отображения счета(при закрашивании площади)
        Dictionary<Color, Control> playerScoreLbls = new Dictionary<Color, Control>();

        //Размер закрашиваемой площади после броска кубиков
        public int Number { get; private set; }
        //Указывает, была ли выделена первая ячейка для опред. площади
        public bool Pressed { get; private set; }
        //Цвет игрока
        public Color PlayerColor { get; set; }

        public GameManager()
        {
            //Инициализация объекта WebSocketClient(ws)
            ws = new WebSocketClient(url, this);

            //Инициализация полей field
            field = new Field(fieldSide, fieldSide);
            field.FieldInit(OnClick);

            square = new List<Cell>();

            //Добавление элементов словаря
            //Серый = 0
            reCellColors.Add(Color.Gainsboro, 0);
            //Красный = 1
            reCellColors.Add(Color.Red, 1);
            //Синий = 2
            reCellColors.Add(Color.Blue, 2);

            //Инициализация параметров формы
            //Заголовок формы
            Text = "FieldGame";
            //Режим формы - фиксированный размер
            FormBorderStyle = FormBorderStyle.FixedSingle;
            //Высота и ширина формы, зависят от размера поля field
            Height = field.Rows * Cell.side + 50;
            Width = field.Columns * Cell.side + 255;

            //Инициализация и отрисовка контролов
            InitControls();

            //Инициализация словаря
            playerScoreLbls.Add(Color.Red, redScoreLbl);
            playerScoreLbls.Add(Color.Blue, blueScoreLbl);

            //Изначально очки кубиков = 0
            Number = 0;
            //Ни одна из ячеек не активирована
            Pressed = false;
        }

        //Установка цвета игрока
        public void GetPlayerColor(Color color)
        {
            PlayerColor = color;
            //Цвет текста кнопки брока кубиков = цвету игрока
            diceRollBtn.ForeColor = color;

            //Если игрок красный, то ходит первым
            if (color == Color.Red)
                if (diceRollBtn.InvokeRequired)
                {
                    diceRollBtn.Invoke(new Action(() => diceRollBtn.Enabled = true));
                }
        }
        
        //Отрисовка значения кубиков, выброшенного соперником
        public void ChangeNumber(int number)
        {
            DrawObjText(diceScoreLbl, number);
        }
        //Изменение счета игроков после получения от сервера fieldMap
        public void ChangePlayersScore(int redScore, int blueScore)
        {
            if (diceRollBtn.InvokeRequired)
            {
                diceRollBtn.Invoke(new Action(() => diceRollBtn.Enabled = true));
            }

            DrawObjText(redScoreLbl, redScore);
            DrawObjText(blueScoreLbl, blueScore);

        }

        //Бросок кубиков
        public void RollDice(object sender, EventArgs e)
        {
            var rnd = new Random();
            int a = rnd.Next(1, 6);
            int b = rnd.Next(1, 6);

            //Получение площади, которую можно закрасить
            Number = a * b;
            diceRollBtn.Enabled = false;
            //Активация кнопки пропуска хода
            skipBtn.Enabled = true;

            DrawObjText(diceScoreLbl, Number);
            //Отправление выброшенного значения на сервер
            ws.SendToServer("Number", Number);
        }
        //Пропуск хода
        public void Skip(object sender, EventArgs e)
        {
            //Отправление текущего состояния поля field
            SendMap(0);
        }

        //Нажатие на ячейку поля field
        public void OnClick(object sender, EventArgs e)
        {
            Cell cellSender = (Cell)sender;

            //Если еще не активирована ни одна из ячеек
            if (!Pressed)
            {
                //Первое нажатие
                FirstCellClick(cellSender);
            }
            else
            {
                //Была ли активирована ранее это же ячейка
                if (square[0] == sender)
                {
                    //Деактивация
                    ChangeCellClick(cellSender);
                }
                else
                {
                    //Активация второй ячейки
                    SecondCellClick(cellSender);
                }
            }

        }

        //Первое нажатие
        public void FirstCellClick(Cell cellSender)
        {
            //Добавление первой активированной ячейки в square
            square.Add(cellSender);
            //Первая ячейка активирована
            Pressed = true;

            //Смена цвета активированной ячейки на более темный
            ColorCell(cellSender, Color.DarkGray);

            //Если число на кубиках = 1
            if (Number == 1)
            {
                //Покраска активированной ячейки в цвет игрока
                cellSender.BackColor = PlayerColor;
                //Отписка от нажатия(нельзя будет активировать после)
                cellSender.Click -= OnClick;

                //Отправка fieldMap
                SendMap(Number);
            }

        }
        //Деактивация ячейки
        public void ChangeCellClick(Cell cellSender)
        {
            //Удаление дактивированной ячейки из square
            square.Remove(cellSender);
            //Ни одна ячейка еще не активорована
            Pressed = false;

            //Перекраска деактивированной ячейки в базовый цвет
            ColorCell(cellSender, Color.Gainsboro);
        }

        //Активация второй ячейки
        public void SecondCellClick(Cell cellSender)
        {
            //Координаты углов выделенной площади
            int minX;
            int maxX;
            int minY;
            int maxY;

            //Кто левее, тот minX, кто правее - maxX
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
            //Кто ниже, тот minY, кто выше - maxY
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

            //Общее число ячеек выделенной площади
            int sqr = (maxX - minX + 1) * (maxY - minY + 1);
            //Если площадь не равна очкам кубиков
            if (sqr != Number)
            {
                //Выход из метода
                return;
            }
            else
            {
                //Перебор всех ячеек выделенной площади
                for (int i = minY; i <= maxY; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        //Покраска ячейки в цвет игрока
                        ColorCell(field.cells[j][i], PlayerColor);
                        //Отписка от события нажатия
                        field.cells[j][i].Click -= OnClick;
                    }
                }

                //Отправка fieldMap
                SendMap(sqr);
            }

        }

        //Отправка fieldMap
        private void SendMap(int sqr)
        {
            //Сброс значений до следующего хода
            Pressed = false;
            square.Clear();
            Number = 0;

            //Блокировка кнопки Skip
            skipBtn.Enabled = false;

            //Изменение очков походившего 
            Control lbl = playerScoreLbls[PlayerColor];
            DrawObjText(lbl, Convert.ToInt32(lbl.Text) + sqr);
            
            //Создание и заполнение fieldMap
            int[,] fieldMap = new int[field.Rows, field.Columns];
            for (int i = 0; i < field.Rows; i++)
            {
                for (int j = 0; j < field.Columns; j++)
                {
                    //Запись цифры в fieldMap в соответствии цвету ячейки
                    fieldMap[i, j] = reCellColors[field.cells[i][j].BackColor];
                }
            }

            //Отправление fieldMap на сервер
            ws.SendToServer("Move", fieldMap);
        }

        //Конец игры
        public void EndOfGame(string str)
        {
            //Оповещение о победе или проигрыше
            DrawObjText(diceRollBtn, str);

            //Обращение к объектам из другого потока
            if (diceRollBtn.InvokeRequired)
            {
                //Блокировка брока кубиков
                diceRollBtn.Invoke(new Action(() => diceRollBtn.Enabled = false));
            }
            if (newGameBtn.InvokeRequired)
            {
                //Появление кнопки NewGame
                newGameBtn.Invoke(new Action(() => newGameBtn.Visible = true));
            }
        }

        //Новая игра
        public void NewGame(object sender, EventArgs e)
        {
            //Сокрытие кнопки NewGame
            newGameBtn.Visible = false;

            //Создание и заполнение fieldMap
            int[,] fieldMap = new int[field.Rows, field.Columns];
            for (int i = 0; i < field.Rows; i++)
            {
                for (int j = 0; j < field.Columns; j++)
                {
                    //Запись в fieldMap обнуленного поля
                    fieldMap[i, j] = 0;
                    //Подписка ячеек на нажатие
                    field.cells[i][j].Click += OnClick;
                }
            }

            //Отправление fieldMap на сервер
            ws.SendToServer("Move", fieldMap);

            //В следующей игре ход переходит проигравшему
            if (diceRollBtn.Text == "Lose")
            {
                //Активация кнопки RollDice
                diceRollBtn.Enabled = true;
            }

        }

        //Инициализация контролов
        public void InitControls()
        {
            //Инициализация кнопки броска кубиков
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

            //Инициализация кнопки пропуска хода
            skipBtn = new Button()
            {
                Height = 50,
                Width = 50,
                Location = new Point(this.Width - 240, 5),
                BackColor = Color.Gainsboro,
                Text = "Skip",
                Enabled = false
            };
            skipBtn.Click += Skip;

            //Инициализация кнопки новой игры
            newGameBtn = new Button()
            {
                Height = 60,
                Width = 100,
                Location = new Point(this.Width - 175, this.Height - 180),
                BackColor = Color.Gainsboro,
                Text = "New Game",
                Visible = false
            };
            newGameBtn.Click += NewGame;

            //Инициализация лейбла очков красного
            redScoreLbl = new Label()
            {
                Height = 60,
                Width = 60,
                Location = new Point(this.Width - 70 * 2 - 35, 5),
                ForeColor = Color.Red,
                BackColor = Color.Gainsboro,
                Text = "0"
            };

            //Инициализация лейбла очков синего
            blueScoreLbl = new Label()
            {
                Height = 60,
                Width = 60,
                Location = new Point(this.Width - 95, 5),
                ForeColor = Color.Blue,
                BackColor = Color.Gainsboro,
                Text = "0"
            };

            //Инициализация лейбла с результатом броска кубиков
            diceScoreLbl = new Label()
            {
                Height = 60,
                Width = 120,
                Location = new Point(this.Width - 175, 90),
                BackColor = Color.Gainsboro,
                Text = "Dise Score"
            };

            //Отрисовка всех контролов
            base.Draw(diceRollBtn, skipBtn, newGameBtn, redScoreLbl, blueScoreLbl, diceScoreLbl);
            //Отрисовка ячеек поля field
            base.Draw(field.cells);

        }
    }
}
