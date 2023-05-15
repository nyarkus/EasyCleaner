using IWshRuntimeLibrary;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

class Program
{
    float size = 0;
    Int64 count = 0;
    string userName = Environment.UserName;
    bool keeplogs = true;
    static void Main(string[] args)
    {
        bool openlog = false;

        Program p = new Program();
        if (!System.IO.File.Exists("Custom Paths.txt"))
        {
            System.IO.File.WriteAllText("Custom Paths.txt", "//Example: \"C:\\tmp\\\"");
        }
        if (!System.IO.File.Exists("Settings.txt"))
        {
            System.IO.File.WriteAllText("Settings.txt", "StartWithWindows=true\r\nOpetMiniLogAfterCleanUp=false\r\nKeepLogs=true");
        }
        string[] settings = System.IO.File.ReadAllLines("Settings.txt");
        if (settings[1].Replace("OpetMiniLogAfterCleanUp=", "") == "true") openlog = true;
        if (settings[0].Replace("StartWithWindows=","") == "true")
        {
            if(!System.IO.File.Exists(@$"C:\Users\{p.userName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\EasyCleaner.url"))
            {
                p.CreateShortcut();
            }
        }
        else
        {
            if (System.IO.File.Exists(@$"C:\Users\{p.userName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\EasyCleaner.url")) System.IO.File.Delete(@$"C:\Users\{p.userName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\EasyCleaner.url");
        }
        if (settings[2].Replace("KeepLogs=","") == "false") p.keeplogs= false;
        p.Log("(INFO)Загрузка кастомных путей...");
        string[] paths = System.IO.File.ReadAllLines("Custom Paths.txt");
        foreach(string s in paths)
        {
            if (s != "//Example: \"C:\\tmp\\\"")
                p.CleanUpD(s);
        }
        p.CleanUpD(@$"C:\Users\{p.userName}\AppData\Local\Temp");
        p.CleanUpD(@"C:\Windows\SoftwareDistribution");
        p.CleanUpD(@"C:\Windows\Logs");
        p.CleanUpD(@"C:\Windows\Prefetch");
        p.CleanUpD(@"C:\Windows\Offline Web Pages");
        Console.WriteLine(@"C:\hiberfil.sys");
        float ssize = 0;
        try
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(@"C:\hiberfil.sys");
            ssize = fileInfo.Length;
            System.IO.File.Delete(@"C:\hiberfil.sys");
        }
        catch (Exception e)
        {
            if (!e.Message.StartsWith("Access to the path") && !e.Message.StartsWith("The process cannot access the file"))
                p.Log($"(ERROR) {e.Message} при удалении {@"C:\hiberfil.sys"}");
            p.size -= ssize;
            p.count--;
        }
        finally
        {
            p.size += ssize;
            p.count++;
        }
        p.Log($"(INFO)Все пути успешно очищены");
        Console.ForegroundColor = ConsoleColor.Yellow;
        p.Log($"(INFO)Удалено {p.count} Файлов весом {p.size / 1024 / 1024} MB");
        Console.ResetColor();
        System.IO.File.AppendAllText("Log.txt", $"[{DateTime.Now}]   Удалено {p.count} файлов весом {p.size / 1024 / 1024} MB\r\n");
        if (openlog)
        {
            System.IO.File.WriteAllText("MiniLog.txt", $"Удалено {p.count} Файлов весом {p.size / 1024 / 1024} MB");
            Process process= new Process();
            process.StartInfo.FileName = "notepad.exe";
            process.StartInfo.Arguments = $@"{Directory.GetCurrentDirectory()}\MiniLog.txt";
            process.Start();
            p.Log("(WARNING) Программа закроется после закрытия текстового документа");
            while (!process.HasExited);
            System.IO.File.Delete($@"{Directory.GetCurrentDirectory()}\MiniLog.txt");
            
        }

    }

    void CleanUpD(string path)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Log($"(LOG)Очистка {path}...");
        Console.ResetColor();
        string[] todelete = {""};
        try
        {
            todelete = Directory.GetFiles(path);
        }
        catch(Exception ex)
        {
            if(ex is System.UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("(WARNING)Запустите программу с правами администратора для очистки этого путя!");
                Console.ResetColor();
                return;
            }
        }

        float localsize = 0;
        Int64 localcount = 0;
        foreach (string s in todelete)
        {
            float ssize = 0;
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(s);
                ssize= fileInfo.Length;
                System.IO.File.Delete(s);
            }
            catch (Exception e)
            {
                if (!e.Message.StartsWith("Access to the path") && !e.Message.StartsWith("The process cannot access the file"))
                    Log($"(ERROR) {e.Message} при удалении {s}");
                size-= ssize;
                localsize-= ssize;
                count--;
                localcount--;
            }
            finally
            {
                size += ssize;
                count++;
                localcount++;
                localsize += ssize;
            }
        }
        for (int i = 0; i < 10; i++)
        {
            string[] paths = Directory.GetDirectories(path);
            foreach (string s in paths)
            {
                string[] path2 = Directory.GetFiles(s);
                foreach (string v in path2)
                {
                    float ssize = 0;
                    try
                    {
                        System.IO.FileInfo fileInfo = new System.IO.FileInfo(v);
                        ssize = fileInfo.Length;
                        System.IO.File.Delete(v);
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Access to the path") && !e.Message.StartsWith("The process cannot access the file"))
                            Log($"(ERROR) {e.Message} при удалении {v}");
                        size -= ssize;
                        localsize -= ssize;
                        count--;
                        localcount--;
                    }
                    finally
                    {
                        size += ssize;
                        count++;
                        localcount++;
                        localsize += ssize;
                    }
                }
            }
            foreach (string s in paths)
            {
                try
                {
                    Directory.Delete(s, true);
                }
                catch (Exception e)
                {
                    if (!e.Message.StartsWith("Access to the path") && !e.Message.StartsWith("The process cannot access the file"))
                        Log($"(ERROR) {e.Message} при удалении {s}");
                }

            }
        }
        Log($"(INFO){path} очищен");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log($"(INFO)Удалено {localcount} Файлов размером {localsize / 1024 / 1024} MB");
        Console.ResetColor();
    }
    private void CreateShortcut()
    {
        object shDesktop = (object)"Desktop";
        WshShell shell = new WshShell();
        string shortcutAddress = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\EasyCleaner.lnk";
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
        shortcut.Description = "Запуск EasyCleaner";
        shortcut.TargetPath = Directory.GetCurrentDirectory() + @"\EasyCleaner.exe";
        shortcut.WorkingDirectory = Directory.GetCurrentDirectory();
        shortcut.Save();
    }
    private void Log(string m)
    {
        if(keeplogs) System.IO.File.AppendAllText("Log.txt", $"[{DateTime.Now}] {m}\r\n");
        if (m.StartsWith("(LOG)"))
        {
            Console.ForegroundColor= ConsoleColor.White;
            Console.WriteLine(m.Replace("(LOG)", ""));
        }
        if (m.StartsWith("(INFO)"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(m.Replace("(INFO)", ""));
        }
        if (m.StartsWith("(WARNING)"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(m.Replace("(WARNING)", ""));
        }
        if (m.StartsWith("(ERROR)"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(m.Replace("(ERROR)", ""));
        }
    }
}

