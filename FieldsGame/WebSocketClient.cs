using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace FieldsGame
{
    delegate void DataTypeHandler(object data);
    class WebSocketClient : WebSocket
    {
        Dictionary<string, DataTypeHandler> dataHandler = new Dictionary<string, DataTypeHandler>();
        Dictionary<string, Color> playerColors = new Dictionary<string, Color>();
        Dictionary<int, Color> cellColors = new Dictionary<int, Color>();

        public GameManager Manager;

        public WebSocketClient(string url, GameManager gameManager) : base(url)
        {
            this.Manager = gameManager;

            this.dataHandler.Add("Color", DataPlayerColor);
            this.dataHandler.Add("Number", DataNumber);
            this.dataHandler.Add("Win", DataWin);
            this.dataHandler.Add("Move", DataMove);

            this.playerColors.Add("Red", Color.Red);
            this.playerColors.Add("Blue", Color.Blue);

            this.cellColors.Add(0, Color.Gainsboro);
            this.cellColors.Add(1, Color.Red);
            this.cellColors.Add(2, Color.Blue);

            this.OnMessage += MessageHandler;
            this.OnOpen += (sender, e) => MessageBox.Show("Connected to server");

            this.Connect();
        }

        public void DataPlayerColor(object data)
        {
            string str = (string)data;
            Manager.GetPlayerColor(playerColors[str]);//Определение цвета игрока; если цвет RED получает право хода
        }
        public void DataNumber(object data)
        {
            int number = (int)(long)data;
            Manager.ChangeNumber(number);
        }
        public void DataMove(object data)
        {
            var fieldMap = JsonConvert.DeserializeObject<int[][]>(JsonConvert.SerializeObject(data));
            int redScore = 0;
            int blueScore = 0;

            for (int i = 0; i < Manager.field.Rows; i++)
            {
                for (int j = 0; j < Manager.field.Columns; j++)
                {
                    Manager.field.cells[i][j].BackColor = cellColors[fieldMap[i][j]];
                    if (fieldMap[i][j] != 0)
                        Manager.field.cells[i][j].Click -= Manager.OnClick;

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
                
            Manager.ChangePlayersScore(redScore, blueScore);//+получение возможности хода
        }

        public void DataWin(object data)
        {
            string win = (string)data;
            //сообщение ПОБЕДИЛ {PlayerColor}(цвет победителя)
            //обнуление полей ; инициализация по новой

            Manager.EndOfGame(win);
        }

        public void MessageHandler(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                var data = JsonConvert.DeserializeObject<Data>(e.Data);
                dataHandler[data.type](data.data);
            }
        }



        public void SendToServer(string type, object obj)
        {
            var data = new Data();
            data.type = type;
            data.data = obj;

            this.Send(JsonConvert.SerializeObject(data));
        }
    }
}
