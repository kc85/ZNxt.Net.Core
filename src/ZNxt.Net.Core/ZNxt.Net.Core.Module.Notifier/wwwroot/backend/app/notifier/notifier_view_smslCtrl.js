(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.notifier_view_smsCtrl', ['$scope', '$controller', '$location', '$rootScope', 'dataService', 'userData',
    function ($scope, $controller, $location, $rootScope, dataService, userData) {
        angular.extend(this, $controller(__ZNxtAppName + '.gridBaseCtrl', { $scope: $scope }));
        $scope.name = "Sms Queue";
        $scope.logData = {};
        $scope.pageData = {};
        $scope.showDetails = false;
        $scope.active = function () {
            if ($scope.loadingData == false) {
                $scope.loadingData = true;
                dataService.get("./api/notifier/sms/queue?pagesize=" + $scope.pageSize + "&currentpage=" + $scope.currentPage + "&filter=" + $scope.getFilter()).then(function (response) {
                    if (response.data.code == 1) {
                        $scope.currentPageShow = $scope.currentPage;
                        $scope.pageData = response.data;
                    }
                    $scope.loadingData = false;
                });
            }
        }

        $scope.showDetailsPage = function (log) {
            $scope.showDetails = true;
            $scope.$broadcast("onShowSMSQueueDetails", log);
        }
        $scope.$on("onHideSMSQueueDetails", function (log) {
            $scope.showDetails = false;
        });
        $scope.pageNumberChanged = function () {
            $scope.gotoPage($scope.currentPageShow);
        };
        $scope.active();
    }]);
})();