;
(function (app) {
    //===========================================================================================
    var current = app.TreeNode = {};
    //===========================================================================================

    jQuery.extend(app.TreeNode,
        {

            ActionUrls: {},
            EditTreeNode: {},
            EditType: '',
            HasRootNode: 'False',

            Initialize: function (actionUrls) {
                /// <summary>
                /// Initialize
                /// </summary>
                /// <param name="actionUrls"></param>

                jQuery.extend(project.ActionUrls, actionUrls);
    
                current.CheckHasRootNode();
                current.Initilaize_TreeNodeDLL();

                if (current.HasRootNode == 'True') {
                    current.Initialize_TreeGrid();
                }

                $('#ButtonCreate').click(function () {
                    current.MoveDownEditType = 'Create';
                    $('#EditDialog').dialog('open').dialog('setTitle', '新增節點');
                    $('#EditForm').form('clear');
                    $('#ParentNode')[0].selectedIndex = 0;
                });

                $('#ButtonRefresh').click(function () {
                    current.Initialize_TreeGrid(); //重新載入TreeGrid
                    current.Initilaize_TreeNodeDLL(); //重新載入TreeNode下拉選單
                });

                $('#ButtonCancel').click(function () {
                    $('#EditForm').form('clear');
                    $('#EditDialog').dialog('close');
                });

                $('#ButtonSave').click(function () {
                    current.ButtonSaveEventHandler();
                });

            },

            CheckHasRootNode: function() {
                /// <summary>
                /// 檢查是否有建立根節點
                /// </summary>

                $.ajax({
                    type: 'Get',
                    url: project.ActionUrls.HasRootNode,
                    dataType: 'json',
                    cache: false,
                    async: false,
                    success: function (data) {
                        if (data.Msg) {
                            current.HasRootNode = data.Msg;
                            if (current.HasRootNode == 'False') {
                                $.messager.confirm('訊息', '尚未建立根節點, 是否新增根節點?', function (result) {
                                    if (result) {
                                        $('#ButtonCreate').trigger('click');
                                    }
                                });
                            }
                        }
                    }
                });
            },

            Initilaize_TreeNodeDLL: function() {
                /// <summary>
                /// 載入TreeNodeDDL
                /// </summary>

                $('#ParentNode').combotree({
                    url: project.ActionUrls.LoadTreeNodeDDL
                });
            },

            Initialize_TreeGrid: function() {
                /// <summary>
                /// 初始化TreeGrid
                /// </summary>

                $('#TreeGrid').treegrid({
                    url: project.ActionUrls.GetTreeNodeJSON,
                    width: 920,
                    height: 400,
                    idField: "ID",
                    treeField: 'Name',
                    nowrap: false,
                    rownumbers: true,
                    animate: true,
                    collapsible: true,
                    columns:
                    [[
                        {
                            field: 'Optional',
                            title: '功能',
                            width: 180,
                            align: 'center',
                            formatter: function (value, row) {
                                var content = '';

                                //上移
                                if (row.MoveUp) {
                                    content += String.format('<a class="MoveUp" nodeID="{0}" style="cursor:pointer; color:#0000ff;">上移</a>　', row.ID);
                                } else {
                                    content += '上移　';
                                }

                                //下移
                                if (row.MoveDown) {
                                    content += String.format('<a class="MoveDown" nodeID="{0}" style="cursor:pointer; color:#0000ff;">下移</a>　', row.ID);
                                } else {
                                    content += '下移　';
                                }

                                //編輯
                                content += String.format('<a class="edit" nodeID="{0}" style="cursor:pointer; color:#0000ff;">編輯</a>　', row.ID);

                                //刪除
                                if (row.Delete) {
                                    content += String.format('<a class="delete" nodeID="{0}" style="cursor:pointer; color:#0000ff;">刪除</a>　', row.ID);
                                } else {
                                    content += '刪除　';
                                }
                                return content;
                            }
                        },
                        { field: 'Name', title: '節點名稱', align: 'left' },
                        {
                            field: 'IsEnable',
                            title: '是否啟用',
                            width: 80,
                            align: 'center',
                            formatter: function (value, row) {
                                if (value) {
                                    return '啟用';
                                } else {
                                    return '不啟用';
                                }
                            }
                        },
                        { field: 'CreateDate', title: '建立日期', width: 200, align: 'center' },
                        { field: 'UpdateDate', title: '更新日期', width: 200, align: 'center' }
                    ]],
                    onLoadSuccess: function (row) {
                        //於 TreeGrid 類的樹狀節點資料載入完畢後，綁定每個 Row 裡的四個功能的操作事件：
                        //上移, 下移, 編輯, 刪除
                        current.MoveUp();
                        current.MoveDown();
                        current.EditNode();
                        current.DeleteNode();
                    }
                });
            },

            ButtonSaveEventHandler: function() {
                /// <summary>
                /// 儲存按鍵的事件處理
                /// </summary>

                var saveType = current.EditType.length == 0
                    ? '新增'
                    : current.EditType == 'Create' ? '新增' : '更新';

                if (saveType == '新增') {
                    current.CreateNode();
                }
                if (saveType == '更新') {
                    current.UpdateNode();
                }
            },

            CreateNode: function() {
                /// <summary>
                /// 新增節點
                /// </summary>

                var message = '';

                var parentId = $.trim($('#EditDialog #ParentNode').combotree('getValue'));
                parentId = parentId.length == 0 ? 'root' : parentId;

                var nodeName = $.trim($('#NodeName').val());
                var nodeEnable = $('#NodeEnable').is(':checked');

                if (current.HasRootNode == 'True' && parentId.length == 0) {
                    message += "請選擇上層節點.<br/>";
                }
                if (nodeName.length == 0) {
                    message += "請輸入節點名稱";
                }
                if (message.length > 0) {
                    $.messager.alert('錯誤', message, 'error');
                } else {
                    $.messager.confirm('Confirm', '確定新增此節點資料嗎?', function (result) {
                        if (result) {
                            var mapData = { parentId: parentId, name: nodeName, enable: nodeEnable };

                            $.ajax({
                                type: 'post',
                                url: project.ActionUrls.Create,
                                data: mapData,
                                dataType: 'json',
                                cache: false,
                                async: false,
                                success: function (data) {
                                    if (data.Msg) {
                                        if (data.Msg != 'Success') {
                                            AlertErrorMessage('建立錯誤', data.Msg);
                                        } else {
                                            $('#EditDialog').dialog('close');

                                            project.ShowMessage('訊息', '節點建立完成.');

                                            //清空編輯Dialog的內容
                                            $('#EditDialog #NodeName').val('');
                                            $('#EditDialog #NodeEnable').attr('checked', false);
                                            $('#EditDialog #ParentNode')[0].selectedIndex = 0;

                                            current.Initialize_TreeGrid();  //重新載入TreeGrid
                                            current.Initilaize_TreeNodeDLL();   //重新載入TreeNode下拉選單
                                        }
                                    } else {
                                        project.AlertErrorMessage('錯誤', '處理出現錯誤');
                                    }
                                },
                                error: function () {
                                    project.AlertErrorMessage('錯誤', '出現錯誤');
                                }
                            });
                        }
                    });
                }
            },

            GetTreeNodeData: function(nodeId) {
                //<summary>取得單一節點的Json資料</summary>

                $.ajax({
                    type: 'post',
                    url: project.ActionUrls.GetTreeNode,
                    data: { id: nodeId },
                    async: false,
                    cache: false,
                    dataType: 'json',
                    success: function (data) {
                        if (data.Msg) {
                            project.AlertErrorMessage('建立錯誤', data.Msg);
                        }
                        else {
                            EditTreeNode =
                            {
                                ID: data.ID,
                                ParentID: data.ParentID,
                                Name: data.Name,
                                Sort: data.Sort,
                                IsEnable: data.IsEnable == 'True',
                                IsRootNode: data.IsRootNode == 'True'
                            };
                        }
                    },
                    error: function () {
                        project.AlertErrorMessage('錯誤', '出現錯誤');
                    }
                });
            },

            EditNode: function() {
                ///<summary>編輯節點</summary>

                $('.edit').each(function (i, item) {
                    $(item).click(function () {
                        var nodeID = $(item).attr('nodeid');

                        //先取得節點的資料,以載入編輯Dialog中
                        current.GetTreeNodeData(nodeID);

                        if (!EditTreeNode.ID) {
                            $.messager.alert('錯誤', '節點資料載入錯誤', 'error', function () {
                                current.Initialize_TreeGrid();
                                current.Initilaize_TreeNodeDLL();
                            });
                        }
                        else {
                            current.EditType = 'Update';
                            $('#EditDialog').dialog('open').dialog('setTitle', '更新節點');

                            $('#EditDialog #NodeID').val(EditTreeNode.ID);
                            $('#EditDialog #NodeName').val(EditTreeNode.Name);
                            $('#EditDialog #IsRootNode').val(EditTreeNode.IsRootNode);
                            $('#EditDialog #ParentNode').combotree('setValue', EditTreeNode.ParentID);
                            $('#EditDialog #NodeEnable').prop('checked', EditTreeNode.IsEnable);
                        }
                    });
                });
            },

            UpdateNode: function() {
                /// <summary>
                /// 更新節點
                /// </summary>

                var message = '';

                var nodeId = $.trim($('#NodeID').val());
                var parentId = $.trim($('#EditDialog #ParentNode').combotree('getValue'));
                var nodeName = $.trim($('#NodeName').val());
                var nodeEnable = $('#NodeEnable').is(':checked');
                var isRootNode = $('#IsRootNode').val() == 'true';

                if (nodeId.length == 0) {
                    message += "無此節點 ID 資料.<br/>";
                }
                if (!isRootNode && parentId.length == 0) {
                    message += "請選擇上層節點.<br/>";
                }
                if (parentId == nodeId) {
                    message += "不可選擇自己為上層節點.<br/>";
                }
                if (nodeName.length == 0) {
                    message += "請輸入節點名稱";
                }
                if (message.length > 0) {
                    $.messager.alert('錯誤', message, 'error');
                }
                else {
                    $.messager.confirm('Confirm', '確定更新此節點資料嗎?', function (result) {
                        if (result) {
                            var mapData = { id: nodeId, parentId: parentId, name: nodeName, enable: nodeEnable };

                            $.ajax({
                                type: 'post',
                                url: project.ActionUrls.Update,
                                data: mapData,
                                dataType: 'json',
                                cache: false,
                                async: false,
                                success: function (data) {
                                    if (data.Msg) {
                                        if (data.Msg != 'Success') {
                                            project.AlertErrorMessage('更新錯誤', data.Msg);
                                        }
                                        else {
                                            $('#EditDialog').dialog('close');

                                            project.ShowMessage('訊息', '節點更新完成');

                                            //清空編輯Dialog的內容
                                            $('#EditDialog #NodeName').val('');
                                            $('#EditDialog #NodeEnable').attr('checked', false);
                                            $('#EditDialog #ParentNode')[0].selectedIndex = 0;

                                            current.Initialize_TreeGrid();  //重新載入TreeGrid
                                            current.Initilaize_TreeNodeDLL();   //重新載入TreeNode下拉選單
                                        }
                                    }
                                    else {
                                        project.AlertErrorMessage('錯誤', '處理出現錯誤');
                                    }
                                },
                                error: function () {
                                    project.AlertErrorMessage('錯誤', '出現錯誤');
                                }
                            });
                        }
                    });
                }
            },

            DeleteNode: function() {
                //<summary>刪除節點</summary>

                $('.delete').each(function (i, item) {
                    $(item).click(function () {
                        var nodeID = $(item).attr('nodeid');
                        if (nodeID.length > 0) {
                            $.messager.confirm('Confirm', '確定刪除此節點嗎?', function (result) {
                                if (result) {
                                    $.ajax({
                                        type: 'post',
                                        url: project.ActionUrls.Delete,
                                        data: { id: nodeID },
                                        async: false,
                                        cache: false,
                                        dataType: 'json',
                                        success: function (data) {
                                            if (data.Msg) {
                                                if (data.Msg != 'Success') {
                                                    $.messager.alert('錯誤', data.Msg, 'error');
                                                }
                                                else {
                                                    project.ShowMessage('訊息', '節點刪除完成.');

                                                    current.Initialize_TreeGrid();  //重新載入TreeGrid
                                                    current.Initilaize_TreeNodeDLL();   //重新載入TreeNode下拉選單
                                                }
                                            }
                                            else {
                                                project.AlertErrorMessage('錯誤', '處理出現錯誤');
                                            }
                                        },
                                        error: function () {
                                            project.AlertErrorMessage('錯誤', '出現錯誤');
                                        }
                                    });
                                }
                            });
                        }
                    });
                });
            },

            MoveUp: function() {
                //<summary>移動節點：上移</summary>

                $('.MoveUp').each(function (i, item) {
                    $(item).click(function () {

                        var nodeID = $(item).attr('nodeid');
                        var nodeName = $.trim($('[id^=datagrid-row][id$=' + nodeID + '] .tree-title').text());

                        if (nodeID.length > 0) {
                            $.ajax({
                                type: 'post',
                                url: project.ActionUrls.MoveUp,
                                data: { id: nodeID },
                                async: false,
                                cache: false,
                                dataType: 'json',
                                success: function (data) {
                                    if (data.Msg) {
                                        if (data.Msg != 'Success') {
                                            project.AlertErrorMessage('錯誤', data.Msg);
                                        }
                                        else {
                                            project.ShowMessage('訊息', String.format('節點：「{0}」上移完成.', nodeName));

                                            current.Initialize_TreeGrid();
                                            current.Initilaize_TreeNodeDLL();
                                        }
                                    }
                                    else {
                                        project.AlertErrorMessage('錯誤', '處理出現錯誤.');
                                    }
                                },
                                error: function () {
                                    project.AlertErrorMessage('錯誤', '出現錯誤');
                                }
                            });
                        }
                    });
                });
            },

            MoveDown: function() {
                //<summary>移動節點：下移</summary>

                $('.MoveDown').each(function (i, item) {
                    $(item).click(function () {

                        var nodeID = $(item).attr('nodeId');
                        var nodeName = $.trim($('[id^=datagrid-row][id$=' + nodeID + '] .tree-title').text());

                        if (nodeID.length > 0) {
                            $.ajax({
                                type: 'post',
                                url: project.ActionUrls.MoveDown,
                                data: { id: nodeID },
                                async: false,
                                cache: false,
                                dataType: 'json',
                                success: function (data) {
                                    if (data.Msg) {
                                        if (data.Msg != 'Success') {
                                            project.AlertErrorMessage('錯誤', data.Msg);
                                        }
                                        else {
                                            project.ShowMessage('訊息', String.format('節點：「{0}」下移完成.', nodeName));

                                            current.Initialize_TreeGrid();
                                            current.Initilaize_TreeNodeDLL();
                                        }
                                    }
                                    else {
                                        project.AlertErrorMessage('錯誤', '處理出現錯誤.');
                                    }
                                },
                                error: function () {
                                    project.AlertErrorMessage('錯誤', '出現錯誤');
                                }
                            });
                        }
                    });
                });
            }
        
        });
})
(project);