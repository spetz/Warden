var dashboard = (function() {

  var organizationId = "";
  var wardenName = "";
  var apiKey = "";

  var init = function (options) {
    organizationId = options.organizationId || "";
    wardenName = options.wardenName || "";
    apiKey = options.apiKey || "";
    var viewModel = new ViewModel();
    ko.applyBindings(viewModel);
  };

  function ViewModel() {
    var self = this;
    var allIterations = [];
    var currentWardenCheckResults = [];
    var mainChartContext = $("#main-chart")[0].getContext("2d");
    var watchersChartContext = $("#watchers-chart")[0].getContext("2d");
    var mainChart = null;
    var watchersChart = null;

    self.organizations = ko.observableArray([createEmptyOrganization()]);
    self.selectedOrganization = ko.observable();
    self.selectedWarden = ko.observable();
    self.selectedOrganization.subscribe(function (organization) {
    });
    self.selectedWarden.subscribe(function (warden) {
      $('select').material_select();
    });

    self.totalUptime = ko.observable(99);
    self.totalUptimeFormatted = ko.computed(function () {
      return self.totalUptime().toFixed(2) + "%";
    });
    self.totalDowntime = ko.observable(1);
    self.totalDowntimeFormatted = ko.computed(function () {
      return self.totalDowntime().toFixed(2) + "%";
    });
    self.validResources = ko.observable(5);
    self.invalidResources = ko.observable(7);
    self.totalResourcesFormatted = ko.computed(function () {
      return self.validResources() + "/" + self.invalidResources();
    });
    self.latestCheckAt = ko.observable("2016-04-15T10:36:51Z");
    self.latestCheckAtFormatted = ko.computed(function () {
      return self.latestCheckAt();
    });

    self.failingResources = ko.observableArray([new ResourceItem("RAM", 10), new ResourceItem("API", 7)]);
    self.mostFailingResources = ko.computed(function() {
      return self.failingResources().slice(0, 2);
    });

    self.iterations = ko.observableArray([]);
    var emptyWardenCheckResult = {
      completedAt: ko.observable("---"),
      watcherCheckResult: {
        watcherName: ko.observable("---"),
        watcherType: ko.observable("---"),
        description: ko.observable("---"),
        isValid: ko.observable("---")
      }
    };
    self.selectedWardenCheckResult = ko.observable(emptyWardenCheckResult);

    function setDefaultWarden() {
      self.organizations.remove(function (organization) {
        return organization.name() === "";
      });

      var selectedOrganization = self.organizations().filter(function (organization) {
        return organization.id() === organizationId;
      })[0];

      var selectedWarden = selectedOrganization.wardens().filter(function (warden) {
        return warden.name() === wardenName;
      })[0];
      self.selectedOrganization(selectedOrganization);
      self.selectedWarden(selectedWarden);
    };

    getOrganizations().then(function (organizations) {
      organizations.forEach(function(organization) {
        self.organizations.push(new Organization(organization));
      });
      setDefaultWarden();
      $('select').material_select();
    });

    getIterations()
      .then(function(iterations) {
        if (iterations.length === 0) {
          renderEmptyMainChart();
          renderEmptyWatchersChart();

          return;
        }

        self.iterations(iterations);
        allIterations.push(iterations);
        displayMainChart();
        renderWatchersChart(iterations[0]);
        setTimeout(updateMainChart, 5000);
      });

    function updateMainChart() {
      getIterations()
        .then(function(iterations) {

          var iteration = iterations[0];
          allIterations.push(iteration);
          var validResults = iteration.results.filter(function(result) {
            return result.isValid;
          });
          var point = validResults.length;
          var label = iteration.completedAt;
          mainChart.removeData();
          mainChart.addData([point], label);
          renderWatchersChart(iteration);
          setTimeout(updateMainChart, 5000);
        });
    };

    function renderEmptyMainChart() {
      var data = {
        labels: ["Missing data"],
        datasets: [
          {
            label: "Warden",
            fillColor: "rgba(75, 74, 73, 0.1)",
            strokeColor: "rgba(220,220,220,1)",
            pointColor: "rgba(220,220,220,1)",
            pointStrokeColor: "#fff",
            pointHighlightFill: "#fff",
            pointHighlightStroke: "rgba(220,220,220,1)",
            data: [0, 1]
          }
        ]
      };

      var options = {
        responsive: true
      };
      mainChart = new Chart(mainChartContext).Line(data, options);
    };

    function displayMainChart() {
      var labels = [];
      var points = [];
      var totalWatchers = 7;
      self.iterations()
        .forEach(function(iteration) {
          labels.push(iteration.completedAt);
          var validResults = iteration.results.filter(function(result) {
            return result.isValid;
          });
          points.push(validResults.length);
        });

      var options = {
        scaleOverride: true,
        scaleSteps: totalWatchers,
        scaleStepWidth: 1,
        scaleStartValue: 0,
        responsive: true
      };

      var data = {
        labels,
        datasets: [
          {
            label: "Warden",
            fillColor: "rgba(91, 187, 22, 0.2)",
            strokeColor: "rgba(220,220,220,1)",
            pointColor: "rgba(220,220,220,1)",
            pointStrokeColor: "#fff",
            pointHighlightFill: "#fff",
            pointHighlightStroke: "rgba(220,220,220,1)",
            data: points
          }
        ]
      };

      mainChart = new Chart(mainChartContext).Line(data, options);

      $("#main-chart")
        .click(function(evt) {
          var point = mainChart.getPointsAtEvent(evt)[0];
        });
    };

    function renderEmptyWatchersChart() {
      var data = [];
      data.push({
        value: 1,
        color: "rgba(75, 74, 73, 0.1)",
        highlight: "rgba(75, 74, 73, 0.2)",
        label: "Missing data"
      });

      var options = {
        responsive: true
      };
      watchersChart = new Chart(watchersChartContext).Pie(data, options);
    };

    function renderWatchersChart(iteration) {
      var invalidResults = iteration.results.filter(function(result) {
        return !result.isValid;
      });
      var validResults = iteration.results.filter(function(result) {
        return result.isValid;
      });
      var data = [];
      var labels = [];
      currentWardenCheckResults = [];
      iteration.results.forEach(function(result) {
        currentWardenCheckResults.push(result);
        labels.push(result.watcherCheckResult.watcherName);
      });
      invalidResults.forEach(function(result) {
        data.push({
          value: 1,
          color: "rgba(247, 70, 74, 0.5)",
          highlight: "rgba(247, 70, 74, 0.8)",
          label: result.watcherCheckResult.watcherName
        });
      });

      validResults.forEach(function(result) {
        data.push({
          value: 1,
          color: "rgba(91, 187, 22, 0.5)",
          highlight: "rgba(91, 187, 22, 0.8)",
          label: result.watcherCheckResult.watcherName
        });
      });

      var options = {
        responsive: true
      };

      watchersChart = new Chart(watchersChartContext).Pie(data, options);

      $("#watchers-chart")
        .click(function(evt) {
          var segment = watchersChart.getSegmentsAtEvent(evt)[0];
          var watcherName = segment.label;
          var wardenCheckResult = currentWardenCheckResults.filter(function(result) {
            return result.watcherCheckResult.watcherName === watcherName;
          })[0];
          self.selectedWardenCheckResult(wardenCheckResult);
        });
    };
  };

  function createEmptyOrganization() {
    return new Organization({
      id: "",
      name: "",
      wardens: []
    });
  }

  function Organization(organization) {
    var self = this;
    self.id = ko.observable(organization.id);
    self.name = ko.observable(organization.name);
    self.wardens = ko.observableArray([]);
    organization.wardens.forEach(function (warden) {
      self.wardens.push(new WardenItem(warden));
    });
  };

  function WardenItem(warden) {
    var self = this;
    self.id = ko.observable(warden.id);
    self.name = ko.observable(warden.name);
    self.enabled = ko.observable(warden.name);
  };

  function ResourceItem(name, percent) {
    var self = this;
    self.name = ko.observable(name);
    self.percent = ko.observable(percent);
    self.infoFormatted = ko.computed(function () {
      return self.name() + " (" + self.percent().toFixed(2) + "%" + ")";
    });
  };

  function getIterations() {
    return $.ajax({
      url: '/api/organizations/' + organizationId + '/wardens/' + wardenName + '/iterations',
      headers: {
        "X-Api-Key": apiKey
      },
      method: 'GET',
      data: { results: 10 },
      success: function(response) {
        return response;
      }
    });
  };

  function getOrganizations() {
    return $.ajax({
        url: '/api/organizations',
        headers: {
        "X-Api-Key": apiKey
      },
      method: 'GET',
      success: function(response) {
        return response;
      }
    });
  };

  return {
    init
  };
})();