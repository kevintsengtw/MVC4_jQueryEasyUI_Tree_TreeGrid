using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using jQueryEasyUI_Tree.Models;
using jQueryEasyUI_Tree.Models.Repository;
using jQueryEasyUI_Tree.Core;

namespace jQueryEasyUI_Tree.Services
{
    public class TreeNodeService : ITreeNodeService
    {
        private Database1Entities dbContext = new Database1Entities();

        private readonly IRepository<TreeNode> _repository;

        public TreeNodeService()
        {
            this._repository = new DataRepository<TreeNode>(dbContext);
        }

        /// <summary>
        /// Creates the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IResult Create(TreeNode instance)
        {
            if (instance == null)
            {
                throw new InvalidOperationException("請輸入 TreeNode 資料");
            }

            IResult result = new Result(false);

            try
            {
                //決定排序
                if (instance.IsRoot)
                {
                    instance.Sort = 0;
                }
                else
                {
                    TreeNode parentNode = this.GetSingle(instance.ParentID.Value);

                    instance.Sort = !parentNode.HasChildren
                        ? 1
                        : parentNode.Children.Count() + 1;
                }

                instance.CreateDate = DateTime.Now;
                instance.UpdateDate = DateTime.Now;

                this._repository.Add(instance);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Updates the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">請輸入 TreeNode 資料</exception>
        public IResult Update(TreeNode instance)
        {
            if (instance == null)
            {
                throw new InvalidOperationException("請輸入 TreeNode 資料");
            }

            IResult result = new Result(false);

            try
            {
                instance.UpdateDate = DateTime.Now;

                this._repository.Update(instance);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }


        /// <summary>
        /// Updates the specified node.
        /// </summary>
        /// <param name="instance">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">請輸入 TreeNode 資料</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public IResult Update(TreeNode instance, Guid originalParentID)
        {
            if (instance == null)
            {
                throw new InvalidOperationException("請輸入 TreeNode 資料");
            }

            IResult result = new Result(false);

            var before = this.GetSingle(instance.ID);
            int beforeSort = instance.Sort;

            try
            {
                //判斷修改前後的 parentID 是否相同，如果不同則要重新調整 Sort 值
                if (!instance.IsRoot && !instance.ParentID.Equals(originalParentID))
                {
                    TreeNode parentNode = this.GetSingle(instance.ParentID.Value);
                    if (!parentNode.HasChildren)
                    {
                        instance.Sort = 1;
                    }
                    else
                    {
                        instance.Sort = parentNode.Children.Count() + 1;
                    }

                    //重新調整原來 parent node 的 ChildrenNode 排序
                    var beforeParentChildren = this.GetSingle(originalParentID).Children;
                    foreach (var item in beforeParentChildren.Where(x => x.Sort > beforeSort))
                    {
                        var currentNode = this.GetSingle(item.ID);
                        currentNode.Sort -= 1;
                        currentNode.UpdateDate = DateTime.Now;
                        this.Update(currentNode);
                    }
                }

                instance.UpdateDate = DateTime.Now;

                this._repository.Update(instance);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Moves the direction.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IResult MoveDirection(Guid id, Direction direction)
        {
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentNullException("沒有輸入TreeNode ID");
            }

            var instance = this.GetSingle(id);

            if (instance == null)
            {
                throw new InvalidOperationException("TreeNode 不存在");
            }

            if (instance.IsRoot)
            {
                throw new InvalidOperationException("根節點無法上移");
            }

            IResult result = new Result(false);

            try
            {
                var sameParentNodes = instance.ParentNode.Children.ToList();
                int currentNodeIndex = sameParentNodes.Select(x => x.ID).ToList().IndexOf(instance.ID);

                var firstOrDefault = sameParentNodes.FirstOrDefault();
                var lastOrDefault = sameParentNodes.LastOrDefault();

                switch (direction)
                {
                    case Direction.Up:
                        if (firstOrDefault != null && !firstOrDefault.ID.Equals(instance.ID))
                        {
                            //前一個節點往下移動
                            var preNodeID = sameParentNodes[currentNodeIndex - 1].ID;
                            var preNode = this.GetSingle(preNodeID);
                            preNode.Sort += 1;
                            preNode.UpdateDate = DateTime.Now;
                            this.Update(preNode);

                            //指定節點向上移動
                            instance.Sort -= 1;
                        }
                        break;

                    case Direction.Down:
                        if (lastOrDefault != null && !lastOrDefault.ID.Equals(instance.ID))
                        {
                            //後一個節點往上移動
                            var nextNodeID = sameParentNodes[currentNodeIndex + 1].ID;
                            var nextNode = this.GetSingle(nextNodeID);
                            nextNode.Sort -= 1;
                            nextNode.UpdateDate = DateTime.Now;
                            this.Update(nextNode);

                            //指定節點向下移動
                            instance.Sort += 1;
                        }
                        break;
                }

                instance.UpdateDate = DateTime.Now;

                this._repository.Update(instance);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Deletes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IResult Delete(Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentNullException("沒有輸入TreeNode ID");
            }
            return this.Delete(this.GetSingle(id));
        }

        /// <summary>
        /// Deletes the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IResult Delete(TreeNode instance)
        {
            if (null == instance)
            {
                throw new ArgumentNullException("需輸入TreeNode instance");
            }

            IResult result = new Result(false);

            try
            {
                var sameParentNodes = instance.ParentNode.Children;

                // 重新調整同層節點的排序
                foreach (var node in sameParentNodes.Where(x => x.Sort > instance.Sort))
                {
                    var modifyNode = this.GetSingle(node.ID);
                    modifyNode.Sort -= 1;
                    modifyNode.UpdateDate = DateTime.Now;
                    this.Update(modifyNode);
                }

                this._repository.Delete(instance);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Determines whether the specified id is exists.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        ///   <c>true</c> if the specified id is exists; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsExists(Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentNullException("沒有輸入TreeNode ID");
            }
            return this._repository.IsExists(x => x.ID == id);
        }


        /// <summary>
        /// Gets the root node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public TreeNode GetRootNode()
        {
            if (this._repository.Count() > 0)
            {
                return this._repository.FirstOrDefault(x => !x.ParentID.HasValue);
            }
            return null;
        }

        /// <summary>
        /// Gets the single.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public TreeNode GetSingle(Guid id)
        {
            if (this._repository.IsExists(x => x.ID == id))
            {
                return this._repository.FirstOrDefault(x => x.ID == id);
            }
            return null;
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <param name="isReadAll">if set to <c>true</c> [is read all].</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IQueryable<TreeNode> GetNodes(bool isReadAll = false)
        {
            var query = this._repository.GetAll();
            if (!isReadAll)
            {
                return query.Where(x => x.IsEnable == true);
            }
            return query;
        }

        /// <summary>
        /// Gets the tree json.
        /// </summary>
        /// <returns></returns>
        public string GetTreeJson()
        {
            List<JObject> jObjects = new List<JObject>();

            var allNodes = this.GetNodes(false);
            if (allNodes.Any())
            {
                var rootNode = this.GetRootNode();
                JObject root = new JObject
                {
                    {"id", rootNode.ID.ToString()}, 
                    {"text", rootNode.Name}
                };
                root.Add("children", this.GetChildJsonArray(rootNode, allNodes));
                jObjects.Add(root);
            }
            return JsonConvert.SerializeObject(jObjects);
        }

        /// <summary>
        /// Gets the child json array.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        private JArray GetChildJsonArray(TreeNode parentNode, IEnumerable<TreeNode> nodes)
        {
            JArray childArray = new JArray();

            foreach (var node in nodes.Where(x => x.ParentID == parentNode.ID).OrderBy(x => x.Sort))
            {
                JObject subObject = new JObject
                {
                    {"id", node.ID.ToString()}, 
                    {"text", node.Name}
                };

                if (nodes.Where(y => y.ParentID == node.ID).Any())
                {
                    subObject.Add("children", this.GetChildJsonArray(node, nodes));
                }
                childArray.Add(subObject);
            }

            return childArray;
        }


        /// <summary>
        /// Gets the single JSON.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetSingleJSON(Guid id)
        {
            if (id.Equals(Guid.Empty))
            {
                throw new ArgumentNullException("沒有輸入TreeNode ID");
            }
            var node = this.GetSingle(id);

            var jo = new JObject
            {
                {"ID", node.ID.ToString()},
                {"ParentID", node.ParentID.ToString()},
                {"Name", node.Name},
                {"Sort", node.Sort.ToString()},
                {"IsEnable", node.IsEnable.ToString()},
                {"IsRootNode", node.IsRoot.ToString()}
            };

            return JsonConvert.SerializeObject(jo);
        }


        /// <summary>
        /// Gets all json.
        /// </summary>
        /// <param name="isReadAll">if set to <c>true</c> [is read all].</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetAllJson(bool isReadAll = false)
        {
            List<JObject> jObjects = new List<JObject>();

            var allNodes = this.GetNodes(isReadAll);
            if (allNodes.Any())
            {
                var rootNode = this.GetRootNode();
                JObject root = this.GetNodeJsonObject(rootNode);
                root.Add("children", this.GetChildArray(allNodes, rootNode));
                jObjects.Add(root);
            }
            return JsonConvert.SerializeObject(jObjects);
        }

        /// <summary>
        /// Gets the child array.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parentNode">The parent node.</param>
        /// <returns></returns>
        private JArray GetChildArray(IEnumerable<TreeNode> nodes, TreeNode parentNode)
        {
            JArray childArray = new JArray();

            foreach (var node in nodes.Where(x => x.ParentID == parentNode.ID).OrderBy(x => x.Sort))
            {
                JObject subObject = this.GetNodeJsonObject(node);

                if (nodes.Where(y => y.ParentID == node.ID).Any())
                {
                    subObject.Add("children", this.GetChildArray(nodes, node));
                }
                childArray.Add(subObject);
            }

            return childArray;
        }

        /// <summary>
        /// Gets the node json object.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private JObject GetNodeJsonObject(TreeNode node)
        {
            var jsonObject = new JObject
            {
                {"ID", node.ID.ToString()}, 
                {"Name", node.Name}, //節點名稱
                {"Delete", !node.HasChildren}, //刪除(當有子節點存在時，該節點是不能被刪除的)
                {"IsEnable", node.IsEnable }, //是否啟用
                {"CreateDate", node.CreateDate.ToString()}, //建立日期
                {"UpdateDate", node.UpdateDate.ToString()} //更新日期
            };

            //節點上下移動
            if (node.ParentID.HasValue)
            {
                var sameParentNodes = node.ParentNode.Children.ToList();

                var firstOrDefault = sameParentNodes.FirstOrDefault();
                var lastOrDefault = sameParentNodes.LastOrDefault();

                jsonObject.Add("MoveUp", firstOrDefault != null && !firstOrDefault.ID.Equals(node.ID));
                jsonObject.Add("MoveDown", lastOrDefault != null && !lastOrDefault.ID.Equals(node.ID));
            }

            return jsonObject;
        }

    }

}