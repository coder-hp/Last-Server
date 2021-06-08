using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Game
{
    class RoomManager
    {
        static List<RoomLogic> list_room = new List<RoomLogic>();

        public static void addRoom(RoomLogic roomLogic)
        {
            list_room.Add(roomLogic);
        }

        public static RoomLogic getRoomByUserId(int userId)
        {
            for (int i = 0; i < list_room.Count; i++)
            {
                if (list_room[i].checkIsExistUser(userId))
                {
                    return list_room[i];
                }
            }

            return null;
        }

        public static void deleteUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < list_room.Count; i++)
            {
                if (list_room[i].checkIsExistUser(clientInfo))
                {
                    list_room[i].deleteUser(clientInfo);
                    CommonUtil.Log("房间有玩家退出");
                    if (list_room[i].list_user.Count == 0)
                    {
                        list_room.RemoveAt(i);
                        CommonUtil.Log("房间玩家数为0，删除该房间");
                    }
                    break;
                }
            }
        }

        public static void heroEnterNextMap(int userId)
        {
            for (int i = 0; i < list_room.Count; i++)
            {
                if (list_room[i].checkIsExistUser(userId))
                {
                    list_room[i].heroEnterNextMap(userId);
                    break;
                }
            }
        }
    }
}
