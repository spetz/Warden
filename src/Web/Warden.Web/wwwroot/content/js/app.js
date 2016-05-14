'use strict';

var app = (function () {
    var loginPath = "";
    var logoutPath = "";
    var url = "";

    var init = function(options) {
        loginPath = options.loginPath || "/login";
        logoutPath = options.logoutPath || "/logout";
        url = options.url || "/";
        var path = "/" + url.split("/")[1];
        setMenu(path);
        initBlockableElements();
        initCheckboxes();
        initModalsAndRemoveClickHandlers();
        initCopyToClipboard();
        $('.tooltipped').tooltip({ delay: 50 });
        $(".button-collapse").sideNav();
    };

    function setMenu(path) {
        $("nav,#nav-mobile").find("li").removeClass("active");
        var $link = $("nav,#nav-mobile").find('a[href^="' + path + '"]');
        $link.parent().addClass("active");
        $link.parent().parent().parent().addClass("active");
    };

    function initBlockableElements() {
        $("[data-block]")
            .on("submit", function(e) {
                    $(this).validate();
                    var valid = $(this).valid();
                    if (!valid)
                        return;

                    var inputs = $(document).find("[type=submit]");
                    inputs.addClass("disabled");
                    inputs.attr("readonly", "readonly");
                    inputs.prop("disabled", true);
                });
    };

    function initCheckboxes() {
        $("input[type=checkbox]")
            .each(function() {
                var checked = $(this).data("checked") === "True";
                if (checked) {
                    $(this).prop("checked", true);
                    $(this).val(true);
                }
            });

        $("input[type=checkbox]")
            .click(function() {
                $(this).val($(this).is(":checked"));
            });
    };

    function initCopyToClipboard() {
        $("[data-clipboard]")
            .click(function() {
                copyToClipboard($(this).data("clipboard"));
            });

        function copyToClipboard(text) {
            window.prompt("Copy to clipboard: Ctrl+C, Enter", text);
        };
    };

    function initModalsAndRemoveClickHandlers() {
        $(".modal-trigger").leanModal();

        $("[data-remove-api-key]")
            .click(function() {
                $("#api-key-to-delete").val($(this).data("api-key"));
            });

        $("[data-remove-user-from-organization]")
            .click(function() {
                $("#user-id-to-delete").val($(this).data("user-id"));
            });

        $("[data-remove-warden-from-organization]")
            .click(function() {
                $("#warden-id-to-delete").val($(this).data("warden-id"));
            });
    };

    return {
        init: init
    };
})();