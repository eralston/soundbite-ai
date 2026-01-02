using AutoMapper;
using Soundbite.Models;
using Soundbite.Services;

namespace Soundbite.Entity
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

            CreateMap<ClipEntity, Clip>().ReverseMap();
            CreateMap<ClipOperationEntity, ClipOperation>()
                .BeforeMap((s, d) => d.ClipRoute = s.Clip.Route)
                .ReverseMap();

            CreateMap<ParticipantEntity, Participant>().ReverseMap();
            CreateMap<ParticipantGroupEntity, ParticipantGroup>().ReverseMap();
            CreateMap<PromptEntity, Prompt>().ReverseMap();
            CreateMap<SeriesEntity, SeriesPreview>().ReverseMap();

            CreateMap<SessionEntity, Session>();
            CreateMap<SessionCommentEntity, SessionComment>();
            CreateMap<SessionEntity, SessionPreview>().ReverseMap();
            CreateMap<SessionEntity, SessionDetails>().ReverseMap();

            #endregion
        }
    }
}