using System.Net;
using System.Net.Sockets;
using System.Text;

namespace slagboom.Droid
{
    class Socketclass
    {
        private string IpAdress { get; set; }
        private int PortNumber { get; set; }
        

        public Socket Open()
        {
            this.IpAdress = "192.168.1.3";
            this.PortNumber = 5007;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress _ip = IPAddress.Parse(this.IpAdress);
            IPEndPoint endPoint = new IPEndPoint(_ip, this.PortNumber);
            try
            {
                socket.Connect(endPoint);
            }
            catch (SocketException)
            {
                System.Diagnostics.Debug.WriteLine ("Connection failed");
            }

            return socket;
        }

        public void Write(Socket socket, string text)
        {
            socket.Send(Encoding.ASCII.GetBytes(text));
        }

        public string Read(Socket socket)
        {
            byte[] bytes = new byte[4096];
            int byterec = socket.Receive(bytes);
            string text = Encoding.ASCII.GetString(bytes, 0, byterec);

            return text;
        }

        public void Close(Socket socket)
        {
            socket.Close();
        }

        public string AskArduino(string question)
        {
            Socket s = Open();
            Write(s, question);
            string reply = Read(s);
            Close(s);
            return reply;
        }
    }
}