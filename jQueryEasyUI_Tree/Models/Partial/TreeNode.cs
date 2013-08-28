using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace jQueryEasyUI_Tree.Models
{
    [MetadataType(typeof(TreeNodeMetadata))]
    public partial class TreeNode
    {
        public class TreeNodeMetadata
        {
            [Display(Name = "節點編號")]
            [Required(ErrorMessage = "此為必要欄位")]
            public Guid ID { get; set; }

            [Display(Name = "上層節點編號")]
            public Guid ParentID { get; set; }

            [Display(Name = "節點名稱")]
            [Required(ErrorMessage = "此為必要欄位")]
            [StringLength(100, MinimumLength = 1, ErrorMessage = "長度為 1 ~ 100")]
            public string Name { get; set; }

            [Display(Name = "節點排序")]
            [Required(ErrorMessage = "此為必要欄位")]
            public int Sort { get; set; }

            [Display(Name = "是否啟用")]
            [Required(ErrorMessage = "此為必要欄位")]
            public bool IsEnable { get; set; }

            [Display(Name = "建立日期")]
            [Required(ErrorMessage = "此為必要欄位")]
            public DateTime CreateDate { get; set; }

            [Display(Name = "更新日期")]
            [Required(ErrorMessage = "此為必要欄位")]
            public DateTime UpdateDate { get; set; }
        }

        public TreeNode(bool isRoot = false)
        {
            this.ID = Guid.NewGuid();
            this.Sort = 9999;
            this.IsEnable = false;
            this.CreateDate = DateTime.Now;
            this.UpdateDate = DateTime.Now;
        }

        //-----------------------------------------------------------------------------------------
        //Extend

        public bool HasParent
        {
            get
            {
                return this.ParentID.HasValue;
            }
        }

        public bool IsRoot
        {
            get
            {
                return !this.HasParent;
            }
        }

        private TreeNode _ParentNode = null;
        public TreeNode ParentNode
        {
            get
            {
                if (this._ParentNode == null && this.ParentID.HasValue)
                {
                    using (Database1Entities db = new Database1Entities())
                    {
                        this._ParentNode = db.TreeNode.SingleOrDefault(x => x.ID == this.ParentID.Value);
                    }
                }
                return this._ParentNode;
            }
        }

        public bool HasChildren
        {
            get
            {
                return this.Children.Any();
            }
        }

        private IQueryable<TreeNode> _Children;
        public IQueryable<TreeNode> Children
        {
            get
            {
                if (this._Children == null)
                {
                    Database1Entities db = new Database1Entities();
                    this._Children = db.TreeNode
                        .Where(x => x.ParentID == this.ID)
                        .OrderBy(x => x.Sort)
                        .ThenBy(x => x.CreateDate);
                }
                return this._Children;
            }
        }

    }
}