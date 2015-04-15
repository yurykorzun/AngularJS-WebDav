(function () {
    var documentFactory = function ($http) {
        var serviceBase = '/api/ajax/',
            factory = {};

        factory.getRootFolder = function () {
            return $http.get(serviceBase + 'GetRootFolder/').then(function (response) {
                return response.data;
            });
        };

        factory.getFolders = function (parentFolderId) {
            return $http.get(serviceBase + 'GetFolderItems/' + parentFolderId).then(function (response) {
                return response.data;
            });
        };

        factory.getFolder = function (id) {
            return $http.get(serviceBase + 'GetFolder/' + id).then(function (response) {
                return response.data;
            });
        };

        factory.createFolder = function (folderId, folderName, callback) {
            return $http.post('/Document/CreateFolder/?id=' + folderId + '&folderName=' + folderName).then(callback);
        };

        factory.deleteFolder = function (itemId, isFolder, callback) {
            return $http.post('/Document/DeleteItem/?id=' + itemId + '&isFolder=' + isFolder).then(callback);
        };

        return factory;
    };

    angular.module('documentsApp').factory('documentService', documentFactory);
}());