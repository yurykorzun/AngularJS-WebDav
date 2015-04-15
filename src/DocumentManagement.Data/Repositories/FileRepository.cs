using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DocumentManagement.Data.Context;
using DocumentManagement.Data.DataEntities;

namespace DocumentManagement.Data.Repositories
{
    public static class FileRepository
    {
        #region Public Methods

        public static File Get(int id)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;
                var file = fileDbSet.AsNoTracking().FirstOrDefault(x => x.Id == id);

                return file;
            }
        }

        public static List<File> GetByParentFolder(int parentId)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;
                var fileList = fileDbSet.AsNoTracking().Where(x => x.ParentFolderId == parentId).ToList();

                return fileList;
            }
        }

        public static List<File> Get(int parentId, string fileName)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;

                var fileList = fileDbSet.AsNoTracking().Where(x => x.ParentFolderId == parentId && x.FileName == fileName);
                var list = fileList.ToList();

                return  list;
            }
        }

        public static File Create(File newFile)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;

                var file = fileDbSet.FirstOrDefault(x => x.Id == newFile.Id);

                if (file == null)
                {
                    context.Entry(newFile).State = EntityState.Added;

                    newFile.CreatedDate = DateTime.Now;
                    newFile.UpdatedDate = DateTime.Now;
                    newFile.VersionSeq = 0;

                    fileDbSet.Add(newFile);

                    context.SaveChanges();
                }

                newFile.Errors = context.GetValidationErrors();
            }

            return newFile;
        }

        public static File Update(File file)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;

                var fileExisting = fileDbSet.FirstOrDefault(x => x.Id == file.Id);

                if (fileExisting != null)
                {
                    context.Entry(fileExisting).State = EntityState.Modified;

                    fileExisting.ContentType = file.ContentType;
                    fileExisting.FileData = file.FileData;
                    fileExisting.FileDataSize = file.FileDataSize;
                    fileExisting.FileName = file.FileName;
                    fileExisting.ParentFolderId = file.ParentFolderId;
                    fileExisting.UpdatedDate = file.UpdatedDate;
                    fileExisting.UpdatedDate = file.UpdatedDate;
                    fileExisting.VersionSeq++;

                    context.SaveChanges();

                    fileExisting.Errors = context.GetValidationErrors();
                }

                return fileExisting;
            }
        }

        public static File Delete(int id)
        {
            using (var context = new DataContext())
            {
                var fileDbSet = context.File;

                var file = fileDbSet.FirstOrDefault(x => x.Id == id);

                if (file != null)
                {
                    fileDbSet.Remove(file);

                    context.SaveChanges();

                    file.Errors = context.GetValidationErrors();
                }

                return file;
            }
        }
       

         #endregion


    }
}