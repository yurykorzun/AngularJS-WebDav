using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DocumentManagement.Data.DataEntities;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Service.Models;

namespace DocumentManagement.Service
{

    public class FolderService
    {
        #region Constructors
        public FolderService()
        {

        }
        #endregion

        #region Folder Management

        public static FolderModel GetRootFolder()
        {
            var folder = FolderRepository.GetByParentId(null).FirstOrDefault();

            var mappedFolder = AutoMapperService.Map<FolderModel>(folder);
            return mappedFolder;
        }


        public static FolderModel GetFolder(int id)
        {
            var folder = FolderRepository.Get(id);

            var mappedFolder = AutoMapperService.Map<FolderModel>(folder);
            return mappedFolder;
        }


        public static List<FolderModel> GetFolderByParentId(int? id)
        {
            var folders = FolderRepository.GetByParentId(id);

            var mappedFolders = folders.Select(AutoMapperService.Map<FolderModel>).ToList();
            return mappedFolders;
        }


        public static FolderModel GetFolder(int? parentId, string foderName)
        {
            var folder = FolderRepository.Get(parentId, foderName);

            var mappedFolder = AutoMapperService.Map<FolderModel>(folder);
            return mappedFolder;
        }

        public static FolderModel SaveFolder(FolderModel folder)
        {
            var mappedFolder = AutoMapperService.Map<Folder>(folder);
            return folder.Id == 0 ?
                AutoMapperService.Map<FolderModel>(FolderRepository.Create(mappedFolder)) : 
                AutoMapperService.Map<FolderModel>(FolderRepository.Update(mappedFolder));
        }

        public static FolderModel DeleteFolder(int id)
        {
            var mappedFolder = AutoMapperService.Map<FolderModel>(FolderRepository.Delete(id));
            return mappedFolder;
        }

        #endregion

    }
}