using AutoMapper;
using DocumentManagement.Data.DataEntities;
using DocumentManagement.Service.Models;

namespace DocumentManagement.Service
{
    public static class AutoMapperService
    {
        /// <summary>
        /// Setup Automapper mappings.
        /// This only needs to be called once.
        /// </summary>
        public static void Setup()
        {
            Mapper.CreateMap<File, FileModel>().ReverseMap();
            Mapper.CreateMap<Folder, FolderModel>().ReverseMap();
            Mapper.CreateMap<Lock, LockModel>().ReverseMap();
            Mapper.CreateMap<LockToken, LockTokenModel>().ReverseMap();        
        }

        public static T Map<T>(object source) 
        {
            var destination = Mapper.Map<T>(source);
            return destination;
        }

    }
}
