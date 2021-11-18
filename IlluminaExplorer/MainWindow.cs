using IlluminaExplorer.Common;
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
        IUserPickInfo userPickInfo = null;
        ITreeViewBuilder treeViewBuilder;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            treeViewBuilder = new TreeViewBuilder();
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

            treeViewBuilder.DefaultNode(tvFolder);
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
                treeViewBuilder.BuildNewNodes(tvFolder, folderBrowserDialog1.SelectedPath);
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
            DirectoryInfo info = new DirectoryInfo(userPickInfo.SelectedPath);
            var Totalfiles=info.GetFiles();
            SetFileText(lblFileSearched, Totalfiles.Length);
            var files = info.GetFiles( "*"+ txtFileName.Text+ "*" + userPickInfo.Extension);
            SetFileText(lblFiles, files.Length);
            SetProgressBar(files.Count());

            for (int i = 0; i < files.Length; i++)
            {
                IncreaseProgressBar(i);

                AddToList(files[i].ToString());
            }

        }

        private void SetFileText(Label label, int count) {
            if (label.InvokeRequired)
            {
                label.BeginInvoke((Action)delegate ()
                {
                    label.Text = count.ToString();
                });

            }
            else
            {
                label.Text = count.ToString();
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
                progressBar1.Value = count;
            


        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            lstFile.Items.Clear();
            Regex regex = new Regex("^[a-zA-Z0-9_]");
            bool containsSpecialCharacter = !regex.IsMatch(txtFileName.Text);
            if (containsSpecialCharacter)
                return;
            
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
                try
                {

             
                DataTable dt = new DataTable();
                using (var sr = new StreamReader(userPickInfo.FullPath))
                {
                    var fileContent = sr.ReadToEnd();
                    var lines = fileContent.Split('\n');
                    char deli = char.Parse(userPickInfo.Delimiter);
                    fileContent = null;//release the memory
                    int totalCount = lines.Length;
                    SetProgressBar(totalCount+1);
                    int rowCount = 0;

                    while (lines.Length >rowCount)
                    {
                        string[] rowValues = lines[rowCount].Split(deli);
                        //we assume header will always be first row in this template
                        if (rowCount == 0)
                        {
                            foreach (string header in rowValues)
                            {
                                dt.Columns.Add(header);
                            }
                        }
                        else
                        {
                            var row = dt.NewRow();
                         
                            for (int i = 0; i < rowValues.Length; i++)
                            {
                                row[i] = rowValues[i];
                            }
                            dt.Rows.Add(row);
                         
                        }
                        rowCount++;
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
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Ops! something went wrong.{ex.Message} ");
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
