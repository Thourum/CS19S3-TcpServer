using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using BookLib;
using System.Collections.Generic;

namespace TcpServer
{
    class Server
    {
        TcpListener server = null;
        List<Book> BooksList = Singleton.GetInstance.BooksList;

        public Server(string ip, int port)
        {
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            StartListener();
        }

        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
                server.Stop();
            }
        }

        public void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();

            string data = null;
            byte[] bytes = new byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    byte[] reply = Encoding.ASCII.GetBytes(OnDataRecived(data.Trim()));
                    stream.Write(reply, 0, reply.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
                client.Close();
            }
        }

        private string OnDataRecived(string data)
        {
            var responseMsg = "";
            Console.WriteLine($"DeviceId: {Thread.CurrentThread.ManagedThreadId}\t Data(Recivied): '{data}'");
            var commands = data.Split(null, 2);
            switch (commands[0].ToLower())
            {
                case "getall":
                    responseMsg = JsonSerializer.Serialize(BooksList);
                    break;
                case "get":
                    responseMsg = string.IsNullOrEmpty(commands[1]) ?
                        "You need to provide additional Parameters (Get <ISBN_Number>)" :
                        JsonSerializer.Serialize(BooksList.Find(x => x.ISBN13 == commands[1]));
                    break;
                case "save":
                    if (string.IsNullOrEmpty(commands[1]))
                        responseMsg = "You need to provide additional Parameters (Get <ISBN_Number>)";

                    try
                    {
                        Book book = JsonSerializer.Deserialize<Book>(commands[1]);
                        book.Validate();
                        if (BooksList.Exists(x => x.ISBN13 == book.ISBN13))
                        {
                            responseMsg = "Book with that ISBN already exists!";
                        }
                        else
                        {
                            BooksList.Add(book);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        responseMsg = e.ToString();
                    }
                    break;
                case "?":
                    responseMsg = "Aviable Commands: GetAll, Get, Save";
                    break;
                default:
                    Console.WriteLine($"DeviceId: {Thread.CurrentThread.ManagedThreadId}\t Data recived does not any known command! ({data})");
                    break;
            }

            Console.WriteLine($"DeviceId: {Thread.CurrentThread.ManagedThreadId}\t Data(Sent): '{responseMsg}'");
            return responseMsg + '\n';
        }
    }
}
