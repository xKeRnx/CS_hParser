using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace test_parser
{
    class Program
    {
        static void NewThread()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Convert.ToInt32(stopwatch.Elapsed.TotalSeconds));
                Console.Write("Running " + timeSpan.ToString("c"));
                Console.Write('\r');
            }
        }

        static void Main(string[] args)
        {
            string stdfile = "std_wm.h";

            if (File.Exists(stdfile))
            {
                Console.WriteLine("Parsing started... Please wait... This can take a while...");
                Thread t = new Thread(NewThread);
                t.Start();
                string[] delim = { Environment.NewLine, "\n" };
                StringBuilder builder = new StringBuilder();
                StringBuilder builder1 = new StringBuilder();
                StringBuilder builder01 = new StringBuilder();
                StringBuilder builder2 = new StringBuilder();
                StringBuilder builder3 = new StringBuilder();
                StringBuilder builder4 = new StringBuilder();
                StringBuilder typedef_builder = new StringBuilder();
                StringBuilder addstruct_builder = new StringBuilder();
                var lines = File.ReadLines(stdfile);
                int i = 0;
                string lastline = string.Empty;

                // Checks START
                bool allow_write = true;
                // CHECKS END

                //Directorys START
                if (Directory.Exists("NEEDED"))
                {
                    Directory.Delete("NEEDED", true);
                }
                if (!Directory.Exists("NEEDED"))
                {
                    Directory.CreateDirectory("NEEDED");
                }
                //Directorys END

                foreach (var line in lines)
                {
                    i++;
                    string thisline = line;

                    if (!line.Contains("#define") && !line.Contains("typedef") && !thisline.Contains("/* "))
                    {
                        thisline = thisline.Replace(" __unaligned", string.Empty);
                        thisline = thisline.Replace(" __cppobj", string.Empty);
                        thisline = thisline.Replace("/*VFT*/ ", string.Empty);
                        thisline = thisline.Replace("_BYTE ", "BYTE ");
                        thisline = thisline.Replace("_TBYTE ", "TBYTE ");
                        thisline = thisline.Replace("*this", "*_this");

                        if (line.Contains("std::") && !line.Contains(";"))
                        {
                            allow_write = false;
                        }

                        if (line.Contains("<") && line.Contains(">"))
                        {
                            thisline = thisline.Replace("<", "");
                            thisline = thisline.Replace(">", "");
                        }

                        if (allow_write == true && line.Contains(";") && line.Contains("std::"))
                        {
                            thisline = "//" + thisline;
                        }

                        if (allow_write == true && !thisline.Contains("/* "))
                        {
                            if (thisline != string.Empty && lastline != string.Empty || thisline == string.Empty && lastline != string.Empty || thisline != string.Empty && lastline == string.Empty)
                            {
                                builder.AppendLine(thisline);
                            }
                        }

                        if (allow_write == false && line.Contains("};"))
                        {
                            allow_write = true;
                        }
                        lastline = thisline;
                    }
                }

                string[] builder_out = builder.ToString().Split(delim, StringSplitOptions.None);
                bool write_struct_to_file = false;
                bool write_struct_addinlinedecl = false;
                bool write_struct_addinlinedecl1 = false;
                string struct_to_file_name = string.Empty;
                foreach (string builder_line in builder_out)
                {
                    string editable_builder_line = builder_line;

                    if (builder_line.Contains("struct") && !builder_line.Contains(";"))
                    {
                        foreach (string builder_line1 in builder_out)
                        {
                            string new_builder_line1 = builder_line;
                            new_builder_line1 = Regex.Split(new_builder_line1, " ").Last();
                            if (new_builder_line1.Contains("::"))
                            {
                                new_builder_line1 = Regex.Split(new_builder_line1, "::").Last();
                            }
                            if (new_builder_line1.Contains(" : "))
                            {
                                new_builder_line1 = Regex.Split(new_builder_line1, " : ").First();
                            }

                            if (builder_line1.Contains("struct") && builder_line1.Contains(";") && builder_line1.Contains(new_builder_line1))
                            {
                                break;
                            }
                            else {
                                if (new_builder_line1 != string.Empty && !addstruct_builder.ToString().Contains(new_builder_line1) && !new_builder_line1.Contains(",") && !new_builder_line1.Contains(":"))
                                {
                                    if (!new_builder_line1.Contains("struct "))
                                    {
                                        new_builder_line1 = "struct " + new_builder_line1;
                                    }
                                    if (!new_builder_line1.Contains(";"))
                                    {
                                        new_builder_line1 = new_builder_line1 + ";";
                                    }
                                    addstruct_builder.AppendLine(new_builder_line1);
                                }
                                break;
                            }
                        }       
                    }

                    if (builder_line.Contains("enum ") && builder_line.Contains("::") && !builder_line.Contains(";") || builder_line.Contains("struct ") && builder_line.Contains("::") && !builder_line.Contains(";") || builder_line.Contains("union ") && builder_line.Contains("::") && !builder_line.Contains(";"))
                    {
                        string test1_fist = Regex.Split(editable_builder_line, "::").First();
                        test1_fist = Regex.Split(test1_fist, " ").Last();

                        string test1_last = Regex.Split(editable_builder_line, "::").Last();

                        if (!test1_last.Contains("*"))
                        {
                            foreach (string builder_line1 in builder_out)
                            {
                                if (builder_line1.Contains("enum ") && !builder_line1.Contains("::") && !builder_line1.Contains(";") || builder_line1.Contains("struct ") && !builder_line1.Contains("::") && !builder_line1.Contains(";") || builder_line1.Contains("union ") && !builder_line1.Contains("::") && !builder_line1.Contains(";"))
                                {
                                    if (builder_line1.Contains(test1_fist) && !builder_line1.Contains(test1_fist.Trim() + "_"))
                                    {
                                        write_struct_to_file = true;
                                        struct_to_file_name = test1_fist;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (write_struct_to_file == false)
                    {
                        builder1.AppendLine(editable_builder_line);
                    }

                    if (write_struct_to_file == true && struct_to_file_name != string.Empty)
                    {
                        try
                        {
                            using (StreamWriter sw = File.AppendText(@"NEEDED\" + struct_to_file_name + ".txt"))
                            {
                                string wirte_builder_line = builder_line;
                                wirte_builder_line = wirte_builder_line.Replace(struct_to_file_name + "::", "");

                                int inlinedecl = Checkdecl(wirte_builder_line);
                                if (inlinedecl > 0)
                                {
                                    write_struct_addinlinedecl = true;
                                    if (wirte_builder_line.Contains(";"))
                                    {
                                        write_struct_addinlinedecl1 = true;
                                        write_struct_addinlinedecl = false;
                                    }
                                    wirte_builder_line = wirte_builder_line.Replace("__declspec(align(" + inlinedecl + ")) ", "");
                                    sw.WriteLine("#pragma pack(push, " + inlinedecl + ")");
                                }

                                sw.WriteLine("  " + wirte_builder_line);

                                if (write_struct_addinlinedecl == false && wirte_builder_line.Contains("};"))
                                {
                                    sw.WriteLine();
                                }

                                if (write_struct_addinlinedecl == true && wirte_builder_line.Contains("};") || write_struct_addinlinedecl1 == true)
                                {
                                    write_struct_addinlinedecl = false;
                                    write_struct_addinlinedecl1 = false;
                                    sw.WriteLine("#pragma pack(pop)");
                                    sw.WriteLine();
                                }
                            }
                            if (builder_line.Contains("};"))
                            {
                                write_struct_to_file = false;
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                bool builder01_addinlinedecl = false;
                bool builder01_addinlinedecl1 = false;
                string[] builder_out1 = builder1.ToString().Split(delim, StringSplitOptions.None);
                foreach (string builder1_line in builder_out1)
                {
                    string editable_builder_line = builder1_line;

                    int inlinedecl = Checkdecl(editable_builder_line);
                    if (inlinedecl > 0)
                    {
                        builder01_addinlinedecl = true;
                        if (editable_builder_line.Contains(";"))
                        {
                            builder01_addinlinedecl1 = true;
                        }
                        editable_builder_line = editable_builder_line.Replace("__declspec(align(" + inlinedecl + ")) ", "");
                        builder01.AppendLine("#pragma pack(push, " + inlinedecl + ")");
                    }

                    builder01.AppendLine(editable_builder_line);

                    if (editable_builder_line.Contains(";") && builder01_addinlinedecl1 == true)
                    {
                        builder01_addinlinedecl1 = false;
                        builder01.AppendLine("#pragma pack(pop)");
                    }
                    if (builder01_addinlinedecl == true && editable_builder_line.Contains("};"))
                    {
                        builder01_addinlinedecl = false;
                        builder01.AppendLine("#pragma pack(pop)");
                    }
                }

                string[] builder_out01 = builder01.ToString().Split(delim, StringSplitOptions.None);
                string builder1_structname = string.Empty;
                foreach (string builder1_line in builder_out01)
                {
                    string editable_builder_line = builder1_line;
                    if (editable_builder_line.Contains("enum ") && !editable_builder_line.Contains("::") && !editable_builder_line.Contains(";") || editable_builder_line.Contains("struct ") && !editable_builder_line.Contains("::") && !editable_builder_line.Contains(";") || editable_builder_line.Contains("union ") && !editable_builder_line.Contains("::") && !editable_builder_line.Contains(";"))
                    {
                        string test1_fist = editable_builder_line;
                        if (editable_builder_line.Contains(" : "))
                        {
                            test1_fist = Regex.Split(test1_fist, " : ").First();
                        }
                        test1_fist = Regex.Split(test1_fist, " ").Last();
                        builder1_structname = test1_fist;
                    }

                    builder2.AppendLine(editable_builder_line);

                    if (builder1_structname != string.Empty && editable_builder_line.Contains("{") && File.Exists(@"NEEDED\" + builder1_structname + ".txt"))
                    {
                        string contents = File.ReadAllText(@"NEEDED\" + builder1_structname + ".txt");
                        builder2.AppendLine(contents);
                        builder1_structname = string.Empty;
                    }
                }

                string b3_lastline = string.Empty;
                string[] builder_out2 = builder2.ToString().Split(delim, StringSplitOptions.None);
                foreach (string builder2_line in builder_out2)
                {
                    string editable_builder_line = builder2_line;

                    if (editable_builder_line.Contains("::") && !editable_builder_line.Contains("__thiscall"))
                    {
                        string[] splitted = Regex.Split(editable_builder_line, "::");
                        int splitcount = splitted.Length;
                        if (splitcount == 2)
                        {
                            string splitfirst = Regex.Split(splitted[0], " ").First();
                            string splitlast = Regex.Split(splitted[0], " ").Last();
                            editable_builder_line = editable_builder_line.Replace(splitlast + "::", "");
                        }
                        if (splitcount == 3)
                        {
                            string splitfirst = Regex.Split(splitted[0], " ").First();
                            string splitlast1 = Regex.Split(splitted[0], " ").Last();
                            string splitlast2 = Regex.Split(splitted[1], " ").Last();
                            editable_builder_line = editable_builder_line.Replace(splitlast1 + "::" + splitlast2 + "::", "");
                        }
                        if (splitcount == 4)
                        {
                            string splitfirst = Regex.Split(splitted[0], " ").First();
                            string splitlast1 = Regex.Split(splitted[0], " ").Last();
                            string splitlast2 = Regex.Split(splitted[1], " ").Last();
                            string splitlast3 = Regex.Split(splitted[2], " ").Last();
                            editable_builder_line = editable_builder_line.Replace(splitlast1 + "::" + splitlast2 + "::" + splitlast3 + "::", "");
                        }
                    }
                    if (editable_builder_line.Contains("__thiscall") && !editable_builder_line.Contains("struct"))
                    {
                        string tcsplit_first = Regex.Split(editable_builder_line, "__thiscall ").First();
                        string tcsplit_last = Regex.Split(editable_builder_line, "__thiscall ")[1];
                        tcsplit_last = tcsplit_last.Split(')')[0];
                        string new_tcsplit_last = tcsplit_last.Trim().Replace("*", "");

                        editable_builder_line = editable_builder_line.Replace(new_tcsplit_last, "Hook_" + new_tcsplit_last);

                        if (editable_builder_line.Contains("std::"))
                        {
                            editable_builder_line = "//typedef" + editable_builder_line;
                        }
                        else
                        {
                            editable_builder_line = "typedef" + editable_builder_line;
                        }
                        

                        typedef_builder.AppendLine(editable_builder_line);
                        editable_builder_line = "  Hook_" + new_tcsplit_last + " Org_" + new_tcsplit_last + ";";

                    }

                    if (editable_builder_line.Contains("~"))
                    {
                        editable_builder_line = "//" + editable_builder_line;
                    }

                    if (b3_lastline == string.Empty && editable_builder_line == string.Empty)
                    {
                    }
                    else
                    {
                        b3_lastline = editable_builder_line;
                        builder3.AppendLine(editable_builder_line);
                    }
                }

                string[] builder_out3 = builder3.ToString().Split(delim, StringSplitOptions.None);
                bool typedef_added = false;
                foreach (string builder3_line in builder_out3)
                {
                    string editable_builder_line = builder3_line;
                    if (editable_builder_line.Contains("struct") && editable_builder_line.Contains(";") && editable_builder_line.Contains(","))
                    {
                        if (Regex.Split(editable_builder_line, ",").Length == 2)
                        {
                            string splitfirst = Regex.Split(editable_builder_line, ",").First();
                            string splitlast = Regex.Split(editable_builder_line, ",").Last();
                            splitlast = splitlast.Replace(" *", "").Trim();
                            if (splitlast != string.Empty && splitlast.Length > 4 && !splitlast.Contains("::") && !splitlast.Contains(")"))
                            {
                                editable_builder_line = splitfirst + ";" + Environment.NewLine + "struct " + splitlast;
                            }
                        }
                    }

                    if (editable_builder_line.Contains("struct") && editable_builder_line.Contains(";") && editable_builder_line.Contains(" "))
                    {
                        if (Regex.Split(editable_builder_line, " ").Length == 3)
                        {
                            string split0 = Regex.Split(editable_builder_line, " ")[0];
                            string split1 = Regex.Split(editable_builder_line, " ")[1];
                            editable_builder_line = split0 + " " + split1 + ";";
                        }
                    }

                    if (editable_builder_line.Contains("struct") && !editable_builder_line.Contains(";") && typedef_added == false || editable_builder_line.Contains("enum") && !editable_builder_line.Contains(";") && typedef_added == false)
                    {
                        typedef_added = true;
                        builder4.AppendLine("// NEW STRUCTS START");
                        builder4.AppendLine(addstruct_builder.ToString());
                        builder4.AppendLine("// NEW STRUCTS END");

                        builder4.AppendLine("// TYPEDEF START");
                        builder4.AppendLine(typedef_builder.ToString());
                        builder4.AppendLine("// TYPEDEF END");
                    }
                    if (!editable_builder_line.Contains("struct;") && !editable_builder_line.Contains("struct char;") && !editable_builder_line.Contains("struct*)") && !editable_builder_line.Contains("struct short)"))
                    {
                        builder4.AppendLine(editable_builder_line);
                    }
                }

                if (File.Exists("new.h"))
                {
                    File.Delete("new.h");
                }
                using (StreamWriter sw = File.CreateText("new.h"))
                {
                    sw.WriteLine(builder4.ToString());
                }
                t.Abort();
                Console.WriteLine();
               Console.WriteLine("Parsing finish.. please check the file :)");
            }
            else
            {
                Console.WriteLine(".h File not found!");
            }

            Console.ReadLine();
        }

        static int Checkdecl(string stdline)
        {
            int adddeclnum = 0;

            if (stdline.Contains("__declspec(align(1))"))
            {
                adddeclnum = 1;
            }
            if (stdline.Contains("__declspec(align(2))"))
            {
                adddeclnum = 2;
            }
            if (stdline.Contains("__declspec(align(3))"))
            {
                adddeclnum = 3;
            }
            if (stdline.Contains("__declspec(align(4))"))
            {
                adddeclnum = 4;
            }
            if (stdline.Contains("__declspec(align(5))"))
            {
                adddeclnum = 5;
            }
            if (stdline.Contains("__declspec(align(6))"))
            {
                adddeclnum = 6;
            }
            if (stdline.Contains("__declspec(align(7))"))
            {
                adddeclnum = 7;
            }
            if (stdline.Contains("__declspec(align(8))"))
            {
                adddeclnum = 8;
            }
            if (stdline.Contains("__declspec(align(16))"))
            {
                adddeclnum = 16;
            }

            return adddeclnum;
        }
    }
}
