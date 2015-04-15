using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DocumentManagement.Data.DataEntities;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Service.Models;

namespace DocumentManagement.Service
{
    public class LockService
    {
        public LockService()
        {
        }

        public static LockModel GetLock(int id)
        {
            var lockItem = LockRepository.Get(id);

            var mappedLock = AutoMapperService.Map<LockModel>(lockItem);
            return mappedLock;
        }

        public static LockModel GetLockByFile(int fileId)
        {
            var lockItem = LockRepository.GetByFile(fileId).FirstOrDefault();

            var mappedLock = AutoMapperService.Map<LockModel>(lockItem);
            return mappedLock;
        }

        public static List<LockModel> GetActiveLocksByFile(int fileId)
        {
            var lockItems = LockRepository.GetByFile(fileId).Where(x => DateTime.Now.Subtract(x.UpdatedDate).TotalSeconds > x.Timeout);

            var mappedLocks = lockItems.Select(AutoMapperService.Map<LockModel>).ToList();
            return mappedLocks;
        }

        public static LockTokenModel GetLockToken(string token)
        {
            var lockToken = LockTokenRepository.GetByToken(token);

            var mappedLockToken = AutoMapperService.Map<LockTokenModel>(lockToken);
            return mappedLockToken;
        }

        public static List<LockTokenModel> GetLockTokens(int lockId)
        {
            var lockTokens = LockTokenRepository.GetList(lockId);

            var mappedLockTokens = lockTokens.Select(AutoMapperService.Map<LockTokenModel>).ToList();
            return mappedLockTokens;
        }

        public static LockModel GetLockByToken(string token)
        {
            var lockToken = LockTokenRepository.GetList(token).FirstOrDefault();

            if (lockToken == null)
            {
                return null;
            }

            var lockItem = LockRepository.Get(lockToken.LockId);

            var mappedLock = AutoMapperService.Map<LockModel>(lockItem);
            return mappedLock;
        }

        public static LockModel DeleteLock(int id)
        {
            var lockItem = AutoMapperService.Map<LockModel>(LockRepository.Delete(id));
            return lockItem;
        }

        public static LockTokenModel DeleteLockToken(int id)
        {
            var lockToken = AutoMapperService.Map<LockTokenModel>(LockTokenRepository.Delete(id));
            return lockToken;
        }

        public static LockModel SaveLock(LockModel lockItem)
        {
            var mappedLock = AutoMapperService.Map<Lock>(lockItem);
            var savedLock = lockItem.Id == 0 ? LockRepository.Create(mappedLock) : LockRepository.Update(mappedLock);

            lockItem = AutoMapperService.Map<LockModel>(savedLock);

            return lockItem;
        }

        public static LockTokenModel SaveLockToken(LockTokenModel lockToken)
        {
            var mappedLockToken = AutoMapperService.Map<LockToken>(lockToken);
            var savedLockToken = lockToken.Id == 0 ? LockTokenRepository.Create(mappedLockToken) : LockTokenRepository.Update(mappedLockToken);

            lockToken = AutoMapperService.Map<LockTokenModel>(savedLockToken);

            return lockToken;
        }

    }
}
