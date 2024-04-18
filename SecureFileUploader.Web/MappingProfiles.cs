using AutoMapper;

namespace SecureFileUploader.Web;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Models.User, Services.Models.User>().ReverseMap();

        CreateMap<Data.Entities.File, Services.Models.FileBase>().ReverseMap();
        CreateMap<Data.Entities.File, Services.Models.File>().ReverseMap();
        CreateMap<Services.Models.File, Models.File>().ReverseMap();
        CreateMap<Services.Models.FileBase, Models.File>().ReverseMap();
        CreateMap<IFormFile, Services.Models.File>();
    }
}