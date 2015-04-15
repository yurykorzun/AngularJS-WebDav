using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DocumentManagement.Data.Context;
using DocumentManagement.Data.DataEntities;

namespace DocumentManagement.Data.Repositories
{
    public static class LockTokenRepository
    {
        #region Public Methods

        public static LockToken Get(int id)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockToken = lockTokenDbSet.AsNoTracking().FirstOrDefault(x => x.Id == id);

                return lockToken;
            }
        }

        public static LockToken GetByToken(string token)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockToken = lockTokenDbSet.AsNoTracking().FirstOrDefault(x => x.Token == token);

                return lockToken;
            }
        }

        public static List<LockToken> GetList(int lockId)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockTokenList = lockTokenDbSet.AsNoTracking().Where(x => x.LockId == lockId).ToList();

                return lockTokenList;
            }
        }

        public static List<LockToken> GetList(string token)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockTokenList = lockTokenDbSet.AsNoTracking().Where(x => x.Token == token).ToList();

                return lockTokenList;
            }
        }

        public static LockToken Create(LockToken newLockToken)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockTokenExisting = lockTokenDbSet.FirstOrDefault(x => x.Id == newLockToken.Id);

                if (lockTokenExisting == null)
                {
                    context.Entry(newLockToken).State = EntityState.Added;

                    newLockToken.CreatedDate = DateTime.Now;
                    newLockToken.UpdatedDate = DateTime.Now;
                    newLockToken.VersionSeq  = 0;

                    lockTokenDbSet.Add(newLockToken);

                    context.SaveChanges();

                    newLockToken.Errors = context.GetValidationErrors();
                }

                return newLockToken;
            }
        }

        public static LockToken Update(LockToken newLockToken)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockTokenExisting = lockTokenDbSet.FirstOrDefault(x => x.Id == newLockToken.Id);

                if (lockTokenExisting != null)
                {
                    context.Entry(lockTokenExisting).State = EntityState.Modified;

                    newLockToken.CreatedDate = DateTime.Now;
                    newLockToken.UpdatedDate = DateTime.Now;
                    newLockToken.VersionSeq  = 0;

                    lockTokenDbSet.Add(newLockToken);

                    context.SaveChanges();

                    newLockToken.Errors = context.GetValidationErrors();
                }

                return newLockToken;
            }
        }

        public static LockToken Delete(int id)
        {
            using (var context = new DataContext())
            {
                var lockTokenDbSet = context.LockToken;

                var lockTokenExisting = lockTokenDbSet.FirstOrDefault(x => x.Id == id);

                if (lockTokenExisting != null)
                {
                    lockTokenDbSet.Remove(lockTokenExisting);

                    context.SaveChanges();

                    lockTokenExisting.Errors = context.GetValidationErrors();
                }

                return lockTokenExisting;
            }
        }
     

        #endregion
    }
}
