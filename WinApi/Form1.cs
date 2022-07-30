using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Threading;

namespace WinApi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();       
        }

        private void GetRecord()
        {
            try
            {
                string token = ConfigurationManager.AppSettings["AuthToken"],
                       ownerId = ConfigurationManager.AppSettings["OwnerId"],
                       cabinetmatchedrulesequals = ConfigurationManager.AppSettings["cabinetmatchedrulesequals"];
                using (var client = new HttpClient())
                {
                    var reqObj = new RootRequest()
                    {
                        filters = new Filters()
                        {
                            policy = new Policy()
                            {
                                cabinetmatchedrulesequals = cabinetmatchedrulesequals.Split(new char[] { ',' }).ToList(),
                            },
                            OwnerEntity = new OwnerEntity()
                            {
                                eq = new List<Eq>()
                            {
                                new Eq() { id=ownerId,inst=0,saas=11161 }
                            }
                            }
                        },
                        skip = 0
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(reqObj), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

                    var result = client.PostAsync("https://diamondsg.us.portal.cloudappsecurity.com/api/v1/files/", jsonContent).Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ArgumentException("something bad happended");
                    }

                    var jsonResult = result.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<RootResponse>(jsonResult);
                    if (response.data.Count > 0)
                    {
                        progressBar.Value = 0;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = response.data.Count;

                        response.data.ForEach(i =>
                        {
                            i.createdDate = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(i.createdDate)).ToString();
                            progressBar.Value += 1;
                        });
                        dataGridView1.DataSource = response.data.ToList();
                        dataGridView1.AutoResizeColumns(
                   DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                    }
                    else
                    {
                        MessageBox.Show("Data is Empty.");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error :" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox.Visible = true;
            progressBar.Value = 0;
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.DataSource = null;
            }

            if (!backgroundWorker.IsBusy)
            {
                // This method will start the execution asynchronously in the background
                backgroundWorker.RunWorkerAsync();
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            progressBar.Value = 0;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string token = ConfigurationManager.AppSettings["AuthToken"],
                       ownerId = ConfigurationManager.AppSettings["OwnerId"],
                       cabinetmatchedrulesequals = ConfigurationManager.AppSettings["cabinetmatchedrulesequals"];
                using (var client = new HttpClient())
                {
                    var reqObj = new RootRequest()
                    {
                        filters = new Filters()
                        {
                            policy = new Policy()
                            {
                                cabinetmatchedrulesequals = cabinetmatchedrulesequals.Split(new char[] { ',' }).ToList(),
                            },
                            OwnerEntity = new OwnerEntity()
                            {
                                eq = new List<Eq>()
                            {
                                new Eq() { id=ownerId,inst=0,saas=11161 }
                            }
                            }
                        },
                        skip = 0
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(reqObj), Encoding.UTF8, "application/json");

                    client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

                    var result = client.PostAsync("https://diamondsg.us.portal.cloudappsecurity.com/api/v1/files/", jsonContent).Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ArgumentException("something bad happended");
                    }

                    var jsonResult = result.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<RootResponse>(jsonResult);
                    if (response.data.Count > 0)
                    {
                        progressBar.Value = 0;
                        //progressBar.Minimum = 0;
                        //progressBar.Maximum = response.data.Count;

                        e.Result = response.data;

                   //     response.data.ForEach(i =>
                   //     {
                   //         i.createdDate = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(i.createdDate)).ToString();
                   //         progressBar.Value += 1;
                   //     });
                   //     dataGridView1.DataSource = response.data.ToList();
                   //     dataGridView1.AutoResizeColumns(
                   //DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                    }
                    else
                    {
                        MessageBox.Show("Data is Empty.");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error :" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (backgroundWorker.IsBusy)
                {
                    // Cancel the asynchronous operation if still in progress
                    backgroundWorker.CancelAsync();
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error :" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Result != null)
            {
                List<ResponseRecord> response = (List<ResponseRecord>)e.Result;
                progressBar.Minimum = 0;
                progressBar.Maximum = response.Count;
                pictureBox.Visible = false;
                response.ForEach(i =>
                {
                    i.createdDate = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(i.createdDate)).ToString();
                    progressBar.Value += 1;
                    Thread.Sleep(100);
                });
                dataGridView1.DataSource = response.ToList();
                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            }
        }

        private void btnCSV_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Voilation.csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = dataGridView1.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[dataGridView1.Rows.Count + 1];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += dataGridView1.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;

                            for (int i = 1; (i - 1) < dataGridView1.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }


    }

}
