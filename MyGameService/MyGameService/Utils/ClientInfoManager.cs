using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketUtil
{
    public class ClientInfo
    {
        public int m_id;
        public string m_ip;
        public Socket m_socketToClient;
        public string m_lastHeratBeatTime;
        public string m_endStr = "";

        public ClientInfo(int id, Socket socketToClient)
        {
            m_id = id;
            m_socketToClient = socketToClient;

            m_ip = ((IPEndPoint)m_socketToClient.RemoteEndPoint).Address.ToString();

            m_lastHeratBeatTime = CommonUtil.getCurTimeNormalFormat();
        }
    }

    public class ClientInfoManager
    {
        static int s_id = 0;
        static List<ClientInfo> s_clientList = new List<SocketUtil.ClientInfo>();

        public static ClientInfo addClientInfo(Socket socketToClient)
        {
            // id最高排到20亿
            {
                ++s_id;
                if (s_id > 2000000000)
                {
                    s_id = 1;
                }
            }

            ClientInfo client = new ClientInfo(s_id, socketToClient);
            s_clientList.Add(client);

            return client;
        }

        public static ClientInfo getClientInfoById(int id)
        {
            for (int i = 0; i < s_clientList.Count; i++)
            {
                if (s_clientList[i].m_id == id)
                {
                    return s_clientList[i];
                }
            }

            return null;
        }

        public static void deleteClientInfo(ClientInfo clientInfo)
        {
            s_clientList.Remove(clientInfo);
        }

        public static int getClientConnectCount()
        {
            return s_clientList.Count;
        }

        public static void heartBeat(ClientInfo clientInfo)
        {
            clientInfo.m_lastHeratBeatTime = CommonUtil.getCurTimeNormalFormat();
        }

        public static void startCheckHeartBeat()
        {
            Timer m_timer = new Timer(onTimerCheckHeartBeat, "", (int)(Socket_S.getInstance().m_heartBeatTime * 1000), (int)(Socket_S.getInstance().m_heartBeatTime * 1000));
        }

        static void onTimerCheckHeartBeat(object data)
        {
            for (int i = s_clientList.Count - 1; i >= 0 ; i--)
            {
                int miaoshucha = CommonUtil.miaoshucha(s_clientList[i].m_lastHeratBeatTime, CommonUtil.getCurTimeNormalFormat());
                if (miaoshucha > (Socket_S.getInstance().m_heartBeatTime * 3))
                {
                    Socket_S.getInstance().DisconnectWithClient(s_clientList[i]);
                }
            }
        }
    }
}