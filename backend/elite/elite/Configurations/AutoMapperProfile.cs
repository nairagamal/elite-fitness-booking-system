using AutoMapper;
using elite.DTOs;
using elite.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace elite.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            
            AllowNullCollections = true;
            AllowNullDestinationValues = true;

            CreateMap<User, UserResponseDto>();
            CreateMap<Membership, MembershipDto>();
            CreateMap<Booking, BookingResponseDto>();
            CreateMap<Class, ClassDto>();
            CreateMap<Trainer, TrainerDto>();
        }
    }
}