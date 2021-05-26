using Newtonsoft.Json;
using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Game
{
    public class WaitMatchUserInfo
    {
        public ClientInfo clientInfo;
        public int userId;

        public WaitMatchUserInfo(ClientInfo _clientInfo, int _userId)
        {
            clientInfo = _clientInfo;
            userId = _userId;
        }
    }

    class MatchLogic
    {
        public static List<WaitMatchUserInfo> waitUserList = new List<WaitMatchUserInfo>();

        public static void addUser(ClientInfo clientInfo,int userId)
        {
            if(!checkIsExist(userId))
            {
                waitUserList.Add(new WaitMatchUserInfo(clientInfo, userId));

                // 该房间需要的人数
                int needUserCount = 2;
                if(waitUserList.Count >= needUserCount)
                {
                    // 新建房间
                    List<WaitMatchUserInfo> userList = new List<WaitMatchUserInfo>();
                    for (int i = 0; i < needUserCount; i++)
                    {
                        userList.Add(waitUserList[i]);
                    }
                    RoomLogic roomLogic = new RoomLogic(userList);

                    S2C_CanEnterGameMode2 s2c = new S2C_CanEnterGameMode2();
                    s2c.Tag = CSParam.NetTag.CanEnterGameMode2.ToString();
                    s2c.Code = (int)CSParam.CodeType.Ok;

                    List<int> allUserId = new List<int>();
                    for(int i = 0; i < needUserCount; i++)
                    {
                        allUserId.Add(waitUserList[i].userId);
                    }
                    s2c.allUserId = allUserId;

                    for (int i = 0; i < needUserCount; i++)
                    {
                        Socket_S.getInstance().Send(waitUserList[i].clientInfo, s2c);
                    }

                    // 从等待队列移除
                    for (int i = 0; i < needUserCount; i++)
                    {
                        waitUserList.RemoveAt(0);
                    }
                }
            }
        }

        static bool checkIsExist(int _userId)
        {
            for(int i = 0; i < waitUserList.Count; i++)
            {
                if(waitUserList[i].userId == _userId)
                {
                    return true;
                }
            }

            return false;
        }

        public static void removeUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < waitUserList.Count; i++)
            {
                if (waitUserList[i].clientInfo == clientInfo)
                {
                    waitUserList.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
