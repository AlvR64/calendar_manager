using Calendar.Application.Abstractions.Messaging;
using Calendar.Application.Auth.LoginAdmin;
using Calendar.Application.Auth.LoginCustomer;
using Calendar.Application.Auth.RegisterBusiness;
using Calendar.Application.Auth.RegisterCustomer;
using Calendar.Application.Businesses;
using Calendar.Application.Businesses.UpdateBusinessBookingWindow;
using Calendar.Application.Businesses.UpdateBusinessDetails;
using Calendar.Application.Services.CreateService;
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

        return services;
    }
}
