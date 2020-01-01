(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.userprofile.generalCtrl', ['$scope', 'dataService',
    function ($scope, dataService) {
        $scope.$on("onShowUserProfileItem", function (e, menu, u) {
            if (menu.key == "user_profile_general") {
                var user = {};
                angular.copy(u, user);
                if (user.user_info != undefined && user.user_info.length != 0) {
                    user.user_info = user.user_info[0];
                }
                $scope.userData = {};
                angular.copy(user, $scope.userData);
                console.log($scope.userData);
            }
        });
        $scope.save = function () {
            var editProfileUrl = "./api/admin/userinfo/update";
            if ($scope.$parent.isShowMyProfile == true) {
                editProfileUrl = "./api/userinfo/update";
            }
            dataService.post(editProfileUrl, $scope.userData).then(function (response) {
                if (response.data.code == 1) {
                    $scope.$emit("onUserInfoUpdate", $scope.userData);
                }
            });
        }
    }]);
})();