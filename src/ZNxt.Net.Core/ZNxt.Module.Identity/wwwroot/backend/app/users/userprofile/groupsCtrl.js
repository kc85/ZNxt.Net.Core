(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.userprofile.groupsCtrl', ['$scope', 'dataService',
        function ($scope, dataService) {
            $scope.allGroups = [];
            $scope.$on("onShowUserProfileItem", function (e, menu, user) {
                if (menu.key == "user_profile_groups") {
                    $scope.userData = angular.copy(user);
                    $scope.getAllUserGroups();
                };
                
            });

            $scope.getAllUserGroups = function () {
                if ($scope.allGroups.length == 0) {
                    dataService.get("./api/sso/user/groups").then(function (data) {
                        console.log(data);
                        if (data.data.code == 1) {
                            $scope.allGroups = data.data.data;
                        }
                        else {
                            
                            logger.error("Error getting user groups");
                        }
                    });
                }
            };
            $scope.removeGroup = function (groupremove) {

                if (confirm("Are you sure to remove group " + groupremove)) {
                    var request = { user_id: $scope.userData.user_id, group: groupremove };
                    dataService.post("./api/sso/user/removegroup", request).then(function (data) {
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
            $scope.addGroup = function (group) {

                if (confirm("Are you sure to add group " + group.name + ". " + group.description )) {
                    var request = { user_id: $scope.userData.user_id, group: group.key  };
                    dataService.post("./api/sso/user/addgroup", request).then(function (data) {
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
            $scope.save = function () {
                //var editProfileUrl = "./api/admin/userinfo/update";
                //if ($scope.$parent.isShowMyProfile == true) {
                //    editProfileUrl = "./api/userinfo/update";
                //}
                //dataService.post(editProfileUrl, $scope.userData).then(function (response) {
                //    if (response.data.code == 1) {
                //        $scope.$emit("onUserInfoUpdate", $scope.userData);
                //    }
                //});
            }
        }]);
})();