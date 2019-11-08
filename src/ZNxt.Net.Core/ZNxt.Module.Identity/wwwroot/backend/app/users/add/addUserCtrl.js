(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.addUserCtrl', ['$scope', '$controller', '$window', '$location', '$rootScope', 'dataService', 'userData', 'menus', 'fileUploadService', 'loggerService',
    function ($scope, $controller, $window, $location, $rootScope, dataService, userData, menus, fileUploadService, logger) {
        $scope.user = {};
        $scope.close = function () {
            $scope.$emit("onHideAddUser");
        }
        $scope.createUser = function () {
            console.log($scope.user);
            dataService.post("./api/sso/admin/adduser", $scope.user).then(function (data) {
                if (data.data.code == 1) {
                    logger.debug("User Created");
                    $scope.frmAddUser.$setUntouched();
                    $scope.frmAddUser.$setPristine();
                    $scope.$emit("onUserCreated");
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
    }]);
})();