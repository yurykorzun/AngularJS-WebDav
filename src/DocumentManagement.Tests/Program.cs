using DocumentManagement.Data.Repositories;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;

namespace DocumentManagement.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = FileService.GetFile(1);
            file.FileName = "test1.docx";
            file = FileService.SaveFile(file);

            var newLockItem = new LockModel
            {
                ResType = 0,
                LockDepth = 2,
                LockOwner = @"Yury.Korzun",
                LockOwnerType = 1,
                LockScope = 2,
                LockType = 1,
                FileId = 1,
                Timeout = 3600,
                CreatedUser = @"Yury.Korzun",
                UpdatedUser = @"Yury.Korzun"
            };

            var newLock = LockService.SaveLock(newLockItem);
        }
    }
}
