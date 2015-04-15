(function () {
    var ModalFolderController = function ($scope, $modalInstance) {
        $scope.createFolder = function () {
            $modalInstance.close($scope.newFolderName);
        };

        $scope.closeFolderModal = function () {
            $modalInstance.dismiss();
        };
    };

    angular.module('documentsApp').controller('ModalFolderController', ModalFolderController);
}());