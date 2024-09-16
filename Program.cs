using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Task3_ChatApp_vSem3
{
    internal class Program
    {
        static private CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Server(string name, CancellationToken cancellationToken)
        {
            UdpClient udpClient = new UdpClient(12345);
            Console.WriteLine("UDP Сервер ожидает сообщений...");
            Console.WriteLine("Нажмите любую клавишу для завершения работы сервера!");

            Task serverTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var recData = await udpClient.ReceiveAsync();
                    byte[] receiveBytes = recData.Buffer;
                    string receivedData = Encoding.UTF8.GetString(receiveBytes);

                    if (receivedData.ToLower().Equals("exit"))
                    {
                        Console.WriteLine("Получено сообщение о завершении работы. Сервер закрывается...");
                        udpClient.Close();
                        cts.Cancel();
                        break;
                    }

                    try
                    {
                        var message = Message.FromJson(receivedData);
                        Console.WriteLine($"Получено сообщение от {message?.FromName} - ({message?.Date}): {message?.Text}");
                        string replyMessage = "Сообщение получено";
                        var replyMessageJson = new Message()
                        {
                            FromName = name,
                            Date = DateTime.Now,
                            Text = replyMessage
                        }.ToJson();

                        byte[] replyBytes = Encoding.UTF8.GetBytes(replyMessageJson);
                        await udpClient.SendAsync(replyBytes, replyBytes.Length, recData.RemoteEndPoint);
                        Console.WriteLine("Ответ отправлен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                    }
                }
            }, cancellationToken);

            Console.ReadKey(true);
            cts.Cancel();
            udpClient.Close();
        }

        static async Task Client(string name, string ip, CancellationTokenSource cts)
        {
            UdpClient udpClient = new UdpClient();
            Console.WriteLine("UDP Клиент запущен...");

            Task clientTask = Task.Run(async () =>
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345);
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("Введите сообщение или Exit для выхода из клиента");
                        string? message = Console.ReadLine();
                        if (message!.ToLower().Equals("exit"))
                        {
                            byte[] exitBytes = Encoding.UTF8.GetBytes("exit");
                            await udpClient.SendAsync(exitBytes, exitBytes.Length, remoteEndPoint);
                            udpClient.Close();
                            cts.Cancel();
                            break;
                        }

                        var messageJson = new Message()
                        {
                            FromName = name,
                            Date = DateTime.Now,
                            Text = message
                        }.ToJson();

                        byte[] replyBytes = Encoding.UTF8.GetBytes(messageJson);
                        await udpClient.SendAsync(replyBytes, replyBytes.Length, remoteEndPoint);
                        Console.WriteLine("Сообщение отправлено.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка при обработке сообщения: " + ex.Message);
                    }

                    var recData = await udpClient.ReceiveAsync();
                    string receivedData = Encoding.UTF8.GetString(recData.Buffer);
                    var messageReceived = Message.FromJson(receivedData);
                    Console.WriteLine($"Получено сообщение от {messageReceived?.FromName} - ({messageReceived?.Date}): {messageReceived?.Text}");
                }
            }, cts.Token);

            await clientTask;
        }

        static async Task Main(string[] args)
        {
            if (args.Length == 1)
            {
                await Server(args[0], cts.Token);
            }
            else if (args.Length == 2)
            {
                await Client(args[0], args[1], cts);
            }
            else
            {
                Console.WriteLine("Для запуска сервера введите ник-нейм как параметр запуска приложения");
                Console.WriteLine("Для запуска клиента введите ник-нейм и IP сервера как параметры запуска приложения");
            }
        }
    }
}