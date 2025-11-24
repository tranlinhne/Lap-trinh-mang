using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace ChatServer
{
    class Program
    {
        private static Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
        private const int Port = 9001;

        static async Task Main(string[] args)
        {
            TcpListener listener;
            if (args.Length > 0)
                listener = new TcpListener(IPAddress.Parse(args[0]), Port);
            else
                listener = new TcpListener(IPAddress.Any, Port);

            listener.Start();
            Console.WriteLine($"Server dang lang nghe tren cong {Port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            string username = "Unknown";
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];

                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return;

                string userJoinMessage = AesEncryption.Decrypt(buffer.Take(bytesRead).ToArray());

                if (userJoinMessage.StartsWith("USER:"))
                {
                    username = userJoinMessage.Substring(5);
                    clients.Add(client, username);
                    Console.WriteLine($"{username} da ket noi.");
                    await BroadcastMessage($"{username} da tham gia phong chat.", client);
                    await BroadcastUserList();
                }
                else
                {
                    client.Close();
                    return;
                }

                while (true)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string decryptedMessage = AesEncryption.Decrypt(buffer.Take(bytesRead).ToArray());

                    if (decryptedMessage.StartsWith("MSG:"))
                    {
                        string chatMessage = decryptedMessage.Substring(4);
                        Console.WriteLine($"[{username}]: {chatMessage}");
                        await BroadcastMessage($"{username}: {chatMessage}", client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi: {ex.Message}");
            }
            finally
            {
                if (clients.ContainsKey(client))
                {
                    clients.Remove(client);
                    Console.WriteLine($"{username} da ngat ket noi.");
                    await BroadcastMessage($"{username} da roi phong chat.", null);
                    await BroadcastUserList();
                    client.Close();
                }
            }
        }

        private static async Task BroadcastMessage(string message, TcpClient sender)
        {
            byte[] encryptedMessage = AesEncryption.Encrypt($"MSG:{message}");

            foreach (var client in clients.Keys)
            {
                if (client != sender)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        await stream.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);
                    }
                    catch { }
                }
            }
        }

        private static async Task BroadcastUserList()
        {
            var usernames = clients.Values.ToList();
            string userListMessage = "USERLIST:" + string.Join(",", usernames);
            byte[] encryptedUserList = AesEncryption.Encrypt(userListMessage);

            foreach (var client in clients.Keys)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(encryptedUserList, 0, encryptedUserList.Length);
                }
                catch { }
            }
        }
    }
}
