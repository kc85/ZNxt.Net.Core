(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.addOauthclientCtrl', ['$scope', '$controller', '$window', '$location', '$rootScope', 'dataService', 'userData', 'menus', 'fileUploadService', 'loggerService',
    function ($scope, $controller, $window, $location, $rootScope, dataService, userData, menus, fileUploadService, logger) {
        $scope.client = {};
        $scope.client.allowed_scopes = [];
        $scope.showPassword = false;
        $scope.allRoles = [];
        $scope.close = function () {
            $scope.$emit("onHideOAuthClientAdd");
        }
        $scope.getAllUserRoles = function () {
            if ($scope.allRoles.length == 0) {
                dataService.get("./api/sso/user/roles").then(function (data) {
                    console.log(data);
                    if (data.data.code == 1) {
                        $scope.allRoles = data.data.data;
                    }
                    else {

                        logger.error("Error getting user roles");
                    }
                });
            }
        };
        $scope.createClient = function () {
            console.log($scope.client);
            dataService.post("./api/sso/oauthclient/add", $scope.client).then(function (data) {
                if (data.data.code == 1) {
                    $scope.client = data.data.data;
                    $scope.showPassword = true;
                    logger.debug("oauthclient Created");
                    $scope.frmAddClient.$setUntouched();
                    $scope.frmAddClient.$setPristine();
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
        $scope.addRole = function (r) {
            $scope.client.allowed_scopes.push(r.name);
        }
        $scope.removeRole = function (r) {
            var index = $scope.client.allowed_scopes.indexOf(r);
            $scope.client.allowed_scopes.splice(index, 1);    
        }
        $scope.$on("onShowAddClientPage", function (log) {
            $scope.client = {};
            $scope.client.allowed_scopes = []; $scope.showPassword = false;
            $scope.frmAddClient.$setUntouched();
            $scope.frmAddClient.$setPristine();
            $scope.getAllUserRoles();
        });
    }]);
})();