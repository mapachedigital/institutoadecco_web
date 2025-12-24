// Copyright (c) 2025, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace InstitutoAdecco;

public class Globals
{
    // Define here the roles for the app
    public const string RoleAdmin = "Administrator"; // The super user. Can do anything
    public const string RoleSupervisor = "Supervisor"; // The supervisor can do mostly anything, except managing the super user
    public const string RoleCompanyUser = "Company";
    public static readonly string[] Roles = [RoleAdmin, RoleSupervisor, RoleCompanyUser];

    public const string ConfigSuperAdminEmail = "SuperAdmin:Email";
    public const string ConfigSuperAdminFirstname = "SuperAdmin:Firstname";
    public const string ConfigSuperAdminLastname = "SuperAdmin:Lastname";
    public const string ConfigSuperAdminCompany = "SuperAdmin:Company";
    public const string ConfigSuperAdminPhoneNumber = "SuperAdmin:PhoneNumber";
    public const string ConfigSuperAdminPassword = "SuperAdmin:Password";

    public const string GoogleTagManagerCode = "GTM-TH6H2SNV";

    public const string StorageContainerNameAttachments = "attachments";

    public const int DefaultPageSize = 9;
    public const string GuidRegex = @"^\/uploads\/([\d]{4})\/([\d]{2})\/([\w\-.]+)$";
}
