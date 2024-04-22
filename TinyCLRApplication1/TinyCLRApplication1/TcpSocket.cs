using System.Diagnostics;
using System.Text;
using System.Threading;
using System;
using System.Net;
using System.Net.Sockets;

namespace TinyCLRApplication1
{
    public delegate void OnConnectionMadeEventHandler(TcpSocket sender);
    public delegate void OnDataReceivedEventHandler(TcpSocket sender, DataReceivedEventArgs e);
    public class TcpSocket
    {
        public Socket Socket { get; set; }
        public Thread ConnectionThread { get; set; }
        public Socket ConnectedSocket { get; set; }
        public bool RequiresLogin { get; set; }
        public bool CheckCarriageReturn { get; set; } = true;
        public bool RemoveCarriageReturn { get; set; } = true;

        public int Port { get; set; }
        public bool IsConnected { get; set; }
        
        private bool HasEnteredPassword;
        private string Password;

        protected string Hello = "";
        
        public OnConnectionMadeEventHandler OnConnectionMade;
        public OnDataReceivedEventHandler OnDataReceived;

        /*
         * This constructor creates an unauthenticated socket on the specified port
         */
        public TcpSocket(int port)
        {
            this.Port = port;

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /*
         * This constructor creates an authenticated socket on the specified port with the specified username and password
         */
        public TcpSocket(int port, string password) : this(port)
        {
            RequiresLogin = true;
            this.Password = password;
        }

        /*
         * Running this method will start the socket.
         * It will bind to the set port, start listening and open a new thread if a new connection occurs
         */
        public virtual void Start()
        {
            IPEndPoint Endpoint = new IPEndPoint(IPAddress.Any, Port);

            Socket.Bind(Endpoint);
            Socket.Listen(10);

            ConnectionThread = new Thread(new ThreadStart(OpenConnection));
            ConnectionThread.Start();
        }

        /*
         * This method is inside of the connectionthread.
         * It will check if there is a connection with Socket.Accept().
         * If a connection is made, the method will ask the user for a username if required
         * After this, the connection is handed over to the ProcessRequest method
         */
        public virtual void OpenConnection()
        {
            while (true)
            {
                try
                {
                    //Wait for a connection
                    Socket ClientSocket = Socket.Accept();

                    //If we make it this far, a connection was been established!
                    IsConnected = true;

                    //Save the socketconnection
                    ConnectedSocket = ClientSocket;

                    OnConnectionMade?.Invoke(this);

                    //Send a message to the newly connected client
                    if (Hello.Length > 0)
                        SendMessage(Hello);

                    //Check if this socket requires authentication, ask for a username if it is
                    if (RequiresLogin)
                    {
                        HasEnteredPassword = false;
                        SendMessage("Password:");
                        ConnectedSocket.Send(new byte[] { 0xFF, 0xFB, 0x01 }); //disable echo for while user is entering password
                    }

                    //Have this method handle the rest of the connection
                    ProcessRequest();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("socket error");
                }
            }
        }

        /*
         * This method will handle all of the incoming messages sent by the client
         */
        public virtual void ProcessRequest()
        {
            using (ConnectedSocket)
            {
                string data = string.Empty;
                DateTime lastData = DateTime.Now;

                while (true)
                {
                    try
                    {
                        //Check if there is a new message to process
                        if (ConnectedSocket.Poll(5 * 1000000, SelectMode.SelectRead))
                        {
                            // If the buffer is zero-length, the connection has been closed or terminated.
                            if (ConnectedSocket.Available == 0)
                            {
                                IsConnected = false;
                                break;
                            }

                            //Initialize variables to read the buffer
                            byte[] buffer = new byte[ConnectedSocket.Available];
                            int bytesRead = ConnectedSocket.Receive(buffer, ConnectedSocket.Available, SocketFlags.None);

                            //Make sure to empty the buffer if it hasnt been touched in a while
                            if (lastData.MillisecondsAgo() > 100)
                                data = string.Empty;

                            //Read the data from the buffer
                            data += BytesToString(buffer);
                           
                            if(CheckCarriageReturn)
                               if (!data.Contains("\r") && !data.Contains("\n"))
                                    continue;

                            //If the received data is empty or just a CRLF, don't do anything with it
                            if (data != "" && data != "\r\n")
                            {

                                //Remove all of the CRLFs from the data
                                if(RemoveCarriageReturn)
                                    data = data.Replace("\r\n", "");

                                DataReceivedEventArgs args = new()
                                {
                                    Data = data,
                                    DataBytes = buffer
                                };
                                data = string.Empty;
                                //Check if the client has entered the username and password if required
                                if (RequiresLogin)
                                {
                                    if (!HasEnteredPassword)
                                    {
                                        if (args.Data == Password)
                                        {
                                            //The client was authenticated successfully, we will welcome them
                                            HasEnteredPassword = true;
                                            SendMessage("Welcome.\r\nEnter 'help' to see a list of commands");
                                            ConnectedSocket.Send(new byte[] { 0xFF, 0xFC, 0x01 }); //enable echo again
                                        }
                                        else
                                        {
                                            SendMessage("**Invalid password; re-enter password: ");
                                        }
                                    }
                                    else //Raise an event that will further parse the data
                                        OnDataReceived?.Invoke(this, args);
                                }
                                else //Raise an event that will further parse the data
                                    OnDataReceived?.Invoke(this, args);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        public virtual int SendMessage(byte[] input)
        {
             try
            {
                if (IsConnected)
                    return ConnectedSocket.Send(input);
                else
                    return 0;
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("Socket disposed!");
            }

            return 0;
        }

        /*
         * This method will send a message to the client.
         * It automatically adds a CRLF to the end of each message
         */
        public virtual int SendMessage(string input)
        {
            //Add CRLF
            input += "\r\n";
            //Convert the message to bytes
            byte[] bytes = StringToBytes(input);

            //Send the message, but only if there is a client connected to us
            try
            {
                if (IsConnected)
                    return ConnectedSocket.Send(bytes);
                else
                    return 0;
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("Socket disposed!");
            }

            return 0;
        }

        /*
         * Converts a string to bytes
         */
        private byte[] StringToBytes(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        /*
         * Converts a byte array into a string
         */
        private string BytesToString(byte[] bytes)
        {
            //Create an empty string
            string str = string.Empty;

            //Loop trough all of the bytes and cast them to a char individually; Add them to the string after
            for (int i = 0; i < bytes.Length; ++i)
                str += (char)bytes[i];

            return str;
        }
    }
}