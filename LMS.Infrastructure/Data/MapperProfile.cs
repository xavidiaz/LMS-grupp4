using AutoMapper;
using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Infrastructure.Paging;
using LMS.Shared.DTOs.ActionType;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.DocumentDtos;
using LMS.Shared.DTOs.FeedbackDtos;
using LMS.Shared.DTOs.Module;
using LMS.Shared.DTOs.SubmissionDtos;
using LMS.Shared.DTOs.UserDtos;
using LMS.Shared.Enum;
using LMS.Shared.Paging;

namespace LMS.Infrastructure.Data
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // ApplicationUser->UserDto
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            // ApplicationUser->CourseParticipantDto
            CreateMap<ApplicationUser, CourseParticipantDto>();

            // UserDto -> ApplicationUser
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Course,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpireTime,
                    opt => opt.Ignore());

            // CreateUserDto -> ApplicationUser
            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName,
                    opt => opt.Ignore());

            // UpdateUserDto -> ApplicationUser
            CreateMap<UpdateUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Course,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken,
                    opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpireTime,
                    opt => opt.Ignore());

            CreateMap<Module, ModuleDto>();
            CreateMap<Module, ModuleListDto>();

            CreateMap<CreateModuleDto, Module>();
            CreateMap<UpdateModuleDto, Module>();

            CreateMap<PagedList<Module>, PagedResponse<ModuleDto>>()
                .ForCtorParam("Items", opt =>
                    opt.MapFrom(src => src.Items))
                .ForCtorParam("PageNumber", opt =>
                    opt.MapFrom(src => 1))
                .ForCtorParam("PageSize", opt =>
                    opt.MapFrom(src => src.Items.Count))
                .ForCtorParam("TotalCount", opt =>
                    opt.MapFrom(src => src.TotalCount));

            // Activity -> ActivityDto
            CreateMap<Activity, ActivityDto>()
                .ForMember(
                    dest => dest.ActionTypeName,
                    opt => opt.MapFrom(src => src.ActionType.Name));

            // CreateActivityDto -> Activity
            CreateMap<CreateActivityDto, Activity>();

            // UpdateActivityDto -> Activity
            CreateMap<UpdateActivityDto, Activity>();

            // PagedList<Activity> -> PagedResponse<ActivityDto>
            CreateMap<PagedList<Activity>, PagedResponse<ActivityDto>>()
                .ForCtorParam("Items", opt =>
                    opt.MapFrom(src => src.Items))
                .ForCtorParam("PageNumber", opt =>
                    opt.MapFrom(src => 1))
                .ForCtorParam("PageSize", opt =>
                    opt.MapFrom(src => src.Items.Count))
                .ForCtorParam("TotalCount", opt =>
                    opt.MapFrom(src => src.TotalCount));


            CreateMap<ActionType, ActionTypeDto>();
            // PagedList<ActionType> -> PagedResponse<ActionTypeDto>
            CreateMap<PagedList<ActionType>, PagedResponse<ActionTypeDto>>()
                .ForCtorParam("Items", opt =>
                    opt.MapFrom(src => src.Items))
                .ForCtorParam("PageNumber", opt =>
                    opt.MapFrom(src => 1))
                .ForCtorParam("PageSize", opt =>
                    opt.MapFrom(src => src.Items.Count))
                .ForCtorParam("TotalCount", opt =>
                    opt.MapFrom(src => src.TotalCount));

            CreateMap<Document, DocumentDto>()
                .ForCtorParam("FileName", opt => opt.MapFrom(src => src.StoredFile.FileName))
                .ForCtorParam("ContentType", opt => opt.MapFrom(src => src.StoredFile.ContentType))
                .ForCtorParam("Size", opt => opt.MapFrom(src => src.StoredFile.Size))
                .ForCtorParam("UploadedAt", opt => opt.MapFrom(src => src.UploadAt))
                .ForCtorParam("CourseId", opt => opt.MapFrom(src => src.CourseId ?? src.Module!.CourseId));

            CreateMap<Submission, SubmissionDto>()
                .ForCtorParam("Status", opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<FeedBack, FeedbackDto>()
                .ForCtorParam("TeacherName", opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));

            CreateMap<StudentAssignmentReadModel, AssignmentDetailsDto>()
                .ForMember(
                        dest => dest.AssignmentStatus,
                        opt => opt.MapFrom(src => MapAssignmentStatus(src.Status)));

            CreateMap<PagedList<StudentAssignmentReadModel>,

            PagedResponse<AssignmentDetailsDto>>()
                .ForCtorParam("Items", opt =>
                        opt.MapFrom(src => src.Items))
                .ForCtorParam("PageNumber", opt =>
                        opt.MapFrom(src => 1))
                .ForCtorParam("PageSize", opt =>
                        opt.MapFrom(src => src.Items.Count))
                .ForCtorParam("TotalCount", opt =>
                        opt.MapFrom(src => src.TotalCount));

            CreateMap<TeacherSubmissionReadModel, TeacherAssignmentSubmissionDto>();
            CreateMap<PagedList<TeacherSubmissionReadModel>,
            PagedResponse<TeacherAssignmentSubmissionDto>>()
                .ForCtorParam("Items", opt => opt.MapFrom(src => src.Items))
                .ForCtorParam("PageNumber", opt => opt.MapFrom(src => 1))
                .ForCtorParam("PageSize", opt => opt.MapFrom(src => src.Items.Count))
                .ForCtorParam("TotalCount", opt => opt.MapFrom(src => src.TotalCount));

        }

        private static AssignmentStatus MapAssignmentStatus(string? status)
        {
            return Enum.TryParse<AssignmentStatus>(
                status,
                true,
                out var result)
                    ? result
                    : AssignmentStatus.Unsubmitted;
        }
    }
}
