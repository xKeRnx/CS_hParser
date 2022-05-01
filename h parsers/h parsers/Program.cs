using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace h_parsers
{
    class Program
    {
        static void Main(string[] args)
        {
            string rip = "rip_wm.h";
            string std = "std_wm.h";
            if (!File.Exists(rip))
            {
                Console.WriteLine(rip);
            }
            if (!File.Exists(std))
            {
                Console.WriteLine(std);
            }

            if (File.Exists(rip) && File.Exists(std))
            {
                bool startnormline = false;
                bool startstruct = false;
                bool finddeclinstd = false;
                int adddeclnum = 0;
                bool addprackop = false;
                string stdlineret = string.Empty;
                string insearchstruct = string.Empty;
                string thisstructname = string.Empty;
                string thisnormlinestruc = string.Empty;

                StringBuilder builder = new StringBuilder();
                var lines = File.ReadLines(rip);
                foreach (var line in lines)
                {
                    string thisline = line;
                    if (thisline.Contains(" __unaligned"))
                    {
                        thisline = thisline.Replace(" __unaligned", string.Empty);
                    }

                    if (thisline.Contains(" __cppobj"))
                    {
                        thisline = thisline.Replace(" __cppobj", string.Empty);
                    }

                    if (thisline.Contains("unnamed-type-"))
                    {
                        thisline = thisline.Replace("unnamed-type-", "unnamed_type_");
                    }

                    if (thisline.Contains("<unnamed-tag>"))
                    {
                        thisline = thisline.Replace("<unnamed-tag>", "unnamed-tag");
                    }

                    if (thisline.Contains("<unnamed-type-"))
                    {
                        thisline = thisline.Replace("<unnamed-type-", "unnamed_type_");
                        thisline = thisline.Replace(">", ">");
                    }

                    if (thisline.Contains("struct") && thisline.Contains("{") || thisline.Contains("class") && thisline.Contains("{"))
                    {
                        startstruct = true;
                        string[] splitted = thisline.Split(' ');
                        thisstructname = splitted[1];
                        var stdlines = File.ReadLines(std);

                        foreach (var stdline in stdlines)
                        {
                            if (stdline.Contains("struct") && stdline.Contains(thisstructname) && !stdline.Contains(";") || stdline.Contains("class") && stdline.Contains(thisstructname) && !stdline.Contains(";"))
                            {
                                insearchstruct = stdline;
                            }

                            if (stdline.Contains("struct") && stdline.Contains(thisstructname) && !stdline.Contains(";") || stdline.Contains("class") && stdline.Contains(thisstructname) && !stdline.Contains(";"))
                            {
                                stdlineret = stdline;
                                if (stdline.Contains("__declspec(align(1))"))
                                {
                                    adddeclnum = 1;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(2))"))
                                {
                                    adddeclnum = 2;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(3))"))
                                {
                                    adddeclnum = 3;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(4))"))
                                {
                                    adddeclnum = 4;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(5))"))
                                {
                                    adddeclnum = 5;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(6))"))
                                {
                                    adddeclnum = 6;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(7))"))
                                {
                                    adddeclnum = 7;
                                    finddeclinstd = true;
                                }
                                if (stdline.Contains("__declspec(align(8))"))
                                {
                                    adddeclnum = 8;
                                    finddeclinstd = true;
                                }
                                break;
                            }
                        }
                    }

                    if (insearchstruct != string.Empty && !thisline.Contains("struct") && thisline.Contains(";") && !thisline.Contains("};") || insearchstruct != string.Empty && !thisline.Contains("struct") && thisline.Contains(";") && !thisline.Contains("};"))
                    {
                        var stdnormlines = File.ReadLines(std);

                        foreach (var stdnormline in stdnormlines)
                        {
                            if (stdnormline.Contains("struct ") && !stdnormline.Contains(";") || stdnormline.Contains("class ") && !stdnormline.Contains(";") )
                            {
                                string[] normssplitted = stdnormline.Split(' ');
                                thisnormlinestruc = normssplitted[1];
                                Console.WriteLine(thisnormlinestruc + ":");
                            }

                            if (!stdnormline.Contains("struct") && stdnormline.Contains(thisstructname) && stdnormline.Contains(";") || !stdnormline.Contains("class") && stdnormline.Contains(thisstructname) && stdnormline.Contains(";"))
                            {
                                if (thisnormlinestruc == thisstructname)
                                {
                                    //Console.WriteLine(stdnormline);
                                }
                            }
                        }
                    }




                    if (stdlineret != string.Empty && finddeclinstd == true)
                    {
                        builder.AppendLine("#pragma pack(push, " + adddeclnum + ")");
                        addprackop = true;
                    }

                    builder.AppendLine(thisline);

                    if (startstruct == true && thisline.Contains("};") && addprackop == true)
                    {
                        builder.AppendLine("#pragma pack(pop)");
                        startstruct = false;
                        addprackop = false;
                    }

                    stdlineret = string.Empty;
                    finddeclinstd = false;
                    adddeclnum = 0;
                }
                Console.WriteLine(builder.ToString());
            }
            Console.ReadLine();
        }
    }
}
