(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.addOauthclientCtrl', ['$scope', '$controller', '$window', '$location', '$rootScope', 'dataService', 'userData', 'menus', 'fileUploadService', 'loggerService',
    function ($scope, $controller, $window, $location, $rootScope, dataService, userData, menus, fileUploadService, logger) {
        $scope.client = {};
        $scope.showPassword = false;
        $scope.close = function () {
            $scope.$emit("onHideOAuthClientAdd");
        }
     
        $scope.createClient = function () {
            console.log($scope.client);
            dataService.post("./api/sso/oauthclient/add", $scope.client).then(function (data) {
                if (data.data.code == 1) {
                    $scope.client = data.data.data;
                    $scope.showPassword = true;
                    logger.debug("oauthclient Created");
                    $scope.frmAddUser.$setUntouched();
                    $scope.frmAddUser.$setPristine();
                    $scope.$emit("onOAuthClientCreated");
                }
                else {
                    var error = "";
                    for (var msg in data.data.data) {
                        error += data.data.data[msg] + ".";
                    }
                    logger.error(error);
                }
            });
        }
        $scope.$on("onShowAddClientPage", function (log) {
            $scope.client = {};
            $scope.showPassword = false;
            $scope.frmAddUser.$setUntouched();
            $scope.frmAddUser.$setPristine();
        });
    }]);
})();