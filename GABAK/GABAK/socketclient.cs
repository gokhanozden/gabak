//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace GABAK
{
    class socketclient
    {
        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        // The response from the remote device.
        private String response = String.Empty;
        byte[] m_clientData;
        List<byte[]> filelist;
        IPHostEntry ipHostInfo;
        private int share;
        private int load = 0;

        public socketclient(string ipAddress, int p_share)
        {
            filelist = new List<byte[]>();
            ipHostInfo = Dns.Resolve(ipAddress);
            share = p_share;
        }

        public void addLoad(int p_load)
        {
            load += p_load;
        }

        public int getLoad()
        {
            return load;
        }

        public int getShare()
        {
            return share;
        }

        public void addFile(string m_fName, string txtfile)
        {
            byte[] clientData;
            // Send test data to the remote device.
            byte[] fileName = Encoding.ASCII.GetBytes(m_fName); //file name
            byte[] fileData = File.ReadAllBytes(txtfile); //file
            byte[] fileNameLen = BitConverter.GetBytes(fileName.Length); //lenght of file name
            clientData = new byte[4 + fileName.Length + fileData.Length];
            fileNameLen.CopyTo(clientData, 0);
            fileName.CopyTo(clientData, 4);
            fileData.CopyTo(clientData, 4 + fileName.Length);
            filelist.Add(clientData);
        }

        public string returnSolution(bool p_concorde)
        {
            connectDone.Reset();
            sendDone.Reset();
            receiveDone.Reset();
            if (p_concorde)
            {
                StartClient(8888);
            }
            else
            {
                StartClient(8889);
            }
            filelist.Clear();
            return response;
        }
        private void StartClient(int portnumber)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, portnumber);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                int totalfilesize = 0;
                for (int i = 0; i < filelist.Count; i++)
                {
                    totalfilesize += filelist[i].Length;
                }
                m_clientData = new byte[totalfilesize+4];
                //Copy all file data to filesend
                int currentfilesizeindex = 0;
                for (int i = 0; i < filelist.Count; i++)
                {
                    filelist[i].CopyTo(m_clientData, currentfilesizeindex);
                    currentfilesizeindex += filelist[i].Length;
                }

                byte[] donesignal = Encoding.ASCII.GetBytes("DONE");
                donesignal.CopyTo(m_clientData, totalfilesize);
                string temp = Encoding.ASCII.GetString(m_clientData);
                Send(client, m_clientData);
                sendDone.WaitOne();

                // Receive the response from the remote device.
                Receive(client);
                receiveDone.WaitOne();
                //MethodInvoker action = delegate { textBoxOutput.Text += response + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);

                // Release the socket.
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += e.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);
            }

        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                try
                {
                    client.EndConnect(ar);
                }
                catch (Exception e)
                {

                }

                //Console.WriteLine("Socket connected to {0}",client.RemoteEndPoint.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += "Socket connected to " + client.RemoteEndPoint.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += e.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                stateobject state = new stateobject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, stateobject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += e.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            string content = "";
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                stateobject state = (stateobject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    content = state.sb.ToString();
                    if (content.IndexOf("EOF") > -1)
                    {
                        //All the data has arrived; put it in response.
                        if (state.sb.Length > 1)
                        {
                            response = state.sb.ToString(0, state.sb.Length - 3);
                        }
                        // Signal that all bytes have been received.
                        receiveDone.Set();
                    }
                    else
                    {
                        // Get the rest of the data.
                        client.BeginReceive(state.buffer, 0, stateobject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                //Console.WriteLine(e.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += e.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);
            }

        }


        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void Send(Socket client, byte[] byteData)
        {
            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                //MethodInvoker action = delegate { textBoxOutput.Text += "Sent " + bytesSent.ToString() + " bytes to server.\r\n"; };
                //textBoxOutput.BeginInvoke(action);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                //MethodInvoker action = delegate { textBoxOutput.Text += e.ToString() + "\r\n"; };
                //textBoxOutput.BeginInvoke(action);
            }
        }
    }
}
