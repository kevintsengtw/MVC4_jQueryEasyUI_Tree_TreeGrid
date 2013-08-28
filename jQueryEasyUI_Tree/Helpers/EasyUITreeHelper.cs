using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using jQueryEasyUI_Tree.Models;
using jQueryEasyUI_Tree.Models.Repository;
using jQueryEasyUI_Tree.Services;

namespace jQueryEasyUI_Tree.Helpers
{
    public class EasyUITreeHelper
    {
        private readonly ITreeNodeService _service;

        public EasyUITreeHelper()
        {
            _service = new TreeNodeService();
        }

        /// <summary>
        /// Gets the root node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public TreeNode GetRootNode()
        {
            return this._service.GetRootNode();
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <param name="isReadAll">if set to <c>true</c> [is read all].</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IQueryable<TreeNode> GetNodes(bool isReadAll = false)
        {
            return _service.GetNodes(isReadAll);
        }

        /// <summary>
        /// Gets the descendant.
        /// </summary>
        /// <param name="isReadAll">if set to <c>true</c> [is read all].</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<TreeNode> GetDescendant(bool isReadAll = false)
        {
            var nodeList = new List<TreeNode>();

            TreeNode rootNode = this.GetRootNode();

            if (rootNode != null)
            {
                nodeList.Add(rootNode);
                var allNodes = this.GetNodes(isReadAll);
                this.GetChildNodes(allNodes.ToList(), rootNode, ref nodeList);
            }
            return nodeList;
        }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="nodeList">The node list.</param>
        private void GetChildNodes(IEnumerable<TreeNode> nodes, TreeNode parentNode, ref List<TreeNode> nodeList)
        {
            if (!nodes.Any()) return;
            if (!parentNode.HasChildren) return;
            if (!nodes.Contains(parentNode)) return;

            var children = nodes.Where(x => x.ParentID == parentNode.ID)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.CreateDate);

            foreach (var item in children)
            {
                if (!nodeList.Contains(item))
                {
                    nodeList.Add(item);
                }
                this.GetChildNodes(nodes, item, ref nodeList);
            }
        }

        /// <summary>
        /// Gets all json.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetTreeJson()
        {
            List<JObject> jObjects = new List<JObject>();

            var allNodes = this._service.GetNodes(true);
            if (allNodes.Any())
            {
                var rootNode = this.GetRootNode();
                JObject root = new JObject
                {
                    {"id", rootNode.ID.ToString()}, 
                    {"text", rootNode.Name}
                };
                root.Add("children", this.GetChildArray(rootNode, allNodes));
                jObjects.Add(root);
            }
            return JsonConvert.SerializeObject(jObjects);
        }

        /// <summary>
        /// Gets the child array.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        private JArray GetChildArray(TreeNode parentNode, IEnumerable<TreeNode> nodes)
        {
            JArray childArray = new JArray();

            foreach (var node in nodes.Where(x => x.ParentID == parentNode.ID).OrderBy(x => x.Sort))
            {
                JObject subObject = new JObject
                {
                    {"id", node.ID.ToString()}, 
                    {"text", node.Name}
                };

                if (nodes.Any(y => y.ParentID == node.ID))
                {
                    subObject.Add("children", this.GetChildArray(node, nodes));
                }
                childArray.Add(subObject);
            }

            return childArray;
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <param name="parentID">The parent ID.</param>
        /// <returns></returns>
        public string GetNodes(Guid? parentID)
        {
            List<JObject> jObjects = new List<JObject>();

            if (!parentID.HasValue)
            {
                var rootNode = this.GetRootNode();
                JObject node = new JObject
                {
                    {"id", rootNode.ID.ToString()}, 
                    {"text", rootNode.Name},
                    {"state", this._service.GetNodes().Any(x => x.ParentID == rootNode.ID) ? "closed" : "open" }
                };
                jObjects.Add(node);
            }
            else
            {
                var nodes = this._service.GetNodes().Where(x => x.ParentID == parentID).OrderBy(x => x.Sort);
                if (nodes.Any())
                {
                    foreach (var item in nodes)
                    {
                        JObject node = new JObject
                        {
                            {"id", item.ID.ToString()}, 
                            {"text", item.Name},
                            {"state", this._service.GetNodes().Any(x => x.ParentID == item.ID) ? "closed" : "open" }
                        };
                        jObjects.Add(node);
                    }
                }
            }
            return JsonConvert.SerializeObject(jObjects);
        }

    }
}