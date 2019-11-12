(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.notifier_view_emailCtrl', ['$scope', '$controller', '$location', '$rootScope', 'dataService', 'userData',
    function ($scope, $controller, $location, $rootScope, dataService, userData) {
        angular.extend(this, $controller(__ZNxtAppName + '.gridBaseCtrl', { $scope: $scope }));
        $scope.name = "Email Queue";
        $scope.logData = {};
        $scope.pageData = {};
        $scope.showDetails = false;
        $scope.email = {}
        $scope.active = function () {
            if ($scope.loadingData == false) {
                $scope.loadingData = true;
                dataService.get("./api/notifier/email/queue?pagesize=" + $scope.pageSize + "&currentpage=" + $scope.currentPage + "&filter=" + $scope.getFilter()).then(function (response) {
                    if (response.data.code == 1) {
                        $scope.currentPageShow = $scope.currentPage;
                        $scope.pageData = response.data;
                    }
                    $scope.loadingData = false;
                });
            }
        }

        $scope.showDetailsPage = function (email) {
            $scope.email = email;
            var iframe = document.getElementById('ifrm_email_preview');
            iframe.src = 'data:text/html,' + encodeURIComponent(email.body);
            $scope.showDetails = true;
        }
        $scope.closeDetails = function () {
            $scope.showDetails = false;
        };
        $scope.pageNumberChanged = function () {
            $scope.gotoPage($scope.currentPageShow);
        };
        $scope.active();
    }]);
})();