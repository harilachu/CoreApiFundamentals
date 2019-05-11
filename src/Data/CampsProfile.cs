using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampsProfile : Profile
    {
        public CampsProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ReverseMap()
                .ForMember(c => c.CampId, a => a.UseDestinationValue()); //Do not set identity value while mapping from Model to entity object

            this.CreateMap<Location, LocationModel>()
                .ReverseMap()
                .ForMember(c => c.LocationId, a => a.UseDestinationValue());

            this.CreateMap<Talk, TalkModel>().ReverseMap()
                .ForMember(t => t.TalkId, opt => opt.UseDestinationValue())
                .ForMember(t => t.Camp, opt => opt.Ignore()) //this will ignore while mapping Camp from Model to entity object
                .ForMember(t=> t.Speaker, opt=> opt.Ignore());
        }
    }
}

