(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.notifierCtrl', ['$scope', '$location', '$rootScope', 'dataService', 'userData', 'loggerService','$window',
    function ($scope, $location, $rootScope, dataService, userData, logger, $window) {
        $scope.name = "Notifier";
        $scope.notifier = {};

        $scope.active = function () {
            dataService.get("./api/notifier/config").then(function (data) {
                if (data.data.code == 1) {
                    $scope.notifier = data.data.data;
                }
                else {
                    logger.error("Error while fetching config");
                }
            });
        };
        $scope.save = function () {
            dataService.post("./api/notifier/config/save", $scope.notifier ).then(function (data) {
                if (data.data.code == 1) {
                    logger.debug("Config saved");
                }
                else {
                    logger.error("Error while saving config");
                }
            });
        };
        $scope.active();
    }]);
})();