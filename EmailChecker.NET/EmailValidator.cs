using DnsClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EmailCheckerNET
{
    public class EmailValidator
    {
        private const int PORT_NUMBER = 25;
        private const string MAIL_FROM = "batman@gmail.com";

        public static bool CheckMailBox(string emailAddress)
        {
            if (ValidateEmail(emailAddress) == false)
            {
                return false;
            }

            string[] MX_Records = FindMXRecords(emailAddress);
            foreach (string hostNameOrAddress in MX_Records)
            {
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
                    IPEndPoint iPEndPoint = new IPEndPoint(hostEntry.AddressList[0], PORT_NUMBER);
                    Socket socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(iPEndPoint);
                    if (GetResponseCode(socket) == 220)
                    {
                        Senddata(socket, string.Format("HELO {0}\r\n", Dns.GetHostName()));
                        bool result;
                        if (GetResponseCode(socket) != 250)
                        {
                            result = false;
                            return result;
                        }
                        Senddata(socket, string.Format("MAIL FROM:<{0}>\r\n", MAIL_FROM));
                        if (GetResponseCode(socket) != 250)
                        {
                            result = false;
                            return result;
                        }
                        Senddata(socket, string.Format("RCPT TO:<{0}>\r\n", emailAddress));
                        if (GetResponseCode(socket) == 250)
                        {
                            Senddata(socket, "QUIT\r\n");
                            socket.Close();
                            result = true;
                            return result;
                        }
                        result = false;
                        return result;
                    }
                    else
                    {
                        socket.Close();
                    }
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        private static int GetResponseCode(Socket socket)
        {
            byte[] array = new byte[1024];
            int counter = 0;
            while (socket.Available == 0)
            {
                Thread.Sleep(100);
                counter++;
                if (counter > 30)
                {
                    socket.Close();
                    return 0;
                }
            }
            socket.Receive(array, 0, socket.Available, SocketFlags.None);
            return Convert.ToInt32(Encoding.ASCII.GetString(array).Substring(0, 3));
        }

        private static void Senddata(Socket socket, string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            socket.Send(bytes, 0, bytes.Length, SocketFlags.None);
        }

        private static string[] FindMXRecords(string email_Adress)
        {
            List<string> mx_Records = new List<string>();
            string domain = email_Adress.Substring(email_Adress.IndexOf("@") + 1);

            var client = new LookupClient();
            var result = client.Query(domain, QueryType.MX);
            foreach (DnsClient.Protocol.MxRecord record in result.Answers)
            {
                mx_Records.Add(record.Exchange.Value);
            }
            
            return mx_Records.ToArray();
        }

        private static bool ValidateEmail(string email_Adress)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email_Adress);
                return addr.Address == email_Adress;
            }
            catch
            {
                return false;
            }
        }
    }
}
