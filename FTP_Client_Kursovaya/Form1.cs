using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FTP_Client_Kursovaya
{
    public partial class Form1 : Form
    {

        string Host = "", UserName, Password;
        string first;

        public Form1()
        {
            InitializeComponent();
            listView1.MouseDoubleClick += new MouseEventHandler(listView1_MouseDoubleClick);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (Host == first)
            {
                MessageBox.Show("Вы уже в корне сервера");
                return;
            }
            Client client = new Client(Host, UserName, Password);
            string NewHost = new string(Host.ToCharArray().Reverse().ToArray());
            int ind = NewHost.IndexOf("/");
            NewHost = NewHost.Remove(0, ind+1);
            Host = new string(NewHost.ToCharArray().Reverse().ToArray());
            btnConnect_Click(null, null);

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string file = openFileDialog1.FileName;
            string file2 = new string(file.ToCharArray().Reverse().ToArray());
            int ind = file2.IndexOf("\\");
            file2 = file2.Substring(0, ind);
            file2 = new string(file2.ToCharArray().Reverse().ToArray());

            Client client = new Client(Host, UserName, Password);
            try
            {
                client.UploadFile(file, file2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            btnConnect_Click(null, null);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                MessageBox.Show("Выделите файл");
                return;
            }
            string ftype = listView1.SelectedItems[0].SubItems[1].Text.Trim();
            if (ftype != "<DIR>")
            {
                try
                {
                    string file = listView1.SelectedItems[0].SubItems[0].Text.Trim();
                    Client client = new Client(Host, UserName, Password);
                    if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                        return;
                    string downloadFile = saveFileDialog1.FileName;
                    client.DownloadFile(file, downloadFile);
                    btnConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите файл");
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                MessageBox.Show("Для удаления необходимо выделить файл или директорию, которую желаете удалить");
                return;
            }
            string ftype = listView1.SelectedItems[0].SubItems[1].Text.Trim();
            if (ftype != "<DIR>")
            {
                try
                {
                    string deletefile = listView1.SelectedItems[0].SubItems[0].Text.Trim();
                    Client client = new Client(Host, UserName, Password);
                    client.DeleteFile(deletefile);
                    btnConnect_Click(null, null);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                }
            }
            else
            {
                try
                {
                    string deletefolder = listView1.SelectedItems[0].SubItems[0].Text.Trim();
                    Client client = new Client(Host, UserName, Password);
                    client.RemoveDirectory(deletefolder);
                    btnConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            string dir_name = Interaction.InputBox("Задайте имя папки", "Create Folder");
            if (dir_name == "")
            {
                MessageBox.Show("Имя папки не может быть пустым");
                return;
            }
            Client client = new Client(Host, UserName, Password);
            client.MakeDirectory(dir_name);
            btnConnect_Click(null, null);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Host = "";
            first = "";
            listView1.Items.Clear();
            btnConnect.Visible = true;
            toolStrip1.Visible = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            if (txtHost.Text == null || txtHost.Text == "")
            {
                MessageBox.Show("Имя сервера не может быть пустым");
                return;
            }
            if (Host == "")
            {
                Host = txtHost.Text;
                first = Host;
            }
            UserName = txtUsername.Text;
            Password = txtPassword.Text;

            try
            {
                Client client = new Client(Host, UserName, Password);
                Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                List<FileDirectoryInfo> list = client.ListDirectoryDetails()
                                                    .Select(s =>
                                                    {
                                                        Match match = regex.Match(s);
                                                        if (match.Length > 5)
                                                        {                                                          
                                                            string type = match.Groups[1].Value == "d" ? "DIR.png" : "FILE.png";
                                                            string size = "";
                                                            if (long.Parse(match.Groups[3].Value) == 4096)
                                                            {
                                                                size = "<DIR>";
                                                            }
                                                            else
                                                            {
                                                                size = $"{long.Parse(match.Groups[3].Value) / 1024} кБ";
                                                            }
                                                            return new FileDirectoryInfo(size, type, match.Groups[6].Value, match.Groups[4].Value, txtHost.Text);
                                                        }
                                                        else return new FileDirectoryInfo();
                                                    }).ToList();

                foreach (var item in list)
                {
                    ListViewItem lvi = new ListViewItem();
                    if (item.Type == "DIR.png")
                    {
                        lvi.ImageIndex = 0;
                    }
                    else
                    {
                        lvi.ImageIndex = 1;
                    }
                    lvi.Text = item.Name;
                    lvi.SubItems.Add(item.FileSize);
                    lvi.SubItems.Add(item.Date);
                    if (item.Name != "" && item.Name != "." && item.Name != "..")
                    {
                        listView1.Items.Add(lvi);
                    }
                }
                btnConnect.Visible = false;
                toolStrip1.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                MessageBox.Show("Выделите папку для просмотра");
                return;
            }
            string ftype = listView1.SelectedItems[0].SubItems[1].Text.Trim();
            if (ftype == "<DIR>")
            {
                try
                {
                    string directory = listView1.SelectedItems[0].SubItems[0].Text.Trim();
                    Client client = new Client(Host, UserName, Password);
                    client.ChangeWorkingDirectory(directory);
                    Host = client.ReturnUri();
                    btnConnect_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Для просмотра файла необходимо его скачать");
            }

        }
    }
}
