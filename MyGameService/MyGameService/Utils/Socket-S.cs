using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketUtil
{
    class Socket_S
    {
        //public delegate void OnSocketEvent_StartSuccess();                                      // Socket启动成功
        //public delegate void OnSocketEvent_StartFail(string ex);                                // Socket启动失败
        //public delegate void OnSocketEvent_Connect(ClientInfo clientInfo);                      // 有客户端连接
        public delegate void OnSocketEvent_Receive(ClientInfo clientInfo, string data);         // 收到客户端消息
        public delegate void OnSocketEvent_Disconnect(ClientInfo clientInfo);                   // 与客户端断开连接
        
        public OnSocketEvent_Receive m_onSocketEvent_Receive = null;
        public OnSocketEvent_Disconnect m_onSocketEvent_Disconnect = null;

        // 数据包尾部标识
        public char m_packEndFlag = (char)1;
        public string m_endStr = "";

        // 心跳包标记
        public string m_heartBeatFlag = "*HeartBeat*";
        public float m_heartBeatTime = 2.0f;

        int byteLength = 1024;

        Socket m_socket;
        IPAddress m_ipAddress;
        int m_ipPort;
        bool m_isStart = false;

        static Socket_S s_instance = null;

        public static Socket_S getInstance()
        {
            if (s_instance == null)
            {
                s_instance = new Socket_S();
            }

            return s_instance;
        }

        public void Start(string ip, int port)
        {
            try
            {
                m_ipAddress = IPAddress.Parse(ip);
                m_ipPort = port;

                m_isStart = true;

                Task t = new Task(() => { Listen(); });
                t.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket启动失败:" + ex);
            }
        }

        public void Stop()
        {
            try
            {
                m_isStart = false;
                m_socket.Close();

                Console.WriteLine("Socket停止成功:");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Socket停止异常:" + ex);
            }
        }

        // 监听客户端请求
        void Listen()
        {
            try
            {
                IPEndPoint iPEndPoint = new IPEndPoint(m_ipAddress, m_ipPort);
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_socket.Bind(iPEndPoint);
                m_socket.Listen(400);

                Console.WriteLine("ipPort:" + m_ipPort);
                Console.WriteLine("服务器启动成功...\n");

                ClientInfoManager.startCheckHeartBeat();


                while (m_isStart)
                {
                    Socket socketToClient = m_socket.Accept();

                    Task t = new Task(() => { OnAccept(ClientInfoManager.addClientInfo(socketToClient)); });
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SocketServer监听异常:" + ex);
            }
        }

        // 有客户端连接
        void OnAccept(object clientInfo)
        {
            try
            {
                ClientInfo client = (ClientInfo)clientInfo;

                Console.WriteLine("有客户端连接：id=" + client.m_id + "   ip=" + client.m_ip + "\n");

                // 接收消息
                OnReceive(clientInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnAccept异常:" + ex);
            }
        }

        void OnReceive(object clientInfo)
        {
            try
            {
                ClientInfo client = (ClientInfo)clientInfo;
                Socket socketToClient = client.m_socketToClient;

                while (true)
                {
                    byte[] rece = new byte[byteLength];
                    int recelong = socketToClient.Receive(rece, rece.Length, 0);
                    if (recelong != 0)
                    {
                        string reces = Encoding.UTF8.GetString(rece, 0, recelong);

                        reces = m_endStr + reces;

                        List<string> list = new List<string>();
                        bool b = CommonUtil.splitStrIsPerfect(reces, list, m_packEndFlag);

                        if (b)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (list[i].CompareTo(m_heartBeatFlag) == 0)
                                {
                                    ClientInfoManager.heartBeat(client);
                                }
                                else
                                {
                                    if (m_onSocketEvent_Receive != null)
                                    {
                                        m_onSocketEvent_Receive(client, list[i]);
                                    }
                                }
                            }

                            m_endStr = "";
                        }
                        else
                        {
                            for (int i = 0; i < list.Count - 1; i++)
                            {
                                if (list[i].CompareTo(m_heartBeatFlag) == 0)
                                {
                                    ClientInfoManager.heartBeat(client);
                                }
                                else
                                {
                                    if (m_onSocketEvent_Receive != null)
                                    {
                                        m_onSocketEvent_Receive(client, list[i]);
                                    }
                                }
                            }

                            m_endStr = list[list.Count - 1];
                        }
                    }
                    // 与客户端断开连接
                    else
                    {
                        Console.WriteLine("与客户端断开连接,id=" + client.m_id + "\n");
                        DisconnectWithClient(client);

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnReceive异常：" + ex);
            }
        }

        public void Send(ClientInfo clientInfo, object dataObj)
        {
            string data = JsonConvert.SerializeObject(dataObj);
            
            try
            {
                if (clientInfo == null)
                {
                    return;
                }

                Console.WriteLine("返回消息给客户端" + clientInfo.m_id + "：" + data + "\n");

                // 增加数据包尾部标识
                data += m_packEndFlag;

                Socket socketToClient = clientInfo.m_socketToClient;

                byte[] bytes = Encoding.UTF8.GetBytes(data);
                socketToClient.Send(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send异常：" + ex);
            }
        }

        public void DisconnectWithClient(ClientInfo client)
        {
            Console.WriteLine("客户端断开：" + client.m_id + "\n");
            ClientInfoManager.deleteClientInfo(client);

            if (m_onSocketEvent_Disconnect != null)
            {
                m_onSocketEvent_Disconnect(client);
            }
        }
    }
}