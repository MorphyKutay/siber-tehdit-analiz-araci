using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Management;
using System.Dynamic;


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string a = "deneme";
            //listBox1.Items.Add(a);

            //static CountdownEvent countdown;

            renderProcessesOnListView();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Bu Program kutaySec tarafindan yazilmistir :)");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        // burasi ip find alani
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            Ping ping;
            IPAddress addr;
            PingReply pingReply;
            IPHostEntry host;
            string name;

            Parallel.For(0, 254, (i, loopState) =>
              {
                  ping= new Ping();
                  pingReply = ping.Send(textBox1.Text + i.ToString());

                  this.BeginInvoke((Action)delegate()
                  { 
                    if(pingReply.Status == IPStatus.Success)
                      {
                          try
                          {
                              addr = IPAddress.Parse(textBox1.Text + i.ToString());
                              host = Dns.GetHostEntry(addr);
                              name = host.HostName;

                              dataGridView1.Rows.Add();
                              int nRowIndex = dataGridView1.Rows.Count - 1;
                              dataGridView1.Rows[nRowIndex].Cells[0].Value = textBox1.Text + i.ToString();
                              dataGridView1.Rows[nRowIndex].Cells[1].Value = name;
                              dataGridView1.Rows[nRowIndex].Cells[2].Value = "Active";

                              var path = "data.txt";

                              string[] lines = { textBox1.Text + i.ToString(), name, "Active" };
                              File.WriteAllLines(path, lines);


                          }
                          catch (Exception ex)
                          {
                              name = "?";
                          }
                      }
                      /*else
                      {
                          MessageBox.Show("Hata");
                      }*/
                  });
            });

            MessageBox.Show("Tarama bitti");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void backgroundWorker3_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        // burasi process view alani

        public void renderProcessesOnListView()
        {
            Process[] processList = Process.GetProcesses();

            ImageList Imagelist = new ImageList();

            foreach (Process process in processList)
            {
                string status = (process.Responding == true ? "Responding" : "Not responding");

                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
 
                string[] row = {
                    
                    process.ProcessName,
    
                    process.Id.ToString(),
           
                    status,
                    extraProcessInfo.Username,
                    BytesToReadableValue(process.PrivateMemorySize64),

                    extraProcessInfo.Description
                };

                
                try
                {
                    Imagelist.Images.Add(
                        process.Id.ToString(),
                        Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap()
                    );
                }
                catch { }

                ListViewItem item = new ListViewItem(row)
                {
                    ImageIndex = Imagelist.Images.IndexOfKey(process.Id.ToString())
                };

                listView1.Items.Add(item);
            }

            listView1.LargeImageList = Imagelist;
            listView1.SmallImageList = Imagelist;
        }

       
        public string BytesToReadableValue(long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }

       
        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

           
            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "Unknown";

            foreach (ManagementObject obj in processList)
            {
               
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                   
                    response.Username = argList[0];

                }

               
                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }

            return response;
        }
    }
}