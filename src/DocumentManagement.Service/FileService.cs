using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DocumentManagement.Data.DataEntities;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Service.Models;

namespace DocumentManagement.Service
{
    public class FileService
    {

        #region Constructors
        public FileService()
        {

        }
        #endregion
        
        #region File Storage Management

        public static FileModel GetFile(int id)
        {
            var file = FileRepository.Get(id);
            var mappedFile = AutoMapperService.Map<FileModel>(file);
            return mappedFile;
        }

        public static List<FileModel> GetFile(int parentId, string fileName)
        {
            var files = FileRepository.Get(parentId, fileName);
            
            var mappedFiles = files.Select(AutoMapperService.Map<FileModel>).ToList();
            return mappedFiles;
        }

        public static List<FileModel> GetFileByParentFolder(int parentId)
        {
            var files = FileRepository.GetByParentFolder(parentId);

            var mappedFiles = files.Select(AutoMapperService.Map<FileModel>).ToList();
            return mappedFiles;
        }

        public static FileModel SaveFile(FileModel file)
        {
            var mappedFile = AutoMapperService.Map<File>(file);
            return file.Id == 0 ?
                AutoMapperService.Map<FileModel>(FileRepository.Create(mappedFile)) : 
                AutoMapperService.Map<FileModel>(FileRepository.Update(mappedFile));
        }

        public static FileModel DeleteFile(int id)
        {
            var mappedFile = AutoMapperService.Map<FileModel>(FileRepository.Delete(id));
            return mappedFile;
        }

        #endregion

    }
}
