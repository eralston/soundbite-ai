using AutoMapper;
using Masticore.Models;
using Masticore.Token;

namespace Masticore.Entity
{
    /// <summary>
    /// AutoMapper profile for setting up object mapping configurations.  Profile is "picked up"
    /// by automapper during service configuration using the AddAutoMapper method.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Responsible for populating the mapping configuration with object mapping definitions.
        /// </summary>
        public MappingProfile()
        {
            #region Entity to Concrete Type Mappings

            // Entities have concrete model implementations and the following maps ensure that
            // data can be mapped between them in both directions.

            CreateMap<Group, GroupEntity>();
            CreateMap<GroupEntity, Group>();
            CreateMap<MemberEntity, Member>().ReverseMap();
            CreateMap<MemberEntity, Member>()
                .ForMember(i => i.Person, cfg => cfg.MapFrom(i => i.Person));
            CreateMap<OrganizationEntity, Organization>().ReverseMap();
            CreateMap<OrganizationEntity, OrganizationDetails>().ReverseMap();
            CreateMap<OrganizationEntity, OrganizationExtended>().ReverseMap();
            CreateMap<PersonEntity, Person>().ReverseMap();
            CreateMap<TenantEntity, Tenant>().ReverseMap();
            CreateMap<UserEntity, User>().ReverseMap();

            #endregion

            CreateMap<ITokenServiceSettings, TokenSettingsEntity>();
        }
    }
}
