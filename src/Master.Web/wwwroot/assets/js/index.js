
var index = {
    //加载对应菜单组
    showMenu: function (menuName) {
        $("[menu-name]").hide();
        $("[menu-name='" + menuName + "']").fadeIn();
    }
};

function initIndex() {
    //锁屏
    var lockPage = function () {
        layer.prompt({ title: '请输入密码进行解锁', formType: 1, closeBtn: 0, btn: ['确定'] }, function (pass, index) {
            //todo:锁屏密码验证
            layer.close(index);
            window.sessionStorage.setItem("lockcms", false);
        });
    }
    // 判断是否显示锁屏
    if (window.sessionStorage.getItem("lockcms") == "true") {
        lockPage();
    }

    $("[lay-href='']").removeAttr("lay-href");
    //加载第一个菜单组
    $("li.menuitem:first a").click();

    // 点击空白处关闭右键弹窗
    $(document).click(function () {
        $('.rightmenu').hide();
    })

    //自定义事件
    $("body").on("click", "*[custom-event]", function () {
        var othis = $(this)
            , attrEvent = othis.attr('custom-event');
        customevents[attrEvent] && customevents[attrEvent].call(this, othis);
    })

    customevents = {
        lock: function (othis) {
            window.sessionStorage.setItem("lockcms", true);
            lockPage();
        }
    }

    /**
    * 注册tab右键菜单点击事件
    */
    $("body").on('contextmenu', '.layui-tab-title li', function (e) {
        var popupmenu = $(".rightmenu");
        l = ($(document).width() - e.clientX) < popupmenu.width() ? (e.clientX - popupmenu.width()) : e.clientX;
        t = ($(document).height() - e.clientY) < popupmenu.height() ? (e.clientY - popupmenu.height()) : e.clientY;

        //判断是否是首页
        var currentActiveTabID = $("li[class='layui-this']").attr('lay-id');// 获取当前激活的选项卡ID
        if (currentActiveTabID != "/Home") {
            popupmenu.css({ left: l, top: t }).show();
        }
        return false;
    });
    $(".rightmenu li").click(function () {
        var currentActiveTabID = $("li[class='layui-this']").attr('lay-id');// 获取当前激活的选项卡ID
        var tabtitle = $(".layui-tab-title li");
        var allTabIDArr = [];
        $.each(tabtitle, function (i) {
            //去除主页
            if ($(this).attr("lay-id") != "/Home") {
                allTabIDArr[i] = $(this).attr("lay-id");
            }
        })

        switch ($(this).attr("data-type")) {
            case "closethis"://关闭当前，如果开始了tab可关闭，实际意义不大
                tabDelete(currentActiveTabID);
                break;
            case "closeall"://关闭所有
                tabDeleteAll(allTabIDArr);
                break;
            case "closeothers"://关闭非当前
                $.each(allTabIDArr, function (i) {
                    var tmpTabID = allTabIDArr[i];
                    if (currentActiveTabID != tmpTabID)
                        tabDelete(tmpTabID);
                })
                break;
            case "closeleft"://关闭左侧全部
                var index = allTabIDArr.indexOf(currentActiveTabID);
                tabDeleteAll(allTabIDArr.slice(0, index));
                break;
            case "closeright"://关闭右侧全部
                var index = allTabIDArr.indexOf(currentActiveTabID);
                tabDeleteAll(allTabIDArr.slice(index + 1));
                break;
            default:
                $('.rightmenu').hide();

        }
        $('.rightmenu').hide();
    });

    tabDelete = function (id) {
        console.log("删除的TabID：" + id)
        layui.element.tabDelete("layadmin-layout-tabs", id);//删除
    }
    tabDeleteAll = function (ids) {
        $.each(ids, function (i, item) {
            layui.element.tabDelete("layadmin-layout-tabs", item);
        })
    }
}

$(function () {
    
})
