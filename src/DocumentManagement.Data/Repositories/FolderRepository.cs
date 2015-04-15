using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DocumentManagement.Data.Context;
using DocumentManagement.Data.DataEntities;

namespace DocumentManagement.Data.Repositories
{
    public static class FolderRepository
    {
        #region Public Methods

        public static Folder Get(int id)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folder = folderDbSet.AsNoTracking().FirstOrDefault(x => x.Id == id);

                return folder;
            }
        }

        public static List<Folder> GetByParentId(int? id)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folder = folderDbSet.AsNoTracking().Where(x => x.ParentFolderId.Value == id.Value);

                return folder.ToList();
            }
        }

        public static Folder Get(int? parentFolderId, string folderName)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folder = folderDbSet.AsNoTracking().FirstOrDefault(x => x.ParentFolderId == parentFolderId && x.FolderName == folderName);

                return folder;
            }
        }

        public static Folder Create(Folder newFolder)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folder = folderDbSet.FirstOrDefault(x => x.Id == newFolder.Id);

                if (folder == null)
                {
                    context.Entry(newFolder).State = EntityState.Added;

                    newFolder.CreatedDate = DateTime.Now;
                    newFolder.UpdatedDate = DateTime.Now;
                    newFolder.VersionSeq = 0;

                    folderDbSet.Add(newFolder);

                    context.SaveChanges();

                    newFolder.Errors = context.GetValidationErrors();
                }

                return newFolder;
            }
        }

        public static Folder Update(Folder folder)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folderExisting = folderDbSet.FirstOrDefault(x => x.Id == folder.Id);

                if (folderExisting != null)
                {
                    context.Entry(folderExisting).State = EntityState.Modified;

                    folderExisting.FolderName = folder.FolderName;
                    folderExisting.ParentFolderId = folder.ParentFolderId;
                    folderExisting.UpdatedDate = folder.UpdatedDate;
                    folderExisting.UpdatedDate = folder.UpdatedDate;
                    folderExisting.VersionSeq++;

                    context.SaveChanges();

                    folderExisting.Errors = context.GetValidationErrors();
                }

                return folderExisting;
            }
        }

        public static Folder Delete(int id)
        {
            using (var context = new DataContext())
            {
                var folderDbSet = context.Folder;

                var folderExisting = folderDbSet.FirstOrDefault(x => x.Id == id);

                if (folderExisting != null)
                {
                    folderDbSet.Remove(folderExisting);

                    context.SaveChanges();

                    folderExisting.Errors = context.GetValidationErrors();
                }

                return folderExisting;
            }
        }
        #endregion


    }
}
