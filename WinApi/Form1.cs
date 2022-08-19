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
using WinApi.Models.SQLiteModels;
using WinApi.DBContext;

namespace WinApi
{
    public partial class Form1 : Form
    {
        List<ClassificationDetails> cd = new List<ClassificationDetails>();
        List<OwnerDetailsModel> owner = new List<OwnerDetailsModel>();
        List<VoilationModel> response = new List<VoilationModel>();
        DataContext db = new DataContext();
        public Form1(List<OwnerDetailsModel> ownerDetail)
        {
            owner = ownerDetail;
            InitializeComponent();       
        }
        private void Form1_Load(object sender, EventArgs e)
        {


            if (owner.Count > 0)
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
            else
            {
                MessageBox.Show("Please sign in before accessing this form", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //cmbSortByName.SelectedIndex = 0;
        }
        private void FillClassificationList()
        {      
            cd = db.ClassificationDetails.OrderBy(x => x.Name).ToList();
            cd.Add(new ClassificationDetails { Name = "", Id = "" });          
            db.Dispose();
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
                           ownerId = owner[0].Id,
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
                                    ClassificationDetails classification = cd.Where(x => x.Id == lableId).FirstOrDefault();
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
                response = (List<VoilationModel>)e.Result;
                FillGrid(response.OrderBy(x=> x.Name).ToList());
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

        //private void btnFilter_Click(object sender, EventArgs e)
        //{
        //    if (response.Count > 0)
        //    {              
        //        List<VoilationModel> flt = new List<VoilationModel>();
                
        //        if (cmbSortByName.Text == "File Name")
        //        {
        //            if (rbtnDesc.Checked)
        //            {
        //                flt = response.OrderByDescending(x => x.Name).ToList();
        //            }
        //            else
        //            {
        //                flt=response.OrderBy(x => x.Name).ToList();
        //            }
        //        }
        //        else if (cmbSortByName.Text == "File Source")
        //        {
        //            if (rbtnDesc.Checked)
        //            {
        //                flt = response.OrderByDescending(x => x.FilePath).ToList(); ;
        //            }
        //            else
        //            {
        //                flt = response.OrderBy(x => x.FilePath).ToList();
        //            }
        //        }
        //        else if (cmbSortByName.Text == "Created Date")
        //        {
        //            if (rbtnDesc.Checked)
        //            {
        //                flt = response.OrderByDescending(x => x.CreatedDate).ToList();
        //            }
        //            else
        //            {
        //                flt = response.OrderBy(x => x.CreatedDate).ToList();
        //            }
        //        }
        //        else if (cmbSortByName.Text == "Classification")
        //        {
        //            if (rbtnDesc.Checked)
        //            {
        //                flt = response.OrderByDescending(x => x.Classification).ToList();
        //            }
        //            else
        //            {
        //                flt = response.OrderBy(x => x.Classification).ToList();
        //            }
        //        }
        //        else if (cmbSortByName.Text == "Link")
        //        {
        //            if (rbtnDesc.Checked)
        //            {
        //                flt = response.OrderByDescending(x => x.AlternateLink).ToList();
        //            }
        //            else
        //            {
        //                flt = response.OrderBy(x => x.AlternateLink).ToList();
        //            }
        //        }
        //        else
        //        {
        //            flt = response.OrderBy(x => x.Name).ToList();
        //        }

        //        if (flt.Count > 0)
        //        {
        //            FillGrid(flt);
        //        }
        //        else
        //        {
        //            dataGridView1.Rows.Clear();
        //        }
               
        //    }
        //}

        private void FillGrid(List<VoilationModel> vm)
        {
            progressBar.Minimum = 0;
            progressBar.Value = 0;
            progressBar.Maximum = vm.Count;
            pictureBox.Visible = false;
            dataGridView1.Rows.Clear();
            foreach (var i in vm)
            {
                dataGridView1.Rows.Add(
                    i.Name,
                    i.OwnerName,
                    i.AppName,
                    i.AlternateLink,
                    i.CreatedDate,
                    i.FilePath,
                    i.Classification,
                    i.ClassificationId
                    );
                progressBar.Value += 1;
            }

            //dataGridView1.DataSource = vm.ToList();
            //dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            //progressBar.Value = response.Count;
        }

    }
}
