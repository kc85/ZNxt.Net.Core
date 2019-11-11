(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.notifier_test_emailCtrl', ['$scope', '$location', '$rootScope', 'dataService', 'userData', 'loggerService','$window',
    function ($scope, $location, $rootScope, dataService, userData, logger, $window) {
        $scope.name = "Email Test";
        $scope.email = { Type : 'Email'};

        $scope.send = function () {
            dataService.post("./api/notifier/send", $scope.email).then(function (data) {
                if (data.data.code == 1) {
                    $scope.fromNotifierEmailTest.$setUntouched();
                    $scope.fromNotifierEmailTest.$setPristine();
                    logger.debug("Email Send");
                }
                else {
                    logger.error("Error while sending email");
                }
            });
        };
    }]);
})();