(function () {
    var app = angular.module('documentsApp', ['ngRoute', 'ui.grid', 'angularFileUpload', 'ui.bootstrap']);

    app.config(['$routeProvider', '$compileProvider', function ($routeProvider, $compileProvider) {
        $routeProvider
        .when('/', {
            controller: 'HomeController',
            templateUrl: 'Partial/Home'
        })
        .when('/home', {
            controller: 'HomeController',
            templateUrl: 'Partial/Home'
        })
        .when('/folder/:folderId?', {
             controller: 'FolderController',
             templateUrl: 'Partial/Folder'
        });

        $compileProvider.aHrefSanitizationWhitelist(/^\s*(http|ms-word):/);
    }]);

}());