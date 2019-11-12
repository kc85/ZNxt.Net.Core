(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.template_engineDetailsCtrl', ['$scope', '$controller', '$location', '$rootScope', '$window', 'dataService', 'userData',
    function ($scope, $controller, $location, $rootScope, $window, dataService, userData) {
        $scope.template = {};
        var scrollX = 0;
        var scrollY;
       
        $scope.closeDetails = function () {
            $scope.$emit("onHideTemplateDetails", $scope.template);
            $window.scrollTo(scrollX, scrollY);
        }

        $scope.$on("onShowTemplateDetails", function (e, template) {
            $scope.template = template;
            var iframe = document.getElementById('ifrm_template_preview');
            iframe.src = 'data:text/html,' + encodeURIComponent(template.data);
            scrollY = $window.scrollY;
            scrollX = $window.scrollX;
            $window.scrollTo(0, 0);
        });
    }]);
})();