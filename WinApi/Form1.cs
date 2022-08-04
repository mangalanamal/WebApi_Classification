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
using WinApi.Models;

namespace WinApi
{
    public partial class Form1 : Form
    {
        List<ClassificationDetailsModel> cd = new List<ClassificationDetailsModel>();

        public Form1()
        {
            InitializeComponent();       
        }

        //private void GetRecord()
        //{
        //    try
        //    {
        //        string token = ConfigurationManager.AppSettings["AuthToken"],
        //               ownerId = ConfigurationManager.AppSettings["OwnerId"],
        //               cabinetmatchedrulesequals = ConfigurationManager.AppSettings["cabinetmatchedrulesequals"];
        //        using (var client = new HttpClient())
        //        {
        //            var reqObj = new RootRequest()
        //            {
        //                filters = new Filters()
        //                {
        //                    policy = new Policy()
        //                    {
        //                        cabinetmatchedrulesequals = cabinetmatchedrulesequals.Split(new char[] { ',' }).ToList(),
        //                    },
        //                    OwnerEntity = new OwnerEntity()
        //                    {
        //                        eq = new List<Eq>()
        //                    {
        //                        new Eq() { id=ownerId,inst=0,saas=11161 }
        //                    }
        //                    }
        //                },
        //                skip = 0
        //            };

        //            var jsonContent = new StringContent(JsonConvert.SerializeObject(reqObj), Encoding.UTF8, "application/json");

        //            client.DefaultRequestHeaders.Add("Authorization", $"Token {token}");

        //            var result = client.PostAsync("https://diamondsg.us.portal.cloudappsecurity.com/api/v1/files/", jsonContent).Result;
        //            if (!result.IsSuccessStatusCode)
        //            {
        //                throw new ArgumentException("something bad happended");
        //            }

        //            var jsonResult = result.Content.ReadAsStringAsync().Result;
        //            var response = JsonConvert.DeserializeObject<RootResponse>(jsonResult);
        //            if (response.data.Count > 0)
        //            {
        //                progressBar.Value = 0;
        //                progressBar.Minimum = 0;
        //                progressBar.Maximum = response.data.Count;

        //                response.data.ForEach(i =>
        //                {
        //                    i.createdDate = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(i.createdDate)).ToString();
        //                    progressBar.Value += 1;
        //                });
        //                dataGridView1.DataSource = response.data.ToList();
        //                dataGridView1.AutoResizeColumns(
        //           DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
        //            }
        //            else
        //            {
        //                MessageBox.Show("Data is Empty.");
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error :" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        private void Form1_Load(object sender, EventArgs e)
        {
            FillClassificationList();
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

        private void FillClassificationList()
        {
            cd = new List<ClassificationDetailsModel>
            {
                new ClassificationDetailsModel {Name ="",                          Id=""},
                new ClassificationDetailsModel {Name ="OFFICIAL - NON-SENSITIVE",                          Id="504426b3-c424-4327-b2a2-d702a47ba1f4"},
                new ClassificationDetailsModel {Name ="OFFICIAL - SENSITIVE NORMAL",                       Id="8476f4e5-9e41-451f-90e8-d65001245451"},
                new ClassificationDetailsModel {Name ="OFFICIAL - SENSITIVE HIGH",                         Id="8934a344-d598-4298-a95a-0a80110e8615"},
                new ClassificationDetailsModel {Name ="RESTRICTED - NON-SENSITIVE",                        Id="3ee5615d-784c-4bb8-b92d-674767327504"},
                new ClassificationDetailsModel {Name ="RESTRICTED - SENSITIVE NORMAL",                     Id="f8d78a06-1c64-4475-b3b9-e73361314834"},
                new ClassificationDetailsModel {Name ="RESTRICTED - SENSITIVE HIGH",                       Id="f07fc75d-7ba8-40fd-bba5-f81c996c98a3"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL (CLOUD ELIGIBLE) - NON-SENSITIVE",     Id="e116f5e9-65e7-4989-9411-329a81f5c891"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL (CLOUD ELIGIBLE) - SENSITIVE NORMAL",  Id="73d84901-ae7a-4646-87cf-ca1a3d33c12b"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL (CLOUD ELIGIBLE) - SENSITIVE HIGH",    Id="6ba114b7-5075-40e3-b82d-a86d69cdfeb5"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL - NON-SENSITIVE",                      Id="fd163c07-e199-433a-b6fc-81e91749b4c3"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL - SENSITIVE NORMAL" ,                  Id="a23b352c-ca19-4b35-80ae-f0574d085110"},
                new ClassificationDetailsModel {Name ="CONFIDENTIAL - SENSITIVE HIGH",                     Id="d1479dd0-0384-4dc5-9b2a-71bd5ed3290f"},
                new ClassificationDetailsModel {Name ="SECRET - NON-SENSITIVE",                            Id="2bd62ecb-02e5-49c4-9178-f63f67b8cf3f"},
                new ClassificationDetailsModel {Name ="SECRET - SENSITIVE NORMAL",                         Id="13c7d60e-8281-4033-90b3-4aa0d8ae9e18"},
                new ClassificationDetailsModel {Name ="SECRET - SENSITIVE HIGH",                           Id="f43701ea-618c-437b-9efa-eea39d0f0834"},

            };
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
                        List<VoilationModel> vdata = new List<VoilationModel>();

                        foreach (var i in response.data)
                        {
                            VoilationModel Obj = new VoilationModel();
                            Obj.Name = i.name;
                            Obj.OwnerName = i.ownerName;
                            Obj.AppName = i.appName;
                            Obj.AlternateLink = i.alternateLink;
                            Obj.CreatedDate = (new DateTime(1970, 1, 1)).AddMilliseconds(long.Parse(i.createdDate)).ToString();
                            Obj.FilePath = i.filePath;
                            if (i.fTags.Length > 0)
                            {
                                var fTags = i.fTags[0].Split(new string[] { "_" }, StringSplitOptions.None);
                                var lableId = fTags[1];
                                ClassificationDetailsModel classification = cd.Where(x => x.Id == lableId).FirstOrDefault();
                                if (classification != null)
                                {
                                    Obj.Classification = classification.Name;
                                    Obj.ClassificationId = lableId;
                                }
                                else
                                {
                                    Obj.Classification = "";
                                    Obj.ClassificationId = "";
                                }
                            }
                            else
                            {
                                Obj.Classification = "";
                                Obj.ClassificationId = "";
                            }

                            vdata.Add(Obj);
                        }

                        e.Result = vdata;
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
                List<VoilationModel> response = (List<VoilationModel>)e.Result;
                progressBar.Minimum = 0;
                progressBar.Maximum = response.Count;
                pictureBox.Visible = false;              
                dataGridView1.DataSource = response.ToList();
                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                progressBar.Value = response.Count;
            }
            else
            {
                pictureBox.Visible = false;
                progressBar.Value = 0;
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
