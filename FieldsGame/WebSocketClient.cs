using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WebSocketSharp;

namespace FieldsGame
{
    delegate void DataTypeHandler(object data);
    class WebSocketClient : WebSocket
    {
        //Словарь распределения данных от сервера; направляет данные в нужную функцию в зависимости от типа данных
        Dictionary<string, DataTypeHandler> dataHandler = new Dictionary<string, DataTypeHandler>();
        //Словарь для распределения цветов игроков
        Dictionary<string, Color> playerColors = new Dictionary<string, Color>();
        //Словарь цветов ячеек, для их окраски по fieldMap
        Dictionary<int, Color> cellColors = new Dictionary<int, Color>();

        //Объект Manager для исполнения программных функций над получеными от сервера данными
        public GameManager Manager;

        public WebSocketClient(string url, GameManager gameManager) : base(url)
        {
            this.Manager = gameManager;

            //Инициалиизация словаря
            //Для обработки данных типа Color метод DataPlayerColor
            this.dataHandler.Add("Color", DataPlayerColor);
            //Для обработки данных типа Number метод DataNumber
            this.dataHandler.Add("Number", DataNumber);
            //Для обработки данных типа Win метод DataWin
            this.dataHandler.Add("Win", DataWin);
            //Для обработки данных типа Move метод DataMove
            this.dataHandler.Add("Move", DataMove);

            //Инициализация словаря
            //Red = красный
            this.playerColors.Add("Red", Color.Red);
            //Blue = синий
            this.playerColors.Add("Blue", Color.Blue);

            //Инициализация словаря
            //0 - серый
            this.cellColors.Add(0, Color.Gainsboro);
            //1 - красный
            this.cellColors.Add(1, Color.Red);
            //2 - синий
            this.cellColors.Add(2, Color.Blue);

            //При получении сообщения от сервера, оно обрабатывается методом MessageHandler
            this.OnMessage += MessageHandler;
            //При подключении к серверу оповещение
            this.OnOpen += (sender, e) => MessageBox.Show("Connected to server");
            //При отключении от сервера оповещение
            this.OnClose += (sender, e) => MessageBox.Show("Connection closed");

            //Подключиться к серверу
            this.Connect();
        }

        //Обработчик данных типа Color
        public void DataPlayerColor(object data)
        {
            string str = (string)data;
            //Определение цвета игрока
            Manager.GetPlayerColor(playerColors[str]);
        }

        //Обработчик данных типа Number
        public void DataNumber(object data)
        {
            int number = (int)(long)data;
            //Отображение числа у игрока
            Manager.ChangeNumber(number);
        }

        //Обработчик данных типа Move
        public void DataMove(object data)
        {
            //Десериализация fieldMap из формата JSON
            var fieldMap = JsonConvert.DeserializeObject<int[][]>(JsonConvert.SerializeObject(data));

            //Очки игроков
            int redScore = 0;
            int blueScore = 0;

            //Преобразование поля field по fieldMap
            for (int i = 0; i < Manager.field.Rows; i++)
            {
                for (int j = 0; j < Manager.field.Columns; j++)
                {
                    //Покраска выделенных на fieldMap ячеек в соответствующий цвет
                    Manager.field.cells[i][j].BackColor = cellColors[fieldMap[i][j]];
                    //Если ячейка уже занята блокировка нажатия
                    if (fieldMap[i][j] != 0)
                        Manager.field.cells[i][j].Click -= Manager.OnClick;

                    //Подсчет ячеек каждого из игроков на поле field
                    switch (fieldMap[i][j])
                    {
                        case 1:
                            redScore++;
                            break;
                        case 2:
                            blueScore++;
                            break;

                    }
                }
            }

            //Фиксация актуального счета игроков(+получение возможности хода)
            Manager.ChangePlayersScore(redScore, blueScore);
        }

        //Обработчик данных типа Win
        public void DataWin(object data)
        {
            string win = (string)data;

            //Конец игры
            Manager.EndOfGame(win);
        }

        //Метод обработки и распределения данных от сервера
        public void MessageHandler(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                //Десериализация данных от сервера
                var data = JsonConvert.DeserializeObject<Data>(e.Data);
                //Перенаправление данных в нужный метод при помощи словаря
                dataHandler[data.type](data.data);
            }
        }

        //Отправка данных на сервер
        public void SendToServer(string type, object obj)
        {
            var data = new Data();
            //Тип данных
            data.type = type;
            //Данные
            data.data = obj;

            //Сериализация и отправка данных на сервер
            this.Send(JsonConvert.SerializeObject(data));
        }
    }
}
