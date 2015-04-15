(function () {
    var FolderController = function ($scope, $routeParams, $window, $modal, $interval, documentService, uiGridConstants) {
        $scope.folderId = ($routeParams.folderId) ? parseInt($routeParams.folderId) : undefined;
        
        $scope.deleteGridItem = function (id, isFolder) {
            documentService.deleteFolder(id, isFolder, function (response) { $interval($scope.reloadFolderGrid, 1000, 1); });
        };

        $scope.gridOptions = {
            enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
            enableSorting: false,
            data: [],
            columnDefs: [
                {
                    name: 'Icon', displayName: '', field: 'IsFolder', width: 60, cellTemplate: '<div style="text-align:center;"><div ng-switch on="row.entity.IsFolder">' +
                                                                        '<i ng-switch-when="true" class="fa fa-folder-o fa-2x"></i>' +
                                                                        '<i ng-switch-when="false" class="fa fa-file-word-o fa-2x"></i>' +
                                                                        '</div></div>'
                },
                {
                    name: 'Name', field: 'Name', cellTemplate: '<div ng-switch on="row.entity.IsFolder">' +
                                                                '<a ng-switch-when="true" ng-href="{{ row.entity.FolderUrl }}/#/folder/{{ row.entity.Id }}" >{{ row.entity.Name }}</a>' +
                                                                '<a ng-switch-when="false" href="ms-word:ofe|u|{{ row.entity.FolderUrl }}/{{ row.entity.Name }}">{{ row.entity.Name }}</a>' +
                                                            '</div>'
                },
                {
                    name: 'Delete', displayName: '', field: 'Id', width: 40, clickCallback: $scope.deleteGridItem, cellTemplate: '<div ng-show="!row.entity.IsFolder" style="text-align:center;cursor:pointer;" ng-click="col.colDef.clickCallback(row.entity.Id, row.entity.IsFolder)"><i class="fa fa-trash fa-2x"></i></div>'
                },
                {
                     name: 'Updated Date', field: 'UpdatedDate', width: 120, cellFilter: 'date:\'yyyy-MM-dd\''
                }
            ]
        };

        $scope.reloadFolderGrid = function () {
            if ($scope.folderId) {
                documentService.getFolders($scope.folderId).then(setFolderData);
            } else {
                documentService.getRootFolder().then(setFolderData);
            }
        };

        $scope.reloadFolderGrid();


        function setFolderData(folderData) {
            $scope.gridOptions.data = folderData.ChildItems;
            $scope.folderName       = folderData.Name;
            $scope.parentFolderId   = folderData.ParentFolderId;
            $scope.folderId = folderData.Id;
        };

        //file modal
        $scope.showFileModal = function () {
            $scope.fileUploadModal = $modal.open({
                templateUrl: 'uploadFileModel',
                controller: 'ModalFileController',
                resolve: {
                    folderId: function() {
                        return $scope.folderId;
                    }
                }
            });

            $scope.fileUploadModal.result.then(function() {
                $interval($scope.reloadFolderGrid, 1000, 1);
            }, function() {
                
            });
        };

        //folder modal
        $scope.showFolderModal = function () {
            $scope.folderCreateModal = $modal.open({
                templateUrl: 'createFolderModel',
                controller: 'ModalFolderController',
                resolve: {
                    newFolderName: function () {
                        return $scope.newFolderName;
                    }
                }
            });

            $scope.folderCreateModal.result.then(function (folderName) {
                documentService.createFolder($scope.folderId, folderName, function (response) { $interval($scope.reloadFolderGrid, 1000, 1); });
            }, function () {

            });
        };
    };

    angular.module('documentsApp').controller('FolderController', FolderController);
})();
