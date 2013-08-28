using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using jQueryEasyUI_Tree.Helpers;
using jQueryEasyUI_Tree.ViewModels;

namespace jQueryEasyUI_Tree.Controllers
{
    public class HomeController : Controller
    {
        private EasyUITreeHelper _helper = new EasyUITreeHelper();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tree1()
        {
            //Display - Tree, ViewModel
            var rootNode = this._helper.GetRootNode();
            var nodes = this._helper.GetNodes();

            TreeViewModel model = new TreeViewModel
            {
                RootNode = rootNode,
                TreeNodes = nodes.ToList()
            };

            return View(model);
        }


        public ActionResult Tree2()
        {
            //Display - Tree, JSON

            ViewBag.HasRootNode = this._helper.GetRootNode() != null ? "true" : "false";
            return View();
        }

        [HttpPost]
        public ActionResult GetTreeNodeJSON()
        {
            string result = this._helper.GetTreeJson();
            return Content(result, "application/json");
        }


        public ActionResult Tree3()
        {
            //Display - Tree, Async

            ViewBag.HasRootNode = this._helper.GetRootNode() != null;
            return View();
        }

        [HttpPost]
        public ActionResult GetData(Guid? id)
        {
            string result = this._helper.GetNodes(id);
            return Content(result, "application/json");
        }

    }
}
