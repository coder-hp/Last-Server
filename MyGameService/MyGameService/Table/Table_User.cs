using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Table_User
{
    public int id;
    public string account;
    public string password;
    public string phone;
    public int zhanshilevel;
    public int fashilevel;
    public int huanshilevel;
    public string createtime;

    public static Table_User init(List<Object> data)
    {
        Table_User table = new Table_User();

        table.id = (int)data[0];
        table.account = data[1].ToString();
        table.password = data[2].ToString();
        table.phone = data[3].ToString();
        table.zhanshilevel = (int)data[4];
        table.fashilevel = (int)data[5];
        table.huanshilevel = (int)data[6];
        table.createtime = data[7].ToString();

        return table;
    }
}
