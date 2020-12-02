

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
            $scope.selectImageFrom = function (img) {
                $scope.selectedImage = img;

            }
            $scope.onSelectImage = function (img) {
                gallery_selected_image = img;
                $rootScope.$broadcast('onGallerySelectImageChanged', gallery_selected_image);

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
            $scope.uploadUserImage = function () {

                var imageFrom = undefined;
                if ($scope.addIndexImage != undefined) {
                    imageFrom = $scope.addIndexImage;
                }
                else if ($scope.addImage != undefined) {
                    imageFrom = $scope.addImage;
                }
                if (imageFrom != undefined) {
                    console.log($scope.addImage);
                    $("#btnImageGalleryUpload").hide();
                    $("#btnImageGalleryUploadCancel").hide();
                    $("#btnImageGalleryUpload_index").hide();
                    $("#btnImageGalleryUploadCancel_index").hide();
                    var uploadImageUrl = "./api/gallery/image/upload";
                    if (imageFrom.type.indexOf("image") != -1) {
                        fileUploadService.uploadFileToUrl(imageFrom, uploadImageUrl, undefined, function (response) {
                            $scope.cancelUploadUserImage();
                            if (response.data.code == 1) {
                                $scope.active();
                                $('#galleryImageAddIndexPreview').attr('src', '/images/add_image.png');
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
                if ($scope.galleryImagefrm != undefined) {
                    $scope.galleryImagefrm.$setPristine()
                }
                if ($scope.galleryImageIndexFrm != undefined) {
                    $scope.galleryImageIndexFrm.$setPristine()
                }
                $('#galleryImageAddIndexPreview').attr('src', '/images/add_image.png');
                $('#galleryImageAddPreview').attr('src', '/images/add_image.png');
                $("#btnImageGalleryUpload").hide();
                $("#btnImageGalleryUploadCancel").hide();
                $("#btnImageGalleryUpload_index").hide();
                $("#btnImageGalleryUploadCancel_index").hide();
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

function readURLOnAddImageChangeIndex(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#galleryImageAddIndexPreview')
                .attr('src', e.target.result);
            $("#btnImageGalleryUpload_index").show();
            $("#btnImageGalleryUploadCancel_index").show();
        };
        reader.readAsDataURL(input.files[0]);
    }
}
