(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.schedulerCtrl', ['$scope', '$controller', '$location', '$rootScope', 'dataService', 'userData',
    function ($scope, $controller, $location, $rootScope, dataService, userData) {
        angular.extend(this, $controller(__ZNxtAppName + '.gridBaseCtrl', { $scope: $scope }));
        $scope.name = "Scheduler";
    }]);
})();