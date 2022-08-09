using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinApi.Models;
using Microsoft.InformationProtection;
using System.Configuration;

namespace WinApi
{
    public partial class ScanFiles : Form
    {
        List<ClassificationDetailsModel> classificationDetails = new List<ClassificationDetailsModel>();
        private static readonly string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static readonly string appName = ConfigurationManager.AppSettings["app:Name"];
        private static readonly string appVersion = ConfigurationManager.AppSettings["app:Version"];
        CheckBox headerCheckBox = new CheckBox();
        List<TableDetailsModel> TableDetails = new List<TableDetailsModel>();
        ApplicationInfo appInfo = new ApplicationInfo()
        {
            // ApplicationId should ideally be set to the same ClientId found in the Azure AD App Registration.
            // This ensures that the clientID in AAD matches the AppId reported in AIP Analytics.
            ApplicationId = clientId,
            ApplicationName = appName,
            ApplicationVersion = appVersion
        };
      
        public ScanFiles()
        {
            InitializeComponent();
             
        }
    
        private void ScanFiles_Load(object sender, EventArgs e)
        {
            FillClassificationList();
            DGVTableColumns();
            FillCmbFilter();
        }

        private void DGVTableColumns()
        {
            ////Add a CheckBox Column to the DataGridView Header Cell.

            ////Find the Location of Header Cell.
            //Point headerCellLocation = this.dataGridView1.GetCellDisplayRectangle(0, -1, true).Location;

            ////Place the Header CheckBox in the Location of the Header Cell.
            //headerCheckBox.Location = new Point(headerCellLocation.X + 8, headerCellLocation.Y + 2);
            //headerCheckBox.BackColor = Color.White;
            //headerCheckBox.Size = new Size(18, 18);

            ////Assign Click event to the Header CheckBox.
            //headerCheckBox.Click += new EventHandler(HeaderCheckBox_Clicked);
            //dataGridView1.Controls.Add(headerCheckBox);

            ////Add a CheckBox Column to the DataGridView at the first position.
            //DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            //checkBoxColumn.HeaderText = "";
            //checkBoxColumn.Width = 30;
            //checkBoxColumn.Name = "SelectRow";
            //dataGridView1.Columns.Insert(0, checkBoxColumn);

            ////Assign Click event to the DataGridView Cell.
            //dataGridView1.CellContentClick += new DataGridViewCellEventHandler(DataGridView_CellClick);

            //add new ComboBox

            var bindingSource = new BindingSource
            {
                DataSource = classificationDetails.ToList()
            };
            DataGridViewComboBoxColumn dgvCboColumn = new DataGridViewComboBoxColumn
            {
                Name = "Reclassification",
                DataSource = bindingSource.DataSource,  //DataTable that contains contact details
                DisplayMember = "Name",
                ValueMember = "Id",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dataGridView1.Columns.Add(dgvCboColumn);
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;

            this.dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;

        }

        private void FillCmbFilter()
        {
            var bindingSource = new BindingSource
            {
                DataSource = classificationDetails.ToList()
            };
            cmbFilter.DataSource = null;
            cmbFilter.DisplayMember = "Name";
            cmbFilter.ValueMember = "Id";
            cmbFilter.DataSource = bindingSource.DataSource;
            cmbFilter.SelectedIndex = -1;
        }
        private void FillClassificationList()
        {
            classificationDetails = new List<ClassificationDetailsModel>
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

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {                    
                    label3.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            cmbFilter.Enabled = false;
            if (!string.IsNullOrEmpty(label3.Text) && !string.IsNullOrEmpty(label4.Text))
            {
                pictureBox.Visible = true;
                progressBar.Value = 0;
                dataGridView1.Rows.Clear();
                if (!backgroundWorker.IsBusy)
                {
                    backgroundWorker.RunWorkerAsync();
                }              
            }
            else
            {
                MessageBox.Show("", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show("Please select files input folder path and files output folder path.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            
        }

        public static List<FileModel> GetFiles(List<string> DirectoriesList)
        {
            List<FileModel> fls = new List<FileModel>();
            string[] fe = {".docx",".doc",".ppt",".pptx",".xls",".xlsx"};
            foreach (var d in DirectoriesList)
            {
                string[] files = Directory.GetFiles(d);
                if (files.Length > 0)
                {
                    foreach (var f in files)
                    {
                        FileInfo fi = new FileInfo(f);

                       bool validFile = fe.Any(x => x.Contains(fi.Extension));
                        if (validFile)
                        {
                            var obj = new FileModel
                            {
                                DirectoryPath = d,
                                FilePath = f,
                                FileName = Path.GetFileName(f)
                            };

                            fls.Add(obj);
                        }
                    }
                }
            }
            return fls;
        }

        public static List<string> GetDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }

        private static List<string> GetDirectories(string path, string searchPattern)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    label4.Text = "";
                    if (!string.IsNullOrEmpty(label3.Text))
                    {
                        if ( label3.Text != fbd.SelectedPath)
                        {
                         label4.Text = fbd.SelectedPath;
                        }
                        else
                        {
                            MessageBox.Show("Files output folder path cannot be the same files input folder path.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select files input folder path before selecting files output folder path.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btnApplyLable_Click(object sender, EventArgs e)
        {
            pictureBox.Visible = true;
            progressBar.Value = 0;
            progressBar = new ProgressBar();
            if (!bgwApplyLable.IsBusy)
            {
                bgwApplyLable.RunWorkerAsync();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                progressBar.Value = 0;
                dataGridView1.Rows.Clear();
                if (!string.IsNullOrEmpty(label3.Text))
                {
                    //pictureBox.Visible = true;
                    List<string> dirs = GetDirectories(label3.Text);
                    dirs.Add(label3.Text);
                    TableDetails = new List<TableDetailsModel>();
                    // Initialize Action class, passing in AppInfo.
                    Action action = new Action(appInfo);
                    List<FileModel> files = GetFiles(dirs);
                    if (files.Count > 0)
                    {                        
                        foreach (var i in files)
                        {
                            Action.FileOptions options = new Action.FileOptions
                            {
                                FileName = i.FilePath,
                                //OutputName = outputFilePath,
                                ActionSource = ActionSource.Manual,
                                AssignmentMethod = AssignmentMethod.Privileged,
                                DataState = DataState.Rest,
                                GenerateChangeAuditEvent = true,
                                IsAuditDiscoveryEnabled = true,
                                //LabelId = labelId
                            };

                            var content = action.GetLabel(options);
                            if (content != null)
                            {
                                string LableId = content.Label.Id;
                                TableDetailsModel obj = new TableDetailsModel
                                {
                                    FilePath = i.FilePath,
                                    FileName = i.FileName,
                                    Classification = classificationDetails.Where(x => x.Id == LableId).FirstOrDefault().Name,
                                    ID = LableId
                                };
                                TableDetails.Add(obj);
                            }
                            else
                            {
                                TableDetailsModel obj = new TableDetailsModel
                                {
                                    FilePath = i.FilePath,
                                    FileName = i.FileName,
                                    Classification = "",
                                    ID = ""
                                };
                                TableDetails.Add(obj);
                            }


                        }

                        if (TableDetails.Count > 0)
                        {                            
                            e.Result = TableDetails;
                        }
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
            progressBar.Value = e.ProgressPercentage;    
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error :" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox.Visible = false;
            }
            else if (e.Result != null)
            {
                string FilerLableId = cmbFilter.SelectedIndex > -1 ? cmbFilter.SelectedValue.ToString() : "";
                List<TableDetailsModel> tableDetails = (List<TableDetailsModel>)e.Result;
                if (!string.IsNullOrEmpty(FilerLableId))
                {
                    tableDetails = tableDetails.Where(x => x.ID == FilerLableId).ToList();
                }
                progressBar.Value = 0;
                progressBar.Minimum = 0;
                progressBar.Maximum = tableDetails.Count;
                pictureBox.Visible = false;
                foreach (var r in tableDetails)
                {
                    progressBar.Value += 1;
                    Thread.Sleep(100);
                    dataGridView1.Rows.Add(
                        r.FileName,
                        r.FilePath,
                        r.Classification,
                        r.ID
                        );

                }
            }
            else
            {
                pictureBox.Visible = false;
            }
            cmbFilter.Enabled = true;
            //MessageBox.Show("Classification changed Successfully !!!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void bgwApplyLable_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0 && !string.IsNullOrEmpty(label4.Text))
                {
                    List<TableDetailsModel> al = new List<TableDetailsModel>();
                 
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty((dataGridView1.Rows[i].Cells[4] as DataGridViewComboBoxCell).FormattedValue.ToString()))
                        {
                            TableDetailsModel obj = new TableDetailsModel();

                            obj.FileName = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            obj.FilePath = dataGridView1.Rows[i].Cells[1].Value.ToString();
                            obj.Classification = dataGridView1.Rows[i].Cells[2].Value.ToString();
                            obj.ID = dataGridView1.Rows[i].Cells[3].Value.ToString();
                            obj.Reclassification = (dataGridView1.Rows[i].Cells[4] as DataGridViewComboBoxCell).FormattedValue.ToString();
                            obj.ReclassificationId = dataGridView1.Rows[i].Cells[4].Value.ToString();
                            al.Add(obj);
                        }
                    }

                    if (al.Count > 0)
                    {
                        int progress = 0;
                        Action action = new Action(appInfo);
                        foreach (var i in al)
                        {
                            string outputFilePath = label4.Text + "\\" + i.FileName;
                            int num = 1;
                            do
                            {
                                if (File.Exists(outputFilePath))
                                {
                                    FileInfo fi = new FileInfo(i.FileName);
                                    outputFilePath = (label4.Text + "\\" + fi.Name.Replace(fi.Extension, "") + "(" + num.ToString() + ")" + fi.Extension);
                                }
                                else
                                {
                                    break;
                                }
                            } while (true);

                        
                            Action.FileOptions options = new Action.FileOptions
                            {
                                FileName = i.FilePath,
                                OutputName = outputFilePath,
                                ActionSource = ActionSource.Manual,
                                AssignmentMethod = AssignmentMethod.Privileged,
                                DataState = DataState.Rest,
                                GenerateChangeAuditEvent = true,
                                IsAuditDiscoveryEnabled = true,
                                LabelId = i.ReclassificationId
                            };

                            // Set label, commit change to outputfile, and send audit event if enabled.
                            bool result = action.SetLabel(options);

                            if (result)
                            {
                                // Update options to read the previously generated file output.
                                options.FileName = options.OutputName;
                            }
                            if (al.Count != progress)
                            {
                                progress += 1;
                            }
                            bgwApplyLable.ReportProgress(progress);
                            Thread.Sleep(100);
                            
                        }

                        if (al.Count == progressBar.Value)
                        {
                            e.Result = "Classification changed Successfully !!!";
                            Log(al, label4.Text);
                        }
                    }
                    else
                    {
                        e.Result = "Please select Reclassification value";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error :" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (bgwApplyLable.IsBusy)
                {
                    // Cancel the asynchronous operation if still in progress
                    bgwApplyLable.CancelAsync();
                }
            }
        }

        private void bgwApplyLable_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void bgwApplyLable_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error :" + e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Result != null)
            {
                pictureBox.Visible = false;
                MessageBox.Show(e.Result.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                pictureBox.Visible = false;
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            dataGridView1.Rows.Clear();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                //You can check for e.ColumnIndex to limit this to your specific column
                var editingControl = this.dataGridView1.EditingControl as DataGridViewComboBoxEditingControl;
                if (editingControl != null)
                    editingControl.DroppedDown = true;
            }
        }
   
        private void Log(List<TableDetailsModel> al,string OutputFilePath)
        {
            if (al.Count > 0)
            {
                string fileName = "Log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                string logDirectoryPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log";
                if (!Directory.Exists(logDirectoryPath))
                {
                    Directory.CreateDirectory(logDirectoryPath);
                }
                fileName = (logDirectoryPath + "\\" + fileName);
                string columnNames = "";
                string[] outputCsv = new string[al.Count + 1];

                columnNames += "File Name" + ",";
                columnNames += "File Path " + ",";
                columnNames += "Classification" + ",";
                columnNames += "Lable ID" + ",";
                columnNames += "Reclassification" + ",";
                columnNames += "Reclassification ID" + ",";
                columnNames += "Output File Path" + ",";
                columnNames += "Create Date & Time" + ",";
                outputCsv[0] += columnNames;

                for (int i = 1; i < al.Count; i++)
                {
                    //foreach (var r in al)
                    //{
                        outputCsv[i] += al[i].FileName + ",";
                        outputCsv[i] += al[i].FilePath + ",";
                        outputCsv[i] += al[i].Classification + ",";
                        outputCsv[i] += al[i].ID + ",";
                        outputCsv[i] += al[i].Reclassification + ",";
                        outputCsv[i] += al[i].ReclassificationId + ",";
                        outputCsv[i] += OutputFilePath + ",";
                        outputCsv[i] += DateTime.Now.ToString("yyyy-MM-dd hh:mm") + ",";
                    //}
                }
                File.WriteAllLines(fileName, outputCsv, Encoding.UTF8);
            }
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TableDetails.Count > 0)
            {
                string FilerLableId = cmbFilter.SelectedIndex > -1 ? cmbFilter.SelectedValue.ToString() : "";
                if (!string.IsNullOrEmpty(FilerLableId))
                {
                    dataGridView1.Rows.Clear();
                    List<TableDetailsModel> td = TableDetails.Where(x => x.ID == FilerLableId).ToList();
                    progressBar.Value = 0;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = td.Count;
                    pictureBox.Visible = false;
                    foreach (var r in td)
                    {
                        progressBar.Value += 1;
                        Thread.Sleep(100);
                        dataGridView1.Rows.Add(
                            r.FileName,
                            r.FilePath,
                            r.Classification,
                            r.ID
                            );

                    }
                }
                else
                {
                    dataGridView1.Rows.Clear();
                    progressBar.Value = 0;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = TableDetails.Count;
                    pictureBox.Visible = false;
                    foreach (var r in TableDetails)
                    {
                        progressBar.Value += 1;
                        Thread.Sleep(100);
                        dataGridView1.Rows.Add(
                            r.FileName,
                            r.FilePath,
                            r.Classification,
                            r.ID
                            );

                    }
                }
            }
        }
    }
}
