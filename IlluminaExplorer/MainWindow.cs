using IlluminaExplorer.DirectoryBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IlluminaExplorer
{
    public partial class MainWindow : Form
    {
        #region global variable
        UserPickInfo userPickInfo = null;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            userPickInfo = new UserPickInfo();
            InitData();
        }

        private void InitData() {
            InitStartUpTreeView();
            cbFileExtension.Items.Add(".csv");
            cbFileExtension.Items.Add(".txt");
            cbFileExtension.Items.Add("All");

            //set the format to csv since this is the typical format
            cbFileExtension.SelectedIndex = 0;
            cbDelimeter.SelectedIndex = 0;
        }

        private void InitStartUpTreeView() {

            TvNodeHelper.DefaultNode(tvFolder);


        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
         
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            lstFile.Items.Clear();
          var result= folderBrowserDialog1.ShowDialog();
            if (result==DialogResult.OK)
            {
                TvNodeHelper.ChangeMainFolder(tvFolder, folderBrowserDialog1.SelectedPath);
            }

        }



        private async void tvFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            lstFile.Items.Clear();
            if (tvFolder.SelectedNode is null || tvFolder.SelectedNode.Tag is null)
                return;

            SetUserInfo();
             //run as async to prevent folder with hundreads of files which may cause UI to lag
             await Task.Run(() => {
                ManipulateFiles();
            });

        }
        private void ManipulateFiles() {
    

            var info = FolderHelper.GetDirectoryInfo(userPickInfo.SelectedPath);
         
            var files = info.GetFiles( "*"+ txtFileName.Text+ "*" + userPickInfo.Extension);
            SetFileText(files.Length);
            SetProgressBar(files.Count());

            for (int i = 0; i < files.Length; i++)
            {
                IncreaseProgressBar(i);

                AddToList(files[i].ToString());
            }

        }

        private void SetFileText(int count) {
            if (lblFiles.InvokeRequired)
            {
                lstFile.BeginInvoke((Action)delegate ()
                {
                    lblFiles.Text = count.ToString();
                });

            }
            else
            {
                lblFiles.Text = count.ToString();
            }
           
        }
        private void AddToList(string fileName) {
            if (lstFile.InvokeRequired)
            {
                lstFile.BeginInvoke((Action)delegate ()
                {
                    lstFile.Items.Add(fileName);
                });

            }
            else
            {
                lstFile.Items.Add(fileName);
            }
           
        }
        private void SetProgressBar(int total) {
          
            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke((Action)delegate ()
                {
                    progressBar1.Visible = total > 0 ? true : false;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = total;
                });

            }
            else
            {
                progressBar1.Visible = total > 0 ? true : false;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = total;
            }

          

        }

        private void IncreaseProgressBar(int value) {
           
            int count = value + 1;
            if (progressBar1.InvokeRequired)
            {
                progressBar1.BeginInvoke((Action)delegate ()
                {
                    progressBar1.Value = count;
                });
               
            }
            else
            {
                progressBar1.Value = count;
            }


        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            lstFile.Items.Clear();
            Regex regex = new Regex("^[a-zA-Z0-9_]");
            bool containsSpecialCharacter = !regex.IsMatch(txtFileName.Text);
            if (containsSpecialCharacter)
            {
              
                return;
            }
            ManipulateFiles();
        }

       
        private void SetUserInfo()
        {
            userPickInfo.SelectedPath = (string)tvFolder.SelectedNode?.Tag;
            userPickInfo.FileName = lstFile.SelectedItem?.ToString() ?? "";
            if (cbFileExtension.SelectedItem.ToString() == "All")
            {
                if (lstFile.SelectedItem is null)
                {
                    userPickInfo.Extension = "*";
                }
                else {
                    
                    if (lstFile.SelectedItem.ToString().Contains(".csv")|| lstFile.SelectedItem.ToString().Contains(".txt"))
                    {
                        userPickInfo.Extension = lstFile.SelectedItem.ToString().Substring(lstFile.SelectedItem.ToString().Length - 3);
                    }
                  
                }
               

            }
            else
            {
                userPickInfo.Extension = cbFileExtension.SelectedItem?.ToString();
                }

            userPickInfo.Delimiter = cbDelimeter?.SelectedItem?.ToString();
        }
         
        private async void ImportFile() {

            await Task.Run(() =>
            {
                DataTable dt = new DataTable();
                using (var sr = new StreamReader(userPickInfo.FullPath))
                {
                    var fileContent = sr.ReadToEnd();
                    var lines = fileContent.Split('\n');
                    fileContent = null;
                    int totalCount = lines.Length;
                    SetProgressBar(totalCount+1);
                    int rowCount = 0;

                    while (lines.Length >rowCount)
                    {
                        //we assume header will always be first row in this template
                        if (rowCount == 0)
                        {
                            string[] headers = lines[rowCount].Split(char.Parse(userPickInfo.Delimiter));
                            foreach (string header in headers)
                            {
                                dt.Columns.Add(header);
                            }

                            rowCount++;
                        }
                        else
                        {
                            var row = dt.NewRow();
                            string[] rowValues = lines[rowCount].Split(char.Parse(userPickInfo.Delimiter));
                            for (int i = 0; i < rowValues.Length; i++)
                            {
                                row[i] = rowValues[i];
                            }
                            dt.Rows.Add(row);
                            rowCount++;
                        }
                        IncreaseProgressBar(rowCount);
                    }
                }

                if (progressBar1.InvokeRequired)
                {
                    dataGridView1.BeginInvoke((Action)delegate ()
                    {
                        dataGridView1.DataSource = dt;
                    });

                }
                else
                {
                    dataGridView1.DataSource = dt;
                }
               
            });

        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (!userPickInfo.FullPath.Contains(userPickInfo.Extension) || string.IsNullOrEmpty(userPickInfo.Extension) )
            {
                MessageBox.Show($"You are only allowed to open:.txt or .csv for viewing.");
                return;
            }
            ImportFile();
        }


        private void OnParameterChanged(object sender, EventArgs e)
        {
            lstFile.Items.Clear();
            SetUserInfo();
            ManipulateFiles();
        }

        private void lstFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetUserInfo();
        }

        private void txtFileName_KeyPress(object sender, KeyPressEventArgs e)
        {
            var regex = new Regex(@"[^a-zA-Z0-9\s_\b]");
            if (regex.IsMatch(e.KeyChar.ToString()))
            {
                e.Handled = true;
            }
        }
    }
}
