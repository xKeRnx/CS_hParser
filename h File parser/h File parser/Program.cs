using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace h_File_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string zone = @"zone.h";
            string wm = @"wm.h";
            string client = @"client.h";

            if (!File.Exists(zone))
            {
                Console.WriteLine(zone + " file not exists");
            }

            if (!File.Exists(wm))
            {
                Console.WriteLine(wm + " file not exists");
            }

            if (!File.Exists(client))
            {
                Console.WriteLine(client + " file not exists");
            }

            if (File.Exists(zone) && File.Exists(wm) && File.Exists(client))
            {

                string readZone = File.ReadAllText(zone);
                string readWM = File.ReadAllText(wm);
                string readClient = File.ReadAllText(client);

                if (File.Exists("Result.txt"))
                {
                    File.Delete("Result.txt");
                }

                if (Directory.Exists("Zone"))
                {
                    Directory.Delete("Zone", true);
                }

                if (Directory.Exists("WM"))
                {
                    Directory.Delete("WM", true);
                }

                if (Directory.Exists("Client"))
                {
                    Directory.Delete("Client", true);
                }

                if (Directory.Exists("NEW"))
                {
                    Directory.Delete("NEW", true);
                }

                if (!Directory.Exists("Zone"))
                {
                    Directory.CreateDirectory("Zone");
                }

                if (!Directory.Exists("WM"))
                {
                    Directory.CreateDirectory("WM");
                }

                if (!Directory.Exists("Client"))
                {
                    Directory.CreateDirectory("Client");
                }

                if (!Directory.Exists("NEW"))
                {
                    Directory.CreateDirectory("NEW");
                }

                //Parse Zone
                using (StringReader reader = new StringReader(readZone))
                {
                    string line;
                    bool startWrite = false;
                    string Protoname = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("struct PROTO_"))
                        {
                            if (!line.Contains(";"))
                            {
                                Protoname = line;
                                startWrite = true;
                            }

                        }

                        if (startWrite == true)
                        {
                            Protoname = Protoname.Replace(":", "");
                            using (StreamWriter sw = File.AppendText("Zone\\" + Protoname + ".txt"))
                            {
                                sw.WriteLine(line);
                            }
                        }

                        if (line.Contains("};"))
                        {
                            startWrite = false;
                        }
                    }
                    Console.WriteLine("Zone Proto export finish!");
                }

                //Parse WM
                using (StringReader reader = new StringReader(readWM))
                {
                    string line;
                    bool startWrite = false;
                    string Protoname = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("struct PROTO_"))
                        {
                            if (!line.Contains(";"))
                            {
                                Protoname = line;
                                startWrite = true;
                            }

                        }

                        if (startWrite == true)
                        {
                            Protoname = Protoname.Replace(":", "");
                            using (StreamWriter sw = File.AppendText("WM\\" + Protoname + ".txt"))
                            {
                                sw.WriteLine(line);
                            }
                        }

                        if (line.Contains("};"))
                        {
                            startWrite = false;
                        }
                    }
                    Console.WriteLine("WM Proto export finish!");
                }

                //Parse Client
                using (StringReader reader = new StringReader(readClient))
                {
                    string line;
                    bool startWrite = false;
                    string Protoname = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("struct PROTO_"))
                        {
                            if (!line.Contains(";"))
                            {
                                Protoname = line;
                                startWrite = true;
                            }

                        }

                        if (startWrite == true)
                        {
                            Protoname = Protoname.Replace(":", "");
                            using (StreamWriter sw = File.AppendText("Client\\" + Protoname + ".txt"))
                            {
                                sw.WriteLine(line);
                            }
                        }

                        if (line.Contains("};"))
                        {
                            startWrite = false;
                        }
                    }
                    Console.WriteLine("Client Proto export finish!");
                }

                //Search Protos From Client in WM and Zone
                using (StringReader reader = new StringReader(readClient))
                {
                    string line;
                    string Protoname;
                    StringBuilder sb0 = new StringBuilder();
                    StringBuilder sb1 = new StringBuilder();
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains("struct PROTO_"))
                        {
                            if (!line.Contains(";"))
                            {
                                Protoname = line;
                                Protoname = Protoname.Replace(":", "");

                                if (File.Exists("Client\\" + Protoname + ".txt"))
                                {
                                    bool FileFound = false;
                                    String[] File1Lines = File.ReadAllLines("Client\\" + Protoname + ".txt");
                                    String[] File2Lines = { };
                                    if (File.Exists("WM\\" + Protoname + ".txt"))
                                    {
                                        FileFound = true;
                                        File2Lines = File.ReadAllLines("WM\\" + Protoname + ".txt");
                                    }
                                    else if (File.Exists("Zone\\" + Protoname + ".txt"))
                                    {
                                        FileFound = true;
                                        File2Lines = File.ReadAllLines("Zone\\" + Protoname + ".txt");
                                    }
                                    else
                                    {
                                        FileFound = false;
                                        sb0.Append("zone / WM " + Protoname + ".txt not found!");
                                        sb0.AppendLine();
                                    }

                                    if (FileFound == true)
                                    {
                                        List<string> NewLines1 = new List<string>();
                                        if (File1Lines.Length == File2Lines.Length)
                                        {
                                            for (int i = 0; i < File1Lines.Length; i++)
                                            {
                                                if (File1Lines[i] != File2Lines[i])
                                                {
                                                    sb1.Append(line + " is not the same! Client: " + File1Lines[i] + " Zone/WM: " + File2Lines[i]);
                                                    sb1.AppendLine();
                                                }
                                                else
                                                {
                                                    //Console.WriteLine(Protoname + " is the same!");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sb1.Append(line + " has not the same length!");
                                            sb1.AppendLine();
                                        }
                                    }
                                }
                                else
                                {
                                    sb0.Append("Client\\" + Protoname + ".txt not found!");
                                    sb0.AppendLine();
                                }
                            }
                        }
                    }
                    using (StreamWriter sw = File.AppendText("Result.txt"))
                    {
                        sw.WriteLine(sb0.ToString());
                        sw.WriteLine(sb1.ToString());
                    }
                    Console.WriteLine("Proto Compare finish");
                }

            }
            Console.ReadLine();
        }
    }
}
