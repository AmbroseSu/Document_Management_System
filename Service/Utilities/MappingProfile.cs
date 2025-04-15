using AutoMapper;
using BusinessObject;
using DataAccess.DTO;
using DataAccess.DTO.Request;

namespace Service.Response;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        /*CreateMap<User, UpsertUserDTO>();
        CreateMap<UpsertUserDTO, User>();
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();
        CreateMap<Ticket, TicketDTO>();
        CreateMap<TicketDTO, Ticket>();
        //CreateMap<NewTicket, Ticket>();
        CreateMap<NewTicket, Ticket>()
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src =>
                DateTime.ParseExact(src.ExpirationDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)));
        CreateMap<updateTicketRequest, Ticket>();
        CreateMap<NewPostRequest, Post>();
        CreateMap<TicketRequest, RequestTicket>();
        CreateMap<RequestTicket, TicketRequest>();
        CreateMap<TicketRequest, TicketRequestDTO>();
        CreateMap<TicketRequestDTO, TicketRequest>();
        CreateMap<ImageTicket, ImageTicketDTO>();
        CreateMap<Post, PostDTO>();
        CreateMap<Order, OrderDTO>();
        CreateMap<OrderDTO, Order>();
        CreateMap<Feedback, FeedbackResponse>();
        CreateMap<FeedbackDTO, Feedback>();
        CreateMap<ImageFeedback, ImageFeedbackDTO>();
        CreateMap<PlatformFee, PlatformFeeDTO>();
        CreateMap<NewFeedback, Feedback>();
        CreateMap<OrderStatus, OrderStatusDTO>();
        CreateMap<OrderStatusDTO, OrderStatus>();

        //TicketResponse
        CreateMap<FeedbackResponse, FeedbackTicketElement>();
        CreateMap<List<FeedbackTicketElement>, TicketResponse>();
        CreateMap<Category, TicketResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Ticket, TicketResponse>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TicketName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Venue))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<User, TicketResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Post, TicketResponse>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PostTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.PostDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CurrentPostStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        //PostResponse
        CreateMap<Post, PostElement>();
        CreateMap<Category, PostResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Ticket, PostResponse>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TicketName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
            .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Venue))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<User, PostResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<PlatformFee, PlatformFeeDTO>();
        CreateMap<Transaction, TransactionDTO>();*/

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.DivisionDto, opt => opt.Ignore())
            .ForMember(dest => dest.DivisionId, opt => opt.MapFrom(src => src.DivisionId));
        CreateMap<UserDto, User>();
        CreateMap<User, UserRequest>()
            .ForMember(dest => dest.RoleId, opt => opt.Ignore());
        CreateMap<UserRequest, User>();
        CreateMap<Permission, PermissionDto>();
        CreateMap<PermissionDto, Permission>();
        CreateMap<ResourceDto, Resource>();
        CreateMap<Resource, ResourceDto>();
        CreateMap<Role, RoleDto>();
        CreateMap<RoleDto, Role>();
        CreateMap<RoleResource, RoleResourceDto>();
        CreateMap<RoleResourceDto, RoleResource>();
        CreateMap<Resource, ResourceResponse>();
        CreateMap<ResourceResponse, Resource>();
        CreateMap<Division, DivisionDto>();
        CreateMap<DivisionDto, Division>();
        CreateMap<DocumentType, DocumentTypeDto>();
        CreateMap<DocumentTypeDto, DocumentType>();
        CreateMap<Workflow, WorkflowDto>();
        CreateMap<WorkflowDto, Workflow>();
        CreateMap<Step, StepDto>();
        CreateMap<StepDto, Step>();
        CreateMap<Task, TaskDto>();
        CreateMap<TaskDto, Task>();
        CreateMap<DocumentDto, Document>();
        CreateMap<Document, DocumentDto>();
        CreateMap<DocumentVersion, DocumentVersionDto>();
        CreateMap<DocumentVersionDto, DocumentVersion>();
    }
}