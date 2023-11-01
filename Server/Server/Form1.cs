using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
namespace Server
{

    public partial class Form1 : Form
    {
        private UdpClient udpServer;
        private bool isRunning = true;

        public Form1()
        {
            InitializeComponent();
            InitializeUdpServer();
        }

        private void InitializeUdpServer()
        {
            udpServer = new UdpClient(5155);
            udpServer.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5155);
                byte[] receivedBytes = udpServer.EndReceive(ar, ref endPoint);
                string receivedData = Encoding.UTF8.GetString(receivedBytes);

                this.Invoke((Action)(() =>
                {
                    ProcessReceivedData(receivedData);
                }));

                if (isRunning)
                {
                    udpServer.BeginReceive(ReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при отриманнi даних: " + ex.Message);
            }
        }

        private void ProcessReceivedData(string receivedData)
        {
            string[] parts = receivedData.Split('|');
            if (parts.Length >= 2)
            {
                string command = parts[0];
                string parameters = parts[1];

                labelCommand.Text = "Отримана команда: " + command;
                labelParam.Text = "Отримані параметри: " + parameters;

                RecogniseCommand(command, parameters);
            }
        }

        private void RecogniseCommand(string command, string parameters)
        {
            string[] parts = parameters.Split(' ');

            Color color = Color.White;

            if (parts.Length > 0 && command != "draw image")
            {
                string colorString = parts[0];
                try
                {
                    int colorValue = Convert.ToInt32(colorString, 16);
                    int r = (colorValue >> 11) & 0x1F;
                    int g = (colorValue >> 5) & 0x3F;
                    int b = colorValue & 0x1F;
                    color = Color.FromArgb((r << 3), (g << 2), (b << 3));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка при розбірі кольору: " + ex.Message);
                }
            }

            switch (command)
            {
                case "get time":
                    string stringTime = parts[3] + " " + parts[4];
                    DateTime clientTime = DateTime.Parse(stringTime);
                    GetTime(int.Parse(parts[1]), int.Parse(parts[2]), clientTime, color);
                    break;
                case "clear display":
                    splitContainer1.Panel2.BackColor = color;
                    break;
                case "draw pixel":
                    DrawPixel(int.Parse(parts[1]), int.Parse(parts[2]), color);
                    break;
                case "draw line":
                    DrawLine(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color);
                    break;
                case "draw rectangle":
                    DrawRectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color);
                    break;
                case "fill rectangle":
                    FillRectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color);
                    break;
                case "draw ellipse":
                    DrawEllipse(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color);
                    break;
                case "fill ellipse":
                    FillEllipse(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color);
                    break;
                case "draw circle":
                    DrawCircle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), color);
                    break;
                case "fill circle":
                    FillCircle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), color);
                    break;
                case "draw rounded rectangle":
                    DrawRoundedRectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), color);
                    break;
                case "fill rounded rectangle":
                    FillRoundedRectangle(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), color);
                    break;
                case "draw text":
                    DrawText(int.Parse(parts[1]), int.Parse(parts[2]), color, parts[3]);
                    //DrawText(int.Parse(parts[1]), int.Parse(parts[2]), color, int.Parse(parts[3]), int.Parse(parts[4]), parts[5]);
                    break;
                case "draw image":
                        string imagePath = parts[0];
                        DrawImage(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), imagePath);
                    break;
                default:
                    Console.WriteLine("Невідома команда: " + command);
                    break;
            }
        }

        private void GetTime(int x0, int y0, DateTime clientTime, Color color)
        {
            Label labelTime = new Label();
            labelTime.ForeColor = color;
            labelTime.Text = clientTime.ToString("HH:mm:ss");
            labelTime.Font = new Font("Times New Roman", 30, FontStyle.Bold);
            labelTime.AutoSize = true;
            labelTime.Location = new Point(x0, y0);
            splitContainer1.Panel2.Controls.Add(labelTime);

            Label labelDate = new Label();
            labelDate.Text = clientTime.ToString("dd.MM") + "\n" + clientTime.Year.ToString();
            labelDate.Location = new Point(x0 + 165, y0);
            labelDate.Font = new Font("Times New Roman", 15, FontStyle.Regular);
            labelDate.AutoSize = true;
            splitContainer1.Panel2.Controls.Add(labelDate);

            timer1.Tick += (sender, e) =>
            {
                clientTime = clientTime.AddSeconds(1);
                labelTime.Text = clientTime.ToString("HH:mm:ss");
            };
        }

        private void DrawPixel(int x, int y, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Brush brush = new SolidBrush(color);

            g.FillRectangle(brush, x, y, 1, 1);

            brush.Dispose();
            g.Dispose();
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2);

            g.DrawLine(pen, x0, y0, x1, y1);

            pen.Dispose();
            g.Dispose();
        }

        private void DrawRectangle(int x0, int y0, int width, int height, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2); 

            g.DrawRectangle(pen, x0, y0, width, height);

            pen.Dispose();
            g.Dispose();
        }

        private void FillRectangle(int x0, int y0, int width, int height, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Brush brush = new SolidBrush(color);

            g.FillRectangle(brush, x0, y0, width, height);

            brush.Dispose();
            g.Dispose();
        }

        private void DrawEllipse(int x0, int y0, int radius_x, int radius_y, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2);

            g.DrawEllipse(pen, x0 - radius_x, y0 - radius_y, radius_x * 2, radius_y * 2);

            pen.Dispose();
            g.Dispose();
        }

        private void FillEllipse(int x0, int y0, int radius_x, int radius_y, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Brush brush = new SolidBrush(color);

            g.FillEllipse(brush, x0 - radius_x, y0 - radius_y, radius_x * 2, radius_y * 2);

            brush.Dispose();
            g.Dispose();
        }

        private void DrawCircle(int x0, int y0, int radius, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2);

            g.DrawEllipse(pen, x0 - radius, y0 - radius, radius * 2, radius * 2);

            pen.Dispose();
            g.Dispose();
        }

        private void FillCircle(int x0, int y0, int radius, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Brush brush = new SolidBrush(color);

            g.FillEllipse(brush, x0 - radius, y0 - radius, radius * 2, radius * 2);

            brush.Dispose();
            g.Dispose();
        }

        private void DrawRoundedRectangle(int x0, int y0, int width, int height, int radius, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2); 

            GraphicsPath path = new GraphicsPath();
            path.AddArc(x0, y0, radius, radius, 180, 90); 
            path.AddArc(x0 + width - radius, y0, radius, radius, 270, 90);
            path.AddArc(x0 + width - radius, y0 + height - radius, radius, radius, 0, 90);
            path.AddArc(x0, y0 + height - radius, radius, radius, 90, 90); 
            path.CloseFigure(); 
            g.DrawPath(pen, path);

            pen.Dispose();
            g.Dispose();
        }

        private void FillRoundedRectangle(int x0, int y0, int width, int height, int radius, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();

            GraphicsPath path = new GraphicsPath();
            path.AddArc(x0, y0, radius, radius, 180, 90); 
            path.AddArc(x0 + width - radius, y0, radius, radius, 270, 90); 
            path.AddArc(x0 + width - radius, y0 + height - radius, radius, radius, 0, 90); 
            path.AddArc(x0, y0 + height - radius, radius, radius, 90, 90); 
            path.CloseFigure(); 
            g.FillPath(new SolidBrush(color), path);

            g.Dispose();
        }

        private void DrawText(int x0, int y0, Color color, string text)
        {
            int x = x0;
            int y = y0;

            foreach (char letter in text)
            {
                switch (letter)
                {
                    case 'a':
                        DrawLetterA(x, y, color);
                        x += 25;
                        break;
                    case 'b':
                        DrawLetterB(x, y, color);
                        x += 25;
                        break;
                    case 'c':
                        DrawLetterC(x, y, color);
                        x += 25;
                        break;
                    case 'd':
                        DrawLetterD(x, y, color);
                        x += 25;
                        break;
                    case 'e':
                        DrawLetterE(x, y, color);
                        x += 25;
                        break;
                    case 'h':
                        DrawLetterH(x, y, color);
                        x += 25;
                        break;
                    case 'l':
                        DrawLetterL(x, y, color);
                        x += 5;
                        break;
                    case 'o':
                        DrawLetterO(x, y, color);
                        x += 25;
                        break;
                    case 'r':
                        DrawLetterR(x, y, color);
                        x += 20;
                        break;
                    case 'w':
                        DrawLetterW(x, y, color);
                        x += 45;
                        break;
                    case '_':
                        DrawLetter_(x, y, color);
                        x += 25;
                        break;
                }
            }
        }

        private void DrawLetterA(int x, int y, Color color)
        {
            DrawLine(x + 5, y + 10, x + 20, y + 10, color);
            DrawLine(x + 20, y + 10, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x + 2, y + 40, color);
            DrawLine(x + 2, y + 40, x + 2, y + 20, color);
            DrawLine(x + 2, y + 20, x + 20, y + 20, color);
        }

        private void DrawLetterB(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color); 
            DrawLine(x, y + 40, x + 20, y + 40, color); 
            DrawLine(x + 20, y + 40, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x, y + 20, color);
        }

        private void DrawLetterC(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawLetterD(int x, int y, Color color)
        {
            DrawLine(x + 20, y, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x + 2, y + 40, color);
            DrawLine(x + 2, y + 40, x + 2, y + 20, color);
            DrawLine(x + 2, y + 20, x + 20, y + 20, color);
        }

        private void DrawLetterE(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 30, color);
            DrawLine(x + 20, y + 30, x, y + 30, color);
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawLetterH(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color);
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 40, color);
        }

        private void DrawLetterL(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color);
        }

        private void DrawLetterO(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x, y + 40, color);
            DrawLine(x, y + 40, x, y + 20, color);
        }
        private void DrawLetterR(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 23, x + 15, y + 23, color);
            DrawLine(x + 15, y + 23, x + 15, y + 26, color);
        }
        private void DrawLetterW(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 10, y + 40, color);
            DrawLine(x + 10, y + 40, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 30, y + 40, color);
            DrawLine(x + 30, y + 40, x + 40, y + 20, color);
        }

        private void DrawLetter_(int x, int y, Color color)
        {
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawImage(int x0, int y0, int width, int height, string imagePath)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();

            Image image = Image.FromFile(imagePath);
            g.DrawImage(image, x0, y0, width, height);

            image.Dispose();
            g.Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (udpServer != null)
            {
                udpServer.Close();
            }
        }

        private void buttonStop_Click_1(object sender, EventArgs e)
        {
            isRunning = false;
            udpServer.Close();
        }
    }
}