
var config = {
    layuiBase: '/assets/layuiadmin/',
    layuiIndex: 'lib/index',
    layuiExtends: {
        authtree: 'lib/extend/authtree',
        ztree: 'lib/extend/ztree/ztree',
        droptree: 'lib/extend/droptree',
		formSelects: '../lib/extend/formselects/formSelects-v4'
    },
    layuiModules: ['index', 'table', 'layer', 'form', 'element', 'laydate','tree','upload','colorpicker'],
    //页面layui加载完后调用
    ready: function () {
        console.log("onready not implemented");
    },
    //table加载完后调用
    onTableDone: function () {
        console.log("onTableDone not implemented");
    },
    //检索后调用
    reloadTable: function () {
        console.log("reloadTable not implemented");
    },
    refresh: function () {
        console.log("refresh not implemented");
    },
    showSearchForm: function (moduleKey) {
        //展示检索窗体
        var url = "/ModuleData/Search?moduleKey=" + moduleKey;
        layer.open({
            type: 2,
            title: L('检索'),
            closeBtn: 1, 
            shade: 0.1,
            shadeClose: true,
            area: ['600px', '100%'],
            offset: 'l', //左侧弹出
            anim: 3,
            content: [url], //iframe的url，no代表不显示滚动条
            end: function () { //此处用于演示

            }
        });
    },
    showRelativeModuleForm: function (option) {
        var moduleKey = option.moduleKey,
            columnKey = option.columnKey,
            maxReferenceNumber = option.maxReferenceNumber || 1;
        //展示关联引用窗体
        var url = "/ModuleData/RelativeSelect?moduleKey=" + moduleKey + "&columnKey=" + columnKey + "&maxReferenceNumber"+maxReferenceNumber;
        window.referenceLayerIndex=layer.open({
            type: 2,
            title: L('关联查询'),
            closeBtn: 1,
            shade: 0.1,
            shadeClose: true,
            area: ['80%', '100%'],
            offset: 'r', //右侧弹出
            anim: 0,
            content: [url], //iframe的url，no代表不显示滚动条
            end: function () { //此处用于演示

            }
        });
    }
};


$(function () {
    
    //全局事件
    //tip事件
    $("body").on("mouseenter", "*[tips]", function () {
        var e = $(this);
        if (!e.attr("tips")) { return;}
        var i = e.attr("tips"),
            t = e.attr("lay-offset"),
            n = e.attr("lay-direction"),
            s = layer.tips(i, this, {
                tips: n || 1,
                time: -1,
                success: function (e, a) {
                    t && e.css("margin-left", t + "px")
                }
            });
        e.data("index", s)
    }).on("mouseleave", "*[tips]", function () {
        layer.close($(this).data("index"))
        });
    $("body").on("mouseenter", "*[formtips]", function () {
        var e = $(this);
        if (!e.attr("formtips")) { return; }
        var i = e.attr("formtips"),
            t = e.attr("lay-offset"),
            n = e.attr("lay-direction"),
            s = layer.tips(i, this, {
                tips: [n || 1, "#fffdce"],
                time: -1,
                success: function (e, a) {
                    t && e.css("margin-left", t + "px");
                    e.css("width", "auto");
                    e.css("max-width", "600px");
                    console.log(e);
                    e.find(".layui-layer-content").css("color", "#4c4c4c");
                }
            });
        e.data("index", s);
    }).on("mouseleave", "*[formtips]", function () {
        layer.close($(this).data("index"));
    });
    //布局初始化,如div自适应
    func.initUI();

    //图片缩略放大事件 2018/5/24 13:55 lijianbo
    $("body").on('click',"img.thumb", function () {
        var img = $(this);
        var fileid = img.attr("FileID");
        top.layer.open({
            title: '图片显示'
            , skin: 'picturesshow'
            , area: ['80%', '80%']
            , content: '<img style=\'width:100%,height:100%\' src=\'/File/Thumb?fileid=' + fileid + '\' >'
        });  
    });
    
    //清空table的检索缓存
    $("table[module]").each(function () {
        var moduleKey = $(this).attr("module");
        layui.sessionData(moduleKey, null);
    })
})


var func = {
    
    //获取模块的js名
    getModuleServiceName: function (name) {
        return name?(name[0].toLowerCase() + name.substring(1)):"moduleData";
    },
    //模块按钮事件
    callModuleButtonEvent: function (element) {
        var ev = getEvent();
        var ele = element || $(ev.srcElement || ev.target),
            moduleKey = ele.attr("modulekey"),
            layevent = ele.attr("lay-event"),
            dataid = ele.attr("dataid"),
            confirmmsg = ele.attr("confirmmsg"),
            buttonname = ele.attr("buttonname"),
            buttonactiontype = ele.attr("buttonactiontype"),
            buttonactionparam = ele.attr("params"),
            buttonactionurl = ele.attr("buttonactionurl"),
            callback = ele.attr("callback"),
            opentop=ele.attr("opentop"),            
            fornonerow = ele.attr("fornonerow");            
        
        //提交的数据
        var data = [];
        if (dataid) { data.push(dataid); }
        else if (!fornonerow) {
            data = layui.table.checkStatus(moduleKey).data.map(function (o) { return o.id; });
            if (data.length === 0) {
                abp.message.info("请先选择记录");
                return false;
            }
        }
        var url = buttonactionurl + (buttonactionurl.indexOf("?") > 0 ? "&" : "?") + "modulekey=" + moduleKey + "&data=" + data.join(','); //url
        
        var funcProxyWrapper;//方法包装
        //异步提交方式
        if (buttonactiontype === "Ajax") {
            var funcProxy = eval(buttonactionurl);
            if (!funcProxy) {
                layer.alert("未找到代理方法" + buttonactionurl);
                return false;
            }

            funcProxyWrapper = function () {
                abp.ui.setBusy(
                    $('body'),
                    funcProxy(data, {
                        success: function () {
                            //abp.message.success("提交成功");
                            parent.layer.msg("提交成功");
                            callback && eval(callback)();
                            moduleKey && func.reload(moduleKey);
                        }
                    })

                );
            }

        }
        //展示窗体
        else if (buttonactiontype === "Form") {
            var defaultOption = {
                type: 2,
                title: buttonname,
                shadeClose: false,
                shade: 0.8,
                area: ['80%', '80%'],
                content: url,
                btn: ['提交', '关闭'],
                yes: function (index, layero) {
                    var iframeWin = window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    if (iframeWin.submit) { iframeWin.submit(); return false;}
                },
                btn2: function (index, layero) {
                    var iframeWin = window[layero.find('iframe')[0]['name']];
                    if (iframeWin.submit2) { iframeWin.submit2(); return false; }
                },
                btn3: function (index, layero) {
                    var iframeWin = window[layero.find('iframe')[0]['name']];
                    if (iframeWin.submit3) { iframeWin.submit3(); return false;}
                }
            };
            var param = buttonactionparam ? $.parseJSON(buttonactionparam) : {};

            funcProxyWrapper = function () {
                if (opentop) {
                    param.closeBtn = 0;
                    param.success = function (layero, index) {
                        //全屏弹窗
                        console.log(layero);
                        $(layero).append("<button class='layui-btn layui-btn-sm layui-btn-danger closeBtn' style='position: absolute;top: 8px;right: 15px; width: 80px;'>返回</button>").find(".closeBtn").click(function () {
                            top.layer.close(index);
                        });

                    };
                }
                (opentop?top.layer:layer).open($.extend(defaultOption, param));
            }
        }
        //打开Tab
        else if (buttonactiontype === "Tab") {
            funcProxyWrapper = function () {
                top.layui.index.openTabsPage(url, buttonname);
            }
        }

        //提交
        if (confirmmsg) {
            abp.message.confirm(confirmmsg, funcProxyWrapper);
        } else {
            funcProxyWrapper();
        }
    },
    //表格重载
    reload: function (tableid,option) {
        layui.table.reload(tableid,option);
    },
    //异步执行
    runAsync: function (fun) {
        top.abp.ui.setBusy(
            null,
            fun
        );
    },
    //表单初始化
    initForm: function () {
        $("div.layui-inline").each(function () {
            var parentNode = $(this).parent();
            var prevNode = $(this).prev();
            if (!parentNode.is(".layui-form-item")) {
                if (prevNode.is(".layui-form-item")) {
                    $(this).appendTo(prevNode);
                } else {
                    $(this).wrap('<div class="layui-form-item"></div>');
                }
            }
        })
    },
    initUI: function ($body) {
        //元素自适应高度
        func.initLayout($body);
    },
    //初始化元素自适应
    initLayout: function ($body) {
        var $target = $body || $("body");
        //<div layouth='130'></div>
        $target.find("[layouth]").each(function () {
            var layouth = $(this).attr("layouth");
            var h = top.$(".layui-body").height();
            //var h = $(document).height();
            h = h - parseInt(layouth);
            $(this).css("overflow-y","auto");
            $(this).css("height",  h + "px");
        })
    },
    //构建查询条件
    buildSearchCondition: function (moduleKey) {
        var conditions = layui.sessionData(moduleKey).conditions;
        if (!conditions || conditions=="") {
            return "";
        } else {
            //var conditionStr = "";
            //for (var i = 0; i < conditions.length; i++) {
            //    var condition = conditions[i];
            //    conditionStr += condition.leftBracket +' '+ condition.column.columnKey + ' ' + condition.operator + ' ' + condition.value + ' ' + condition.rightBracket + ' ' + condition.joiner+' ';
            //}
            return JSON.stringify(conditions);
        }
    },
    //查找返回,子页面调用
    bringBack: function (moduleKey,isReturn) {
        var checkStatus = layui.table.checkStatus(moduleKey);
        if (checkStatus.data.length == 0) {
            layer.msg(L('请至少选择一项'), { icon: 5, anim: 6 });
            return false;
        }
        var key = $.getUrlParam("key");
        parent.func.getBringBack(checkStatus.data, key, isReturn);
    },
    //获取返回数据,父页面调用
    getBringBack: function (data, key, isReturn) {
        //调用页面中定义的回调函数
        if (func.bringBackFuncs[key](data)) {
            //成功回调
            if (isReturn) {
                layer.closeAll('iframe');
            }
        }
        
        console.log(data);
    },
    bringBackFuncs: [],
    referenceDatas:[]
};
/*多语种*/
abp.localization.defaultSourceName = "Master";
function L(name) {
    return abp.localization.localize(name);
}

function getEvent() { //同时兼容ie和ff的写法 
    if (document.all) return window.event;
    var func = getEvent.caller;
    while (func !== null) {
        var arg0 = func.arguments[0];
        if (arg0) {
            if ((arg0.constructor === Event || arg0.constructor === MouseEvent)
                || (typeof (arg0) === "object" && arg0.preventDefault && arg0.stopPropagation)) {
                return arg0;
            }
        }
        func = func.caller;
    }
    return null;
} 

/*jquery扩展*/
//获取url的参数值
    $.getUrlParam = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r !== null) return unescape(r[2]); return null;
    }

//把name/value的数组转为obj对象
$.arrayToObj = function (array) {
    var result = {};
    for (var i = 0; i < array.length; i++) {
        var field = array[i];
        if (field.name in result) {
            result[field.name] += ',' + field.value;
        } else {
            result[field.name] = field.value;
        }
    }
    return result;
}
$.newid = function () {
    return new Date().getTime() +''+ Math.round(Math.random()*1000);
}
function getCheckboxValue(name) {
    var data = [];
    $(":checked[name='" + name + "']").each(function () {
        data.push($(this).val());
    })
    return data;
}
abp.ajax.defaultError = { message: "网络错误!", details: "网络连接不通畅，无法提供服务。请关闭窗口稍后重试。" }

function checkToken() {
    var token = $.cookie("token");
    var func = function () {
        if (token != $.cookie("token")) {
            top.layer.alert("当前账号已退出", function () {
                top.location.href = "/Account/Login";
            });
            window.clearInterval(checkTokenInterval);
        }
        abp.services.app.user.getCurrentToken({
            error: function () {
                abp.message.error("网络连接不通畅，无法提供服务。请关闭窗口稍后重试。");
                window.clearInterval(checkTokenInterval);
            }
        }).done(function (data) {
            if (data != token) {
                top.layer.alert("当前账号已退出", function () {
                    top.location.href = "/Account/Login";
                });
                window.clearInterval(checkTokenInterval);
            }
            window.setTimeout(func, 1000);
        });
    };
    func();
    //var checkTokenInterval = window.setInterval(, 1000);
}
// 对Date的扩展，将 Date 转化为指定格式的String
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
// 例子： 
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
Date.prototype.Format = function (fmt) { //author: meizz 
	var o = {
		"M+": this.getMonth() + 1, //月份 
		"d+": this.getDate(), //日 
		"h+": this.getHours(), //小时 
		"m+": this.getMinutes(), //分 
		"s+": this.getSeconds(), //秒 
		"q+": Math.floor((this.getMonth() + 3) / 3), //季度 
		"S": this.getMilliseconds() //毫秒 
	};
	if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
	for (var k in o)
		if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
	return fmt;
}