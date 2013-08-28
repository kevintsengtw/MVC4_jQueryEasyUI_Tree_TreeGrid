;
(function (window) {
    //===========================================================================================
    if (typeof (jQuery) === 'undefined') { alert('jQuery Library NotFound.'); return; }

    var project = window.project =
    {
        AppName: 'project',
        ActionUrls: {}
    };
    //===========================================================================================
    jQuery.extend(project, {

        Initialize: function () {
            /// <summary>
            /// 初始化函式
            /// </summary>

        },
        
        ShowMessage: function (title, message) {
            /// <summary>
            /// jQuery EasyUI Messager 訊息 Show
            /// </summary>
            /// <param name="title"></param>
            /// <param name="message"></param>

            $.messager.show({
                title: title,
                msg: message,
                timeout: 1000,
                showType: 'show',
                style:
                {
                    right: 0,
                    left: '',
                    top: document.body.scrollTop + document.documentElement.scrollTop,
                    bottom: ''
                }
            });
        },

        AlertErrorMessage: function (title, message) {
            /// <summary>
            /// jQuery EasyUI Messager 錯誤訊息 Alert
            /// </summary>
            /// <param name="title"></param>
            /// <param name="message"></param>

            $.messager.alert(title, message, 'error');
        }
        
    });
})
(window);