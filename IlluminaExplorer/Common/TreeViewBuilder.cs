using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IlluminaExplorer.Common
{
    public class TreeViewBuilder : ITreeViewBuilder
    {
        //create a dummy treeview
        public void DefaultNode(TreeView tv)
        {
            
            BuildNewNodes(tv, Environment.CurrentDirectory);
            if (tv.Nodes.Count > 0)
            {
                tv.SelectedNode = tv.Nodes[0];
            }

            tv.ExpandAll();

        }
        public void AddNodeToParent(TreeNode parent, TreeNode childNode)
        {
            parent.Nodes.Add(childNode);
        }
        public void BuildNewNodes(TreeView tv, string path)
        {
            //clear all the nodes before adding new view
            tv.Nodes.Clear();
            var FolderNames = path.Split('\\').LastOrDefault();
            //set the current folder as main node
            TreeNode main = new TreeNode()
            {
                Name = FolderNames,
                Text = FolderNames,
                Tag = path
            };

            //set all the folders
            FolderRecursive(path, main);
            tv.Nodes.Add(main);
            tv.ExpandAll();

        }

        private void FolderRecursive(string path, TreeNode parent)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                foreach (var item in info.GetDirectories())
                {
                    TreeNode node = new TreeNode()
                    {
                        Name = item.Name,
                        Text = item.Name,
                        Tag = item.FullName
                    };
                    parent.Nodes.Add(node);
                    //if there is folders in the sub folders.
                    //we will retrieve it
                    if (item.GetDirectories().Count() > 0)
                    {
                        FolderRecursive(item.FullName, node);
                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
