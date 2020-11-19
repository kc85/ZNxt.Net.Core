(function () {
    var ZApp = angular.module(__ZNxtAppName);

    ZApp.controller(__ZNxtAppName + '.galleryCtrl', ['$scope', '$controller', '$location', '$rootScope', 'dataService', 'userData', 'fileUploadService', 'loggerService',
        function ($scope, $controller, $location, $rootScope, dataService, userData, fileUploadService, logger) {
        angular.extend(this, $controller(__ZNxtAppName + '.gridBaseCtrl', { $scope: $scope }));
        $scope.name = "Image Gallery";
        $scope.logData = {};
        $scope.pageData = {};
        $scope.showDetails = false;
        $scope.active = function () {
            if ($scope.loadingData == false) {
                $scope.loadingData = true;
                dataService.get("./api/gallery/images?pagesize=" + $scope.pageSize + "&currentpage=" + $scope.currentPage + "&filter=" + $scope.getFilter()).then(function (response) {
                    if (response.data.code == 1) {
                        $scope.currentPageShow = $scope.currentPage;
                        $scope.pageData = response.data;
                    }
                    $scope.loadingData = false;
                });
            }
        }

        //$scope.showDetailsPage = function (log) {
        //    $scope.showDetails = true;
        //    $scope.$broadcast("onShowLogViewDetails", log);
        //}
        //$scope.$on("onHideViewDetails", function (log) {
        //    $scope.showDetails = false;
        //});
        $scope.pageNumberChanged = function () {
            $scope.gotoPage($scope.currentPageShow);
        };
        //$scope.$watch('addImage', function () {
        //    uploadUserImage();
        //});
        $scope.uploadUserImage= function() {
            if ($scope.addImage != undefined) {
                console.log($scope.addImage);
                $("#btnImageGalleryUpload").hide();
                $("#btnImageGalleryUploadCancel").hide();
                var uploadImageUrl = "./api/gallery/image/upload";
                if ($scope.addImage.type.indexOf("image") != -1) {
                    fileUploadService.uploadFileToUrl($scope.addImage, uploadImageUrl, undefined, function (response) {
                        $scope.cancelUploadUserImage();
                        if (response.data.code == 1) {
                            $scope.active();
                           
                            $('#galleryImageAddPreview').attr('src', '/images/add_image.png');
                        }
                    }, function () {
                            $scope.cancelUploadUserImage();
                        logger.error("Something worng in server");
                    });
                }
                else {
                    logger.error("Invalid file selection. Please select image file");
                }
            }
        }
            $scope.active();
            $scope.cancelUploadUserImage = function () {
                $scope.galleryImagefrm.$setPristine()
                $('#galleryImageAddPreview').attr('src', '/images/add_image.png');
                $("#btnImageGalleryUpload").hide();
                $("#btnImageGalleryUploadCancel").hide();
            }
        }]);


    
  
})();

function readURLOnAddImageChange(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#galleryImageAddPreview')
                .attr('src', e.target.result);
            $("#btnImageGalleryUpload").show();
            $("#btnImageGalleryUploadCancel").show();
        };

        reader.readAsDataURL(input.files[0]);
    }
}
