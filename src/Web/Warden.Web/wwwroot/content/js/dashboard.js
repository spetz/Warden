var dashboard = (function() {

    var organizationId = "";
    var wardenName = "";
    var apiKey = "";
    var viewModel = null;

    var init = function(options) {
        organizationId = options.organizationId || "";
        wardenName = options.wardenName || "";
        apiKey = options.apiKey || "";

        viewModel = new ViewModel();
        ko.applyBindings(viewModel);
        initWardenHub();
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
        self.selectedOrganization.subscribe(function(organization) {
        });
        self.selectedWarden.subscribe(function(warden) {
            $('select').material_select();
        });

        self.totalUptime = ko.observable(0);
        self.totalUptimeFormatted = ko.computed(function() {
            return self.totalUptime().toFixed(2) + "%";
        });
        self.totalDowntime = ko.observable(0);
        self.totalDowntimeFormatted = ko.computed(function() {
            return self.totalDowntime().toFixed(2) + "%";
        });
        self.validResources = ko.observable(0);
        self.invalidResources = ko.observable(0);
        self.totalResourcesFormatted = ko.computed(function() {
            return self.validResources() + " of " + self.invalidResources();
        });
        self.latestCheckAt = ko.observable("---");
        self.latestCheckAtFormatted = ko.computed(function() {
            return self.latestCheckAt();
        });

        self.failingResources = ko.observableArray([]);
        self.mostFailingResources = ko.computed(function() {
            return self.failingResources.sort(function (left, right) { return left.totalDowntime() < right.totalDowntime() })().slice(0, 3);
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

        self.setIterationDetails = function (iteration) {
            allIterations.push(iteration);
            self.latestCheckAt(iteration.completedAt);
            updateResourcesInfo(iteration);
            updateMainChart(iteration);
        };

        function setStats(stats) {
            self.totalUptime(stats.totalUptime);
            self.totalDowntime(stats.totalDowntime);
            stats.watchers.forEach(function(watcher) {
                var watcherStats = new WatcherItem(watcher);
                self.failingResources.push(watcherStats);
            });
        };

        self.changeWarden = function() {
            var selectedOrganizationId = self.selectedOrganization().id();
            var selectedWardenId = self.selectedWarden().id();
            window.location = "/dashboards/organizations/" + selectedOrganizationId + "/wardens/" + selectedWardenId;
        };

        function updateResourcesInfo(iteration) {
            var validResults = iteration.results.filter(function (result) {
                return result.isValid;
            });

            self.validResources(validResults.length);
            self.invalidResources(iteration.results.length);
        }

        function setDefaultWarden() {
            self.organizations.remove(function(organization) {
                return organization.name() === "";
            });

            var selectedOrganization = self.organizations()
                .filter(function(organization) {
                    return organization.id() === organizationId;
                })[0];

            var selectedWarden = selectedOrganization.wardens()
                .filter(function(warden) {
                    return warden.name() === wardenName;
                })[0];
            self.selectedOrganization(selectedOrganization);
            self.selectedWarden(selectedWarden);
        };

        getStats()
            .then(function (stats) {
                setStats(stats);
            });

        getOrganizations()
            .then(function(organizations) {
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

                var latestIteration = iterations[0];
                self.iterations(iterations);
                allIterations.push(iterations);
                displayMainChart();
                renderWatchersChart(latestIteration);
                self.setIterationDetails(latestIteration);
            });

        function updateMainChart(iteration) {
              var validResults = iteration.results.filter(function(result) {
                return result.isValid;
              });
              var point = validResults.length;
              var label = iteration.completedAt;
              mainChart.removeData();
              mainChart.addData([point], label);
            renderWatchersChart(iteration);
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
        organization.wardens.forEach(function(warden) {
            self.wardens.push(new WardenItem(warden));
        });
    };

    function WardenItem(warden) {
        var self = this;
        self.id = ko.observable(warden.id);
        self.name = ko.observable(warden.name);
        self.enabled = ko.observable(warden.name);
    };

    function WatcherItem(watcher) {
        var self = this;
        self.name = ko.observable(watcher.name);
        self.type = ko.observable(watcher.type);
        self.totalDowntime = ko.observable(watcher.totalDowntime);
        self.totalUptime = ko.observable(watcher.totalUptime);
        self.infoFormatted = ko.computed(function() {
            return self.name() + " (" + self.totalDowntime().toFixed(2) + "%" + ")";
        });
    };

    function getStats() {
        var endpoint = organizationId + '/wardens/' + wardenName + '/stats';
        return getDataFromApi(endpoint);
    };

    function getIterations() {
        var endpoint = organizationId + '/wardens/' + wardenName + '/iterations';

        return getDataFromApi(endpoint, { results: 10 });
    };

    function getOrganizations() {
        return getDataFromApi();
    };

    function getDataFromApi(endpoint, params) {
        return $.ajax({
            url: '/api/organizations/' + (endpoint || ""),
            headers: {
                "X-Api-Key": apiKey
            },
            method: 'GET',
            data: params,
            success: function(response) {
                return response;
            }
        });
    };

    function initWardenHub() {
        $.connection.hub.qs = { organizationId, wardenName };
        var chat = $.connection.wardenHub;
        chat.client.iterationCreated = function (iteration) {
            iteration = toCamelCase(iteration);
            viewModel.setIterationDetails(iteration);
        };

        $.connection.hub.start()
            .done(function() {
            });
    };

    ////SignalR camelCase JSON resolver does not seem to be working - temporary workaround.
    function toCamelCase(o) {
        var build, key, destKey, value;

        if (o instanceof Array) {
            build = [];
            for (key in o) {
                value = o[key];

                if (typeof value === "object") {
                    value = toCamelCase(value);
                }
                build.push(value);
            }
        } else {
            build = {};
            for (key in o) {
                if (o.hasOwnProperty(key)) {
                    destKey = (key.charAt(0).toLowerCase() + key.slice(1) || key).toString();
                    value = o[key];
                    if (value !== null && typeof value === "object") {
                        value = toCamelCase(value);
                    }

                    build[destKey] = value;
                }
            }
        }
        return build;
    }


    return {
        init
    };
})();