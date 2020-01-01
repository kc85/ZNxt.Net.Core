(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.userprofile.parentsCtrl', ['$scope', 'dataService',
        function ($scope, dataService) {
            $scope.allRoles = [];
            $scope.$on("onShowUserProfileItem", function (e, menu, user) {
                if (menu.key == "user_profile_parents") {
                    $scope.userData = angular.copy(user);
                    $scope.userData.parents = [];
                    getParents($scope.userData);
                };
            });

            function getParents(user) {
                dataService.get("./api/user/parents?user_id=" + user.user_id).then(function (response) {
                    if (response.data.code == 1) {
                        user.parents = response.data.data;
                        user.parents.forEach(function (d) {
                            if (d.user != undefined) {
                                for (var ui in d.user[0]) {
                                    d[ui] = d.user[0][ui];
                                }
                            }
                            getUserInfo(d);
                        });
                    }
                });
            }
            $scope.getRelationValue = function (val) {
                return val;
            }
            function getUserInfo(user) {
                dataService.get("./api/sso/userinfo?user_id=" + user.user_id).then(function (response) {
                    if (response.data.code == 1) {
                        var user_info = response.data.data.user_info[0];
                        for (var d in user_info) {
                            user[d] = user_info[d];
                        }
                    }
                });
            };
        }]);
})();