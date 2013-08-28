using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using jQueryEasyUI_Tree.Models;

namespace jQueryEasyUI_Tree.ViewModels
{
    public class TreeViewModel
    {
        public TreeNode RootNode { get; set; }
        public List<TreeNode> TreeNodes { get; set; }
    }

}