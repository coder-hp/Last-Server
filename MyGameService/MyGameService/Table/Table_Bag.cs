using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Table_Bag
{
    // 这两个不需要返回
    //public int pk;
    //public int id;

    public int itemid;
    public int uniqueid;
    public int count;

    public static List<Table_Bag> init(List<Object> data)
    {
        List<Table_Bag> list = new List<Table_Bag>();

        int keyCount = 5;
        if((data.Count % keyCount) == 0)
        {
            for(int i = 0; i < data.Count; i += 5)
            {
                Table_Bag table = new Table_Bag();

                //table.pk = (int)data[i];
                //table.id = (int)data[i + 1];
                table.itemid = (int)data[i + 2];
                table.uniqueid = (int)data[i + 3];
                table.count = (int)data[i + 4];

                list.Add(table);
            }
        }

        return list;
    }
}
