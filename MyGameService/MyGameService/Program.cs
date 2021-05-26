using MyGameService.Net;
using Newtonsoft.Json;
using SocketUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService
{
    public delegate bool ConsoleCtrlDelegate(int dwCtrlType);

    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        //当用户关闭Console时，系统会发送次消息  
        private const int CTRL_CLOSE_EVENT = 2;

        static bool isShowLog = false;

        static void Main(string[] args)
        {
            // 读取文件
            {
                StreamReader sr = new StreamReader("data/config.json", System.Text.Encoding.GetEncoding("utf-8"));
                string config = sr.ReadToEnd().ToString();
                sr.Close();

                ConfigEntity.getInstance().data = JsonConvert.DeserializeObject<ConfigData>(config);
            }

            if (true)
            {
                var configData = ConfigEntity.getInstance().data;
                Socket_S.getInstance().Start("0.0.0.0", configData.server_port);
                Socket_S.getInstance().m_onSocketEvent_Receive = OnReceive;

                {
                    // 用API安装事件处理  
                    ConsoleCtrlDelegate newDelegate = new ConsoleCtrlDelegate(HandlerRoutine);
                    bool bRet = SetConsoleCtrlHandler(newDelegate, true);
                    if (bRet == false)
                    {
                        // 安装关闭事件失败
                    }
                    else
                    {
                        // 安装关闭事件成功
                    }
                }

                MySqlUtil.getInstance().openDatabase();
            }

            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
        }

        private static bool HandlerRoutine(int CtrlType)
        {
            switch (CtrlType)
            {
                case CTRL_CLOSE_EVENT:       //用户要关闭Console了  
                    {
                        CommonUtil.Log("控制台关闭");
                    }
                    break;
            }

            return false;
        }

        static void OnReceive(ClientInfo clientInfo, string data)
        {
            string[] data_split = data.Split('{');
            if (data_split.Length == 2 && data_split[0] == "")
            {
                if (isShowLog)
                {
                    CommonUtil.Log("收到" + clientInfo.m_id + "消息：" + data);
                }
                DoTaskClientReq.Do(clientInfo, data);
            }
            else if(data_split.Length > 0)
            {
                data_split[data_split.Length - 1] = "{" + data_split[data_split.Length - 1];

                if (isShowLog)
                {
                    CommonUtil.Log("收到" + clientInfo.m_id + "异常消息：" + data);
                    CommonUtil.Log("收到" + clientInfo.m_id + "异常消息转换后：" + data_split[data_split.Length - 1]);
                }
                DoTaskClientReq.Do(clientInfo, data_split[data_split.Length - 1]);
            } 
        }
    }
}
