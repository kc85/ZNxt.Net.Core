(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.addUserCtrl', ['$scope', '$controller', '$window', '$location', '$rootScope', 'dataService', 'userData', 'menus', 'fileUploadService', 'loggerService',
    function ($scope, $controller, $window, $location, $rootScope, dataService, userData, menus, fileUploadService, logger) {
        $scope.user = {};
        $scope.close = function () {
            $scope.$emit("onHideAddUser");
        }
        $scope.createUserName = function () {
            var username = $scope.user.first_name.trim().toLocaleLowerCase() + "_" + $scope.user.last_name.trim().toLocaleLowerCase();
            $scope.user.user_name = username;
        }
        $scope.createUser = function () {
            console.log($scope.user);
            var dob = new Date($scope.user.dateofbirth);
            $scope.user.dob = {};
            $scope.user.dob.day = dob.getDate();
            $scope.user.dob.month = dob.getMonth();
            $scope.user.dob.year = dob.getFullYear();

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