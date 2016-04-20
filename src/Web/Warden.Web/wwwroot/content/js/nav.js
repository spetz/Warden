var nav = (function() {

  var init = function(options) {
    var loginPath = options.loginPath || "/login";
    var logoutPath = options.logoutPath || "/logout";

    $("[data-logout]").click(function () {
        $.ajax({
          url: logoutPath,
          type: 'DELETE',
          success: function(result) {
            window.location = loginPath;
          }
        });
      });
  };

  return {
    init
  };
})();