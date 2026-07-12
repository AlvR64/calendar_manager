using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.LoginAdmin;
using Calendar.Application.Auth.LoginCustomer;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
using Calendar.Application.Businesses;
using Calendar.Application.Businesses.UpdateBusinessBookingWindow;
using Calendar.Application.Businesses.UpdateBusinessDetails;
using Calendar.Application.Services;
using Calendar.Application.Services.CreateService;
using Calendar.Application.Services.DeleteService;
using Calendar.Application.Services.UpdateService;
using Calendar.Application.Services.UpdateServiceActiveState;
using Calendar.Application.StaffMemberServices.AssignStaffMemberService;
using Calendar.Application.StaffMembers.CreateStaffMember;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<LoginAdminCommand, LoginAdminResult>, LoginAdminCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCustomerCommand, LoginCustomerResult>, LoginCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterBusinessCommand, RegisterBusinessResult>, RegisterBusinessCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterCustomerCommand, RegisterCustomerResult>, RegisterCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<CreateServiceCommand, CreateServiceResult>, CreateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateServiceCommand, UpdateServiceResult>, UpdateServiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateServiceActiveStateCommand, UpdateServiceActiveStateResult>, UpdateServiceActiveStateCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteServiceCommand, DeleteServiceResult>, DeleteServiceCommandHandler>();
        services.AddScoped<ICommandHandler<CreateStaffMemberCommand, CreateStaffMemberResult>, CreateStaffMemberCommandHandler>();
        services.AddScoped<ICommandHandler<AssignStaffMemberServiceCommand, AssignStaffMemberServiceResult>, AssignStaffMemberServiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBusinessDetailsCommand, UpdateBusinessDetailsResult>, UpdateBusinessDetailsCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateBusinessBookingWindowCommand, UpdateBusinessBookingWindowResult>, UpdateBusinessBookingWindowCommandHandler>();
        services.AddScoped<IQueryHandler<GetBusinessByIdQuery, BusinessDetails?>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<GetBusinessProfileByIdQuery, BusinessProfileDetails?>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<GetBusinessProfileBySlugQuery, BusinessProfileDetails?>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<ListBusinessServicesQuery, ListBusinessServicesResult>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<GetBusinessServiceQuery, BusinessServiceDetails?>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<ListBusinessStaffMembersQuery, ListBusinessStaffMembersResult>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<GetBusinessStaffMemberQuery, BusinessStaffMemberDetails?>, PublicBusinessQueryHandler>();
        services.AddScoped<IQueryHandler<ListAdminServicesQuery, ListAdminServicesResult>, AdminServiceQueryHandler>();
        services.AddScoped<IQueryHandler<GetAdminServiceQuery, GetAdminServiceResult>, AdminServiceQueryHandler>();

        return services;
    }
}
