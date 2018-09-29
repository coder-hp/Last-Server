using MyGameService.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketUtil
{
    class DoTaskClientReq
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            try
            {
                CSBaseData csBaseData = JsonConvert.DeserializeObject<CSBaseData>(data);
                
                switch (csBaseData.Tag)
                {
                    case (int)Consts.NetTag.Login:
                        {
                            DoTask_Login.Do(clientInfo, data);
                        }
                        break;

                    case (int)Consts.NetTag.UserInfo:
                        {
                            DoTask_UserInfo.Do(clientInfo, data);
                        }
                        break;

                    case (int)Consts.NetTag.ChangeEquip:
                        {
                            DoTask_ChangeEquip.Do(clientInfo, data);
                        }
                        break;

                    case (int)Consts.NetTag.Sign:
                        {
                            DoTask_Sign.Do(clientInfo, data);
                        }
                        break;
                        
                    default:
                        {
                            Console.WriteLine("未知tag，不予处理：" + data);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("客户端传的数据有问题：" + data);
            }
        }
    }
}
