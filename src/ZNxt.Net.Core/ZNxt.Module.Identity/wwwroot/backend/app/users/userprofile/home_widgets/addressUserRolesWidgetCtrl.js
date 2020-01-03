﻿(function () {
    var ZApp = angular.module(__ZNxtAppName);
    ZApp.controller(__ZNxtAppName + '.userprofile.home.addressWidgetCtrl', ['$scope', '$rootScope', 'dataService',
    function ($scope, $rootScope, dataService) {
        $scope.addressCount = 0;
        $scope.$on("onShowUserProfileItem", function (e, menu, user) {
            if (menu.key == "info") {
                $scope.userData = user;
                $scope.addressCount = 0;
                if ($scope.userData.user_info != undefined && $scope.userData.user_info[0].addresses != undefined) {
                    $scope.addressCount = $scope.userData.user_info[0].addresses.length;
                }
            }
        });
        $scope.showAddress = function () {
            $scope.$parent.clickMenu({ key: 'user_profile_address' });
        }
    }]);
})();

(function () {
    var ZApp = angular.module(__ZNxtAppName);
    ZApp.controller(__ZNxtAppName + '.userprofile.home.userRoleWidgetCtrl', ['$scope', '$rootScope', 'dataService',
    function ($scope, $rootScope, dataService) {
        $scope.$on("onShowUserProfileItem", function (e, menu, user) {
            if (menu.key == "info") {
                $scope.userData = user;
            }
        });
        $scope.showRoles = function () {
            $scope.$parent.clickMenu({ key: 'user_profile_roles' });
        }
    }]);
})();