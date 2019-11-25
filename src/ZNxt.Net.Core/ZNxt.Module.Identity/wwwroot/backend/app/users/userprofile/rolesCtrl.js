(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.userprofile.rolesCtrl', ['$scope', 'dataService',
        function ($scope, dataService) {
            $scope.allRoles = [];
            $scope.$on("onShowUserProfileItem", function (e, menu, user) {
                if (menu.key == "user_profile_groups") {
                    $scope.userData = angular.copy(user);
                    $scope.getAllUserRoles();
                };
                
            });

            $scope.getAllUserRoles= function () {
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
            $scope.removeRole = function (roleremove) {

                if (confirm("Are you sure to remove role " + roleremove)) {
                    var request = { user_id: $scope.userData.user_id, role: roleremove };
                    dataService.post("./api/sso/user/removerole", request).then(function (data) {
                        console.log(data);
                        if (data.data.code == 1) {
                            $scope.userData = data.data.data;
                            $scope.$emit("onUserInfoUpdate", $scope.userData);
                        }
                        else {

                            logger.error("Error getting user role");
                        }
                    });
                }
            };
            $scope.addRole = function (addrole) {

                if (confirm("Are you sure to add role " + addrole.name + ". " + addrole.description )) {
                    var request = { user_id: $scope.userData.user_id, role: addrole.key  };
                    dataService.post("./api/sso/user/addrole", request).then(function (data) {
                        console.log(data);
                        if (data.data.code == 1) {
                            $scope.userData = data.data.data;
                            $scope.$emit("onUserInfoUpdate", $scope.userData);
                        }
                        else {

                            logger.error("Error getting user groups");
                        }
                    });
                }
            };
            
        }]);
})();