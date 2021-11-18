using System.Windows.Forms;

namespace IlluminaExplorer.Common
{
    public interface ITreeViewBuilder
    {
        void AddNodeToParent(TreeNode parent, TreeNode childNode);
        void BuildNewNodes(TreeView tv, string path);
        void DefaultNode(TreeView tv);
    }
}