var app = (function() {
    var loginPath = "";
    var logoutPath = "";

    var init = function(options) {
        loginPath = options.loginPath || "/login";
        logoutPath = options.logoutPath || "/logout";
        initNav();
        initBlockableElements();
        initCheckboxes();
        initModalsAndRemoveClickHandlers();
    };

    function initNav() {
        $("[data-logout]")
            .click(function() {
                $.ajax({
                    url: logoutPath,
                    type: 'DELETE',
                    success: function(result) {
                        window.location = loginPath;
                    }
                });
            });
    };

    function initBlockableElements() {
        $("[data-block]")
            .on("submit",
                function(e) {
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
        init
    };
})();