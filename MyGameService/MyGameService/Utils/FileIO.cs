using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileIO
{
    public static void Write(string path,string data)
    {
        StreamWriter sw = null;
        try
        {
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            sw = new StreamWriter(path, true);

            //开始写入
            sw.WriteLine(data);

            //清空缓冲区
            sw.Flush();

            //关闭流
            sw.Close();
        }
        catch (IOException e)
        {
            Console.Write(e);
        }
        finally
        {
            sw.Close();
        }
    }

    public static string Read(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return "";
            }

            StreamReader sr = new StreamReader(path);
            string str = sr.ReadToEnd().ToString();
            sr.Close();

            return str;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public static void Rename(string oldPath,string newPath)
    {
        try
        { 
            if (!File.Exists(oldPath))
            {
                return;
            }

            FileInfo fileInfo = new FileInfo(oldPath);
            fileInfo.MoveTo(newPath);
        }
        catch (Exception ex)
        {
        }
    }
}
