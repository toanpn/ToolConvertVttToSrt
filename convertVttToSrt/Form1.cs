using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace convertVttToSrt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
        List<string> lstFileVTT;

        private void button1_Click(object sender, EventArgs e)
        {
            lstFileVTT = new List<string>();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var files = Directory.EnumerateFiles(folderBrowserDialog1.SelectedPath, "*.vtt", SearchOption.AllDirectories);
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
                foreach (string filename in files)
                {
                    lstFileVTT.Add(Path.GetFullPath(filename));
                }
                MessageBox.Show($"{ lstFileVTT.Count.ToString()} file(s) found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstFileVTT.Count == 0)
                {
                    MessageBox.Show("You should add some files!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    foreach (string iteem in lstFileVTT)
                    {
                        ConvertToSrt(iteem);
                    }
                    MessageBox.Show("Successfully finished !!", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {

            }
        }
        void ConvertToSrt(string filePathh)
        {
            try
            {
                using (StreamReader stream = new StreamReader(filePathh))
                {
                    //MessageBox.Show(filePathh);
                    StringBuilder output = new StringBuilder();
                    int lineNumber = 1;
                    while (!stream.EndOfStream)
                    {
                        string line = stream.ReadLine();
                        if (IsTimecode(line))
                        {
                            output.AppendLine(lineNumber.ToString());
                            lineNumber++;
                            line = line.Replace('.', ',');
                            line = DeleteCueSettings(line);
                            output.AppendLine(line);
                            bool foundCaption = false;
                            while (true)
                            {
                                if (stream.EndOfStream)
                                {
                                    if (foundCaption)
                                        break;
                                    else
                                        throw new Exception("Corrupted file: Found timecode without caption");
                                }
                                line = stream.ReadLine();
                                if (String.IsNullOrEmpty(line) || String.IsNullOrWhiteSpace(line))
                                {
                                    output.AppendLine();
                                    break;
                                }
                                foundCaption = true;
                                output.AppendLine(line);
                            }
                        }
                    }
                    
                    string fileName = Path.GetFileNameWithoutExtension(filePathh) + ".srt";
                    string finalfilepath = Path.GetDirectoryName(filePathh) + '\\' + fileName;
                    //MessageBox.Show(fileName);
                    using (StreamWriter outputFile = new StreamWriter(finalfilepath))
                        outputFile.Write(output);

                    // time repaire
                    string srcfilepath2 = finalfilepath;
                    string[] lines = File.ReadAllLines(srcfilepath2);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (IsTimecode(lines[i]))
                        {

                            string secdtime = lines[i].Substring(lines[i].IndexOf('>') + 2, lines[i].LastIndexOf(",") - (lines[i].IndexOf('>') + 2));
                            string newline;
                            string finalline;
                            try
                            {
                                string oldline = lines[i];

                                TimeSpan ts = new TimeSpan();
                                ts = TimeSpan.Parse(secdtime);

                                if (secdtime == ts.ToString())
                                {
                                    //MessageBox.Show("Equal " + secdtime);
                                }
                                else
                                {
                                    newline = oldline.Replace("--> ", "--> 00:");
                                    lines[i] = newline;
                                    //MessageBox.Show("false " + secdtime);
                                }
                                string frsttime = lines[i].Substring(0, lines[i].IndexOf(","));
                                TimeSpan ts2 = new TimeSpan();
                                ts2 = TimeSpan.Parse(frsttime);
                                if (frsttime == ts2.ToString())
                                {
                                    //MessageBox.Show("Equal " + secdtime);
                                }
                                else
                                {
                                    finalline = lines[i].Replace(lines[i], "00:" + lines[i]);
                                    lines[i] = finalline;
                                    //MessageBox.Show("false " + secdtime);

                                }
                            }
                            catch (Exception ex)
                            {
                                //do something...
                            }

                        }
                    }
                    File.WriteAllLines(srcfilepath2, lines);

                    //string srcfilepath3 = finalfilepath;
                    //string[] lines2 = File.ReadAllLines(srcfilepath3);
                    //for (int i2 = 0; i2 < lines2.Length; i2++)
                    //{
                    //    if (IsTimecode2(lines2[i2]))
                    //    {
                    //        string oldline = lines2[i2];
                    //        //MessageBox.Show(oldline);
                    //        string newline = oldline.Replace(" ", string.Empty);  // Remove spaces
                    //        //MessageBox.Show(newline);
                    //        string finalline = newline.Replace("->", " --> ");
                    //        string finalline2 = finalline.Replace('.', ',');
                    //        //MessageBox.Show(finalline);
                    //        lines2[i2] = finalline2;
                    //    }
                    //}
                    //File.WriteAllLines(srcfilepath3, lines2);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        bool IsTimecode(string line)
        {
            return line.Contains("-->");
        }

        string DeleteCueSettings(string line)
        {
            StringBuilder output = new StringBuilder();
            foreach (char ch in line)
            {
                char chLower = Char.ToLower(ch);
                if (chLower >= 'a' && chLower <= 'z')
                {
                    break;
                }
                output.Append(ch);
            }
            return output.ToString();
        }

    }
}
