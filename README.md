# AngularJS-WebDav
Rough prototype that provides webdav access for MS Word documents via ASP.Net MVC app

Brief preview - https://github.com/yurykorzun/AngularJS-WebDav/blob/master/documents.mp4

Based on the post written in 2005 by Hoju Saram - http://thehojusaram.blogspot.com/2007/06/c-webdav-server-with-sql-backend-source.html

Completely rewrote the original VS 2005 solution and wrapped it in WebApi and ASP.Net MVC application.

In order to run the project you need:

1. Create a database using \AngularJS-WebDav\src\DocumentManagement.WebDav\Scripts\webdav.sql
2. Configure connection strings in \AngularJS-WebDav\src\DocumentManagement.Web\Web.config and \AngularJS-WebDav\src\DocumentManagement.Web\Web.config
3. Configure an IIS website and enable WebDav support in it
4. Deploy WebDav project to the IIS website


