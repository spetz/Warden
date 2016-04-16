var dashboard = (function() {

  var init = function() {
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

    getIterations().then(function(iterations) {
      self.iterations(iterations);
      allIterations.push(iterations);
      displayMainChart();
      renderWatchersChart(iterations[0]);
      setTimeout(updateMainChart, 5000);
    });

    function updateMainChart() {
      getIterations().then(function (iterations) {

        var iteration = iterations[0];
        allIterations.push(iteration);
        var validResults = iteration.results.filter(function (result) {
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

    function displayMainChart() {
      var labels = [];
      var points = [];
      var totalWatchers = 7;
      self.iterations().forEach(function (iteration) {
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

      $("#main-chart").click(function(evt) {
        var point = mainChart.getPointsAtEvent(evt)[0];
        console.log(point);
      });
    };

    function renderWatchersChart(iteration) {
      var invalidResults = iteration.results.filter(function (result) {
        return !result.isValid;
      });
      var validResults = iteration.results.filter(function (result) {
        return result.isValid;
      });
      var data = [];
      var labels = [];
      currentWardenCheckResults = [];
      iteration.results.forEach(function (result) {
        currentWardenCheckResults.push(result);
        labels.push(result.watcherCheckResult.watcherName);
      });
      invalidResults.forEach(function (result) {
        data.push({
          value: 1,
          color: "rgba(247, 70, 74, 0.5)",
          highlight: "rgba(247, 70, 74, 0.8)",
          label: result.watcherCheckResult.watcherName
        });
      });

      validResults.forEach(function (result) {
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

      $("#watchers-chart").click(function (evt) {
        var segment = watchersChart.getSegmentsAtEvent(evt)[0];
        var watcherName = segment.label;
        var wardenCheckResult = currentWardenCheckResults.filter(function(result) {
          return result.watcherCheckResult.watcherName === watcherName;
        })[0];
        self.selectedWardenCheckResult(wardenCheckResult);
      });
    };
  };

  function getIterations() {
    return $.get("/api/data/iterations", { results: 10 }, function (response) {
      return response;
    });
  };

  return {
    init
  };
})();