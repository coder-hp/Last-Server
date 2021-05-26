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

        // 心跳包标记
        public string m_heartBeatFlag = "*HeartBeat*";
        public float m_heartBeatTime = 2.0f;

        int byteLength = 1024;

        Socket m_socket;
        IPAddress m_ipAddress;
        int m_ipPort;
        bool m_isStart = false;
        bool isShowLog = false;

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
                CommonUtil.Log("Socket启动失败:" + ex);
            }
        }

        public void Stop()
        {
            try
            {
                m_isStart = false;
                m_socket.Close();

                CommonUtil.Log("Socket停止成功:");
            }
            catch (Exception ex)
            {
                CommonUtil.Log("Socket停止异常:" + ex);
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

                CommonUtil.Log("ipPort:" + m_ipPort);
                CommonUtil.Log("服务器启动成功...");

                // 心跳检测
                //ClientInfoManager.startCheckHeartBeat();

                while (m_isStart)
                {
                    Socket socketToClient = m_socket.Accept();

                    Task t = new Task(() => { OnAccept(ClientInfoManager.addClientInfo(socketToClient)); });
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log("SocketServer监听异常:" + ex);
            }
        }

        // 有客户端连接
        void OnAccept(object clientInfo)
        {
            try
            {
                ClientInfo client = (ClientInfo)clientInfo;

                CommonUtil.Log("有客户端连接：id=" + client.m_id + "   ip=" + client.m_ip);

                // 接收消息
                OnReceive(clientInfo);
            }
            catch (Exception ex)
            {
                CommonUtil.Log("OnAccept异常:" + ex);
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
                        //CommonUtil.Log(string.Format("收到客户端{0}原生数据{1}  size={2}", client.m_id, reces, reces.Length));

                        // 过滤http非法请求
                        if (reces.StartsWith("GET") || reces.StartsWith("POST"))
                        {
                            CommonUtil.Log("收到客户端GET/POST非法请求，主动断开链接:" + client.m_id);
                            DisconnectWithClient(client,true);
                            break;
                        }
                        else
                        {
                            reces = client.m_endStr + reces;
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

                                client.m_endStr = "";
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

                                client.m_endStr = list[list.Count - 1];
                            }
                        }
                    }
                    // 与客户端断开连接
                    else
                    {
                        CommonUtil.Log("与客户端断开连接,id=" + client.m_id);
                        DisconnectWithClient(client);

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log("OnReceive异常：" + ex);
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

                if (isShowLog)
                {
                    CommonUtil.Log("返回消息给客户端" + clientInfo.m_id + "：" + data);
                }
                // 增加数据包尾部标识
                data += m_packEndFlag;

                Socket socketToClient = clientInfo.m_socketToClient;

                byte[] bytes = Encoding.UTF8.GetBytes(data);
                if (socketToClient.Connected)
                {
                    socketToClient.Send(bytes);
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log("Send异常：" + ex);
            }
        }

        public void DisconnectWithClient(ClientInfo client,bool isCloseConnect = false)
        {
            CommonUtil.Log("客户端断开：" + client.m_id);
            ClientInfoManager.deleteClientInfo(client);

            if(isCloseConnect)
            {
                client.m_socketToClient.Close();
            }
            if (m_onSocketEvent_Disconnect != null)
            {
                m_onSocketEvent_Disconnect(client);
            }
        }
    }
}