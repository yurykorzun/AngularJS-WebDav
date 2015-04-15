(function () {
    var ModalFileController = function ($scope, $modalInstance, FileUploader, folderId) {
        $scope.folderId = folderId;

        $scope.uploader = new FileUploader({
            url: '/Document/UploadFile/' + $scope.folderId
        });

        $scope.uploadFile = function () {
            $scope.uploader.uploadAll();
            $modalInstance.close();
        };

        $scope.uploader.onCompleteAll = function () {
            
        };

        $scope.closeFileModal = function () {
            $modalInstance.dismiss();
        };
    };

    angular.module('documentsApp').controller('ModalFileController', ModalFileController);
}());