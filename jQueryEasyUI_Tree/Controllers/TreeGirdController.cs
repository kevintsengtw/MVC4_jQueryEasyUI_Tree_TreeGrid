using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using jQueryEasyUI_Tree.Models;
using jQueryEasyUI_Tree.Services;

namespace jQueryEasyUI_Tree.Controllers
{
    public class TreeGirdController : Controller
    {
        private readonly ITreeNodeService _service;

        public TreeGirdController()
        {
            _service = new TreeNodeService();
        }


        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 檢查有無建立根節點
        /// </summary>
        /// <returns>回傳Json</returns>
        public ActionResult HasRootNode()
        {
            JObject jo = new JObject();
            bool result = this._service.GetRootNode() != null;
            jo.Add("Msg", result.ToString());
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        /// <summary>
        /// 取得編輯節點資料時所用的下拉選單option內容.
        /// </summary>
        /// <returns>回傳Json</returns>
        public ActionResult LoadTreeNodeDDL()
        {
            var items = this._service.GetTreeJson();
            return Content(items, "application/json");
        }

        /// <summary>
        /// 取得所有TreeNode的Json.
        /// </summary>
        /// <returns>回傳Json</returns>
        public ActionResult GetTreeNodeJSON()
        {
            string result = this._service.GetAllJson(true);
            return Content(result, "application/json");
        }

        /// <summary>
        /// Gets the tree node.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTreeNode(string id)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(id))
            {
                jo.Add("Msg", "需輸入TreeNode ID");
                return Content(JsonConvert.SerializeObject(jo));
            }

            Guid nodeID;
            if (!Guid.TryParse(id, out nodeID))
            {
                jo.Add("Msg", "TreeNode ID 錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                string jsonContent = this._service.GetSingleJSON(nodeID);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
        }

        /// <summary>
        /// 建立節點.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="name">The name.</param>
        /// <param name="enable">The enable.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(string parentId, string name, bool enable)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(parentId) || string.IsNullOrWhiteSpace(name))
            {
                jo.Add("Msg", "請輸入必要資料");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            bool isRoot = parentId.Equals("root", StringComparison.OrdinalIgnoreCase);

            Guid parentNodeID;
            bool parseResult = Guid.TryParse(parentId, out parentNodeID);

            if (!isRoot && !parseResult)
            {
                jo.Add("Msg", "上層節點 ID 資料錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            bool checkRoot = isRoot || this._service.IsExists(parentNodeID);

            if (!isRoot && !checkRoot)
            {
                jo.Add("Msg", "找不到指定的上層資料");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                jo.Add("Msg", "需輸入節點名稱");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                TreeNode node = new TreeNode();

                node.ID = Guid.NewGuid();
                if (!isRoot)
                {
                    node.ParentID = parentNodeID;
                }
                node.Name = name;
                node.IsEnable = enable;

                this._service.Create(node);

                jo.Add("Msg", "Success");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        /// <summary>
        /// 刪除節點.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string id)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(id))
            {
                jo.Add("Msg", "未指定TreeNode ID");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            Guid nodeID;
            if (!Guid.TryParse(id, out nodeID))
            {
                jo.Add("Msg", "TreeNode ID 錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            var node = this._service.GetSingle(nodeID);
            if (node == null)
            {
                jo.Add("Msg", "TreeNode不存在");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                this._service.Delete(new Guid(id));

                jo.Add("Msg", "Success");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        /// <summary>
        /// Updates the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <param name="name">The name.</param>
        /// <param name="enable">if set to <c>true</c> [enable].</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(string id, string parentId, string name, bool enable)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(id))
            {
                jo.Add("Msg", "沒有輸入 TreeNode ID");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            Guid nodeID;
            if (!Guid.TryParse(id, out nodeID))
            {
                jo.Add("Msg", "TreeNode ID 錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }
            if (!this._service.IsExists(nodeID))
            {
                jo.Add("Msg", "TreeNode 資料不存在");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            var node = this._service.GetSingle(nodeID);
            var originalParentID = node.ParentID.HasValue ? node.ParentID.Value : Guid.Empty;

            Guid parentNodeID = new Guid();
            if (!node.IsRoot)
            {
                if (!Guid.TryParse(parentId, out parentNodeID))
                {
                    jo.Add("Msg", "上層節點 ID 錯誤");
                    return Content(JsonConvert.SerializeObject(jo), "application/json");
                }
                if (!this._service.IsExists(parentNodeID))
                {
                    jo.Add("Msg", "上層節點資料不存在");
                    return Content(JsonConvert.SerializeObject(jo), "application/json");
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                jo.Add("Msg", "沒有輸入 TreeNode Name");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                if (!node.IsRoot)
                {
                    node.ParentID = parentNodeID;
                }
                node.Name = name;
                node.IsEnable = enable;

                this._service.Update(node, originalParentID);

                jo.Add("Msg", "Success");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        /// <summary>
        /// Moves up.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MoveUp(string id)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(id))
            {
                jo.Add("Msg", "沒有輸入TreeNode ID");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            Guid nodeID;
            if (!Guid.TryParse(id, out nodeID))
            {
                jo.Add("Msg", "TreeNode ID 錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                this._service.MoveDirection(nodeID, Direction.Up);
                
                jo.Add("Msg", "Success");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }

        /// <summary>
        /// Moves down.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MoveDown(string id)
        {
            JObject jo = new JObject();

            if (string.IsNullOrWhiteSpace(id))
            {
                jo.Add("Msg", "沒有輸入TreeNode ID");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            Guid nodeID;
            if (!Guid.TryParse(id, out nodeID))
            {
                jo.Add("Msg", "TreeNode ID 錯誤");
                return Content(JsonConvert.SerializeObject(jo), "application/json");
            }

            try
            {
                this._service.MoveDirection(nodeID, Direction.Down);

                jo.Add("Msg", "Success");
            }
            catch (Exception ex)
            {
                jo.Add("Msg", ex.Message);
            }
            return Content(JsonConvert.SerializeObject(jo), "application/json");
        }
    
    }
}
