(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.oauthclientCtrl', ['$scope', '$controller', '$location', '$rootScope', 'dataService', 'userData',
    function ($scope, $controller, $location, $rootScope, dataService, userData) {
        angular.extend(this, $controller(__ZNxtAppName + '.gridBaseCtrl', { $scope: $scope }));
        $scope.name = "OAuth Clients";
        $scope.pageData = {};
        $scope.filterIncludeColumns = ["id", "name"];
        $scope.showDetails = false;
        $scope.loadingData = false;
        $scope.showAddUser = false;
        $scope.active = function () {
            fetchClientInfo();
        }

        function fetchClientInfo() {
            if ($scope.loadingData == false) {
                $scope.loadingData = true;
                dataService.get("./api/sso/oauthclient?pagesize=" + $scope.pageSize + "&currentpage=" + $scope.currentPage + "&filter=" + $scope.getFilter()).then(function (response) {
                    if (response.data.code == 1) {
                        $scope.currentPageShow = $scope.currentPage;
                        $scope.pageData = response.data;
                        console.log($scope.pageData);
                    }
                    $scope.loadingData = false;
                });
            }
        }

        $scope.showAddClientPage = function (data) {
            $scope.showAddClient = true;
            $scope.$broadcast("onShowAddClientPage", data);
        }

        //$scope.$on("onHideUserDetails", function (log) {
        //    $scope.showDetails = false;
        //});
        $scope.$on("onHideOAuthClientAdd", function (log) {
            $scope.showAddClient = false;
        });
        $scope.pageNumberChanged = function () {
            $scope.gotoPage($scope.currentPageShow);
        };

        $scope.active();

        //$scope.$on("onUserInfoUpdate", function () {
        //    $scope.active();
        //});
        $scope.$on("onOAuthClientCreated", function () {
            $scope.active();
        });
    }]);
})();