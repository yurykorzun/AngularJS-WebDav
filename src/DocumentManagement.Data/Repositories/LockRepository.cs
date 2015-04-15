using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DocumentManagement.Data.Context;
using DocumentManagement.Data.DataEntities;

namespace DocumentManagement.Data.Repositories
{
    public static class LockRepository
    {
        #region Public Methods

        public static Lock Get(int id)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockItem = lockDbSet.AsNoTracking().FirstOrDefault(x => x.Id == id);

                return lockItem;
            }
        }

        public static List<Lock> GetByFile(int fileId)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockItem = lockDbSet.AsNoTracking().Where(x => x.FileId == fileId).ToList();

                return lockItem;
            }
        }

        public static List<Lock> GetList(int fileId)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockList = lockDbSet.AsNoTracking().Where(x => x.FileId == fileId).ToList();

                return lockList;
            }
        }

        public static Lock Create(Lock newLockItem)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockExisting = lockDbSet.FirstOrDefault(x => x.Id == newLockItem.Id);

                if (lockExisting == null)
                {
                    context.Entry(newLockItem).State = EntityState.Added;

                    newLockItem.CreatedDate  = DateTime.Now;
                    newLockItem.UpdatedDate  = DateTime.Now;
                    newLockItem.VersionSeq   = 0;

                    lockDbSet.Add(newLockItem);

                    context.SaveChanges();

                    newLockItem.Errors = context.GetValidationErrors();
                }

                return newLockItem;
            }
        }

        public static Lock Update(Lock lockItem)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockExisting = lockDbSet.FirstOrDefault(x => x.Id == lockItem.Id);

                if (lockExisting != null)
                {
                    context.Entry(lockExisting).State = EntityState.Modified;

                    lockExisting.LockDepth     = lockItem.LockDepth;
                    lockExisting.LockOwner     = lockItem.LockOwner;
                    lockExisting.LockOwnerType = lockItem.LockOwnerType;
                    lockExisting.LockScope     = lockItem.LockScope;
                    lockExisting.LockType      = lockItem.LockType;
                    lockExisting.FileId        = lockItem.FileId;
                    lockExisting.ResType       = lockItem.ResType;
                    lockExisting.Timeout       = lockItem.Timeout;

                    lockExisting.UpdatedDate = DateTime.Now;
                    lockExisting.UpdatedDate = DateTime.Now;
                    lockExisting.VersionSeq++;

                    context.SaveChanges();

                    lockExisting.Errors = context.GetValidationErrors();
                }

                return lockExisting;
            }
        }

        public static Lock Delete(int id)
        {
            using (var context = new DataContext())
            {
                var lockDbSet = context.Lock;

                var lockExisting = lockDbSet.FirstOrDefault(x => x.Id == id);

                if (lockExisting != null)
                {
                    lockDbSet.Remove(lockExisting);

                    context.SaveChanges();

                    lockExisting.Errors = context.GetValidationErrors();
                }

                return lockExisting;
            }
        }
        

        #endregion


    }
}
