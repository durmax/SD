using AutoMapper;
using SD.Shared;
using System.Collections.Generic;

namespace sd.Api.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<WordModel, WordDto>()
                .ForMember(dto => dto.CommentsCount, exp => exp.MapFrom(w => w.Comments.Count))
                .ForMember(dto => dto.LikesCount, exp => exp.MapFrom(w => w.Likes.Count));

            CreateMap<WordDto, WordModel>();
        }
    }
}
