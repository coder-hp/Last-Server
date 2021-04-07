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

        static void Main(string[] args)
        {
            Socket_S.getInstance().Start("0.0.0.0", 8888);
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

            // 加载用户数据
            {
                string data = FileIO.Read(AppDomain.CurrentDomain.BaseDirectory + "UserInfo.txt");
                if (!string.IsNullOrEmpty(data))
                {
                    UserInfo.UserInfoList = JsonConvert.DeserializeObject<List<UserInfoData>>(data);
                }
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
                        Console.WriteLine("控制台关闭");

                        SaveData();
                    }
                    break;
            }

            return false;
        }

        static void OnReceive(ClientInfo clientInfo, string data)
        {
            Console.WriteLine("收到" + clientInfo .m_id + "消息：" + data);

            DoTaskClientReq.Do(clientInfo, data);
        }

        // 保存用户数据
        static void SaveData()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "UserInfo.txt";
                string data = JsonConvert.SerializeObject(UserInfo.UserInfoList);

                if (File.Exists(path))
                {
                    // 重命名
                    FileIO.Rename(path, AppDomain.CurrentDomain.BaseDirectory + "UserInfo-" + CommonUtil.getCurTimeNoFormat() + ".txt");
                }

                FileIO.Write(path, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
