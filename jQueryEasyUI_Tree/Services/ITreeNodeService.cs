using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jQueryEasyUI_Tree.Core;
using jQueryEasyUI_Tree.Models;

namespace jQueryEasyUI_Tree.Services
{
    public interface ITreeNodeService
    {
        IResult Create(TreeNode instance);
        IResult Update(TreeNode instance);
        IResult Update(TreeNode instance, Guid originalParentID);
        IResult MoveDirection(Guid id, Direction direction);
        IResult Delete(Guid id);
        IResult Delete(TreeNode instance);

        bool IsExists(Guid id);

        TreeNode GetRootNode();
        TreeNode GetSingle(Guid id);
        IQueryable<TreeNode> GetNodes(bool isReadAll = false);

        string GetTreeJson();
        string GetSingleJSON(Guid id);
        string GetAllJson(bool isReadAll = false);

    }
}
