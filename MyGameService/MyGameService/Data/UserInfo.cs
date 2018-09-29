using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UserInfo
{
    public static List<UserInfoData> UserInfoList = new List<UserInfoData>();

    public static bool isExitsUser(string id)
    {
        for (int i = 0; i < UserInfoList.Count; i++)
        {
            if (UserInfoList[i].Id.CompareTo(id) == 0)
            {
                return true;
            }
        }

        return false;
    }

    public static UserInfoData getUserInfoData(string id)
    {
        for (int i = 0; i < UserInfoList.Count; i++)
        {
            if (UserInfoList[i].Id.CompareTo(id) == 0)
            {
                return UserInfoList[i];
            }
        }

        return null;
    }

    public static void addUser(string id)
    {
        UserInfoData data = new UserInfoData();
        data.Id = id;
        data.Gold = 1000;

        data.BagList.Add(101);
        data.BagList.Add(102);
        data.BagList.Add(103);
        data.BagList.Add(104);
        data.BagList.Add(105);
        data.BagList.Add(106);

        UserInfoList.Add(data);
    }
}