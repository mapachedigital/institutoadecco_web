// Copyright (c) 2021, Mapache Digital
// Version: 1.14
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using InstitutoAdecco.Data;
using InstitutoAdecco.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InstitutoAdecco.Utils;

/// <summary>
/// Utility classes for users
/// </summary>
public interface IUserUtils
{
    /// <summary>
    /// Checks if there is a user logged in and is a valid one
    /// </summary>
    bool IsLoggedIn();

    /// <summary>
    /// Obtain the current user ID from the Session
    /// </summary>
    /// <returns>The current user ID</returns>
    /// <exception cref="InvalidOperationException">If a user cannot be found</exception>
    string? GetUserId();

    /// <summary>
    /// Create the user roles and assign a super admin one to the configured user
    /// </summary>
    Task InitializeRolesAsync();

    /// <summary>
    /// Obtain the current user name from the session
    /// </summary>
    /// <returns>The current user name</returns>
    /// <exception cref="InvalidOperationException">If a user cannot be found</exception>
    string GetUserName();

    /// <summary>
    /// Obtain the list of roles that tue user belongs to
    /// </summary>
    /// <param name="id">The user Id</param>
    Task<List<string>> GetUserRolesAsync(string id);

    /// <summary>
    /// Obtains the current logged-in User
    /// </summary>
    /// <returns>The current logged-in User</returns>
    Task<ApplicationUser> GetUserAsync();

    /// <summary>
    /// Obtain a user from its Id
    /// </summary>
    /// <param name="userId">The user to get from the database</param>
    /// <returns>The user whose ID matches the supplied one</returns>
    /// <exception cref="ArgumentException">If a valid userId is not provided</exception>
    Task<ApplicationUser> GetUserAsync(string userId);

    /// <summary>
    /// Checks that the current user has all the required information set
    /// </summary>
    bool IsUserInfoComplete(string userId);

    /// <summary>
    /// Verifies that a given password is correct for the given user
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="password">The password to check</param>
    bool CheckPassword(ApplicationUser user, string password);

    /// <summary>
    /// Returns whether the current user has the SuperAdmin role
    /// </summary>
    Task<bool> IsAdminAsync(bool onlySuperAdmin);

    /// <summary>
    /// Returns whether the given user has the SuperAdmin role
    /// </summary>
    Task<bool> IsAdminAsync(ApplicationUser user, bool onlySuperAdmin);

    /// <summary>
    /// Returns whether the current user is in the given role
    /// </summary>
    Task<bool> UserInRoleAsync(string roleName);

    /// <summary>
    /// Validate that the given password complies with the rules
    /// </summary>
    /// <returns>A list of the errors found</returns>
    Task<List<string>> ValidatePasswordQualityAsync(string password);

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    Task<List<ApplicationUser>> GetAllUsersAsync(bool onlyEnabled = false);

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    Task<List<ApplicationUser>> GetAllUsersAsync(string? roleName, bool onlyEnabled = false);

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    Task<List<ApplicationUser>> GetAllUsersAsync(List<string> roleNames, bool onlyEnabled = false);

    /// <summary>
    /// Determine if the specified user is locked out by the system
    /// </summary>
    Task<bool> IsLockedOutAsync(ApplicationUser user);

    /// <summary>
    /// Returns the mail store used by the usermanager.
    /// TODO: Check if with this we can use our own instead of the Identity one.
    /// </summary>
    IUserEmailStore<ApplicationUser> GetEmailStore();
}

/// <summary>
/// Utility classes for user management
/// </summary>
public class UserUtils : IUserUtils
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public UserUtils(UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Verifies that a given password is correct for the given user
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <param name="password">The password to check</param>
    public bool CheckPassword(ApplicationUser user, string password)
    {
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        var hasher = _userManager.PasswordHasher;

        return hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;
    }

    /// <summary>
    /// Obtains the current logged-in User
    /// </summary>
    /// <returns>The current logged-in User</returns>
    public async Task<ApplicationUser> GetUserAsync()
    {
        var userId = GetUserId() ?? throw new InvalidOperationException("Cannot get current user Id");

        var user = await _userManager.FindByIdAsync(userId);

        return user ?? throw new InvalidOperationException("Current user not found");
    }

    /// <summary>
    /// Obtain a user from its Id
    /// </summary>
    /// <param name="userId">The user to get from the database</param>
    /// <returns>The user whose ID matches the supplied one</returns>
    /// <exception cref="ArgumentException">If a valid userId is not provided</exception>
    public async Task<ApplicationUser> GetUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("Not a valid user");
        }

        return await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("Current user not found");
    }

    /// <summary>
    /// Obtain the current user ID from the Session
    /// </summary>
    /// <returns>The current user ID</returns>
    /// <exception cref="InvalidOperationException">If a user cannot be found</exception>
    public string? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            return null;
        }

        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            //User not authenticated
            return null;
        }

        // Get the User ID
        return _userManager.GetUserId(user) ?? throw new InvalidOperationException("Not a valid user");
    }

    /// <summary>
    /// Obtain the current user name from the session
    /// </summary>
    /// <returns>The current user name</returns>
    /// <exception cref="InvalidOperationException">If a user cannot be found</exception>
    public string GetUserName()
    {
        var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("Can't obtain current user");
        return _userManager.GetUserName(user) ?? throw new InvalidOperationException("Not a valid user");
    }

    /// <summary>
    /// Obtain the list of roles that tue user belongs to
    /// </summary>
    /// <param name="userId">The user Id</param>
    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User id is required");

        var user = await _userManager.FindByIdAsync(userId) ?? throw new ArgumentException("Non existing user");
        var roles = await _userManager.GetRolesAsync(user);
        return [.. roles.OrderBy(x => x)];
    }

    /// <summary>
    /// Create the user roles and assign a super admin one to the configured user
    /// </summary>
    public async Task InitializeRolesAsync()
    {
        // 1. Create all the roles
        foreach (var roleName in Globals.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new Exception($"Cannot create role {roleName}");
                }
            }
        }

        // 2. Assign the sueperadmin role to the chosen user (if not existing, create)
        var superAdminEmail = _configuration[Globals.ConfigSuperAdminEmail] ?? throw new Exception("Super Admin Email config not found");
        var superAdminFirstname = _configuration[Globals.ConfigSuperAdminFirstname] ?? throw new Exception("Super Admin Firstname config not found");
        var superAdminLastname = _configuration[Globals.ConfigSuperAdminLastname] ?? throw new Exception("Super Admin Lastname config not found");
        var superAdminCompany = _configuration[Globals.ConfigSuperAdminCompany] ?? throw new Exception("Super Admin Company config not found");
        var superAdminPassword = _configuration[Globals.ConfigSuperAdminPassword] ?? throw new Exception("Super Admin password config not found");

        var superAdminUser = await _userManager.FindByNameAsync(superAdminEmail);

        superAdminUser ??= await CreateUserAsync(username: superAdminEmail,
                                                     firstname: superAdminFirstname,
                                                     lastname: superAdminLastname,
                                                     company: superAdminCompany,
                                                     acceptTermsOfService: true,
                                                     email: superAdminEmail,
                                                     emailConfirmed: true,
                                                     approved: true,
                                                     password: superAdminPassword);

        await _userManager.AddToRoleAsync(superAdminUser, Globals.RoleAdmin);
    }

    /// <summary>
    /// Checks that the current user has all the required information set
    /// </summary>
    public bool IsUserInfoComplete(string userId)
    {
        // TODO: Whatever you check here, keep in mind that it will be checked on *every* request. So make it quick!
        // If possible, implement it in the dashboard, or after log in so it will be performed only once.
        // Also, keep a reliable data base with model validations so you don't have to check consistency
        return true;
    }

    /// <summary>
    /// Checks if there is a user logged in and is a valid one
    /// </summary>
    public bool IsLoggedIn()
    {
        var identity = _httpContextAccessor.HttpContext?.User.Identity;

        return identity != null && identity.IsAuthenticated;
    }

    /// <summary>
    /// Returns whether the current user has the SuperAdmin role
    /// </summary>
    public async Task<bool> IsAdminAsync(bool onlySuperAdmin)
    {
        var user = await GetUserAsync();
        return await IsAdminAsync(user, onlySuperAdmin);
    }

    /// <summary>
    /// Returns whether the given user has the SuperAdmin role
    /// </summary>
    public async Task<bool> IsAdminAsync(ApplicationUser user, bool onlySuperAdmin)
    {
        // Superadmin always return true
        var isSuperAdmin = await _userManager.IsInRoleAsync(user, Globals.RoleAdmin);
        if (isSuperAdmin) return true;

        // So, the user is not a super admin and the caller only wanted super admins...
        if (onlySuperAdmin) return false;

        // Normal admins
        var isNormalAdmin = await _userManager.IsInRoleAsync(user, Globals.RoleSupervisor);
        if (isNormalAdmin) return true;

        return false;
    }

    /// <summary>
    /// Returns whether the current user is in the given role
    /// </summary>
    public async Task<bool> UserInRoleAsync(string roleName)
    {
        var user = await GetUserAsync();
        return await _userManager.IsInRoleAsync(user, roleName);
    }

    /// <summary>
    /// Validate that the given password complies with the rules
    /// </summary>
    /// <returns>A list of the errors found</returns>
    public async Task<List<string>> ValidatePasswordQualityAsync(string password)
    {
        var passwordErrors = new List<string>();

        var validators = _userManager.PasswordValidators;

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(_userManager, new ApplicationUser(), password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    passwordErrors.Add(error.Description);
                }
            }
        }
        return passwordErrors;
    }

    /// <summary>
    /// Creates a user and adds it to the database
    /// </summary>
    /// <param name="username">The username of the user. Usually it's the same as the Email</param>
    /// <param name="firstname">The firstname of the user.</param>
    /// <param name="lastname">The lastname of the user.</param>
    /// <param name="company">The company of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="phoneNumber">The phone number of the user.</param>
    /// <param name="emailConfirmed">Whether the email is confirmed.  If not, depending on the site policy it won't be able to access until confirmed.</param>
    /// <param name="password">The password for the user.</param>
    /// <returns>The newly created user.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user creation is not successful.</exception>
    public async Task<ApplicationUser> CreateUserAsync(string username, string firstname, string lastname, string company, string email, bool acceptTermsOfService, bool emailConfirmed, bool approved, string password)
    {
        ApplicationUser user;

        try
        {
            user = Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }

        user.Firstname = firstname;
        user.Lastname = lastname;
        user.Company = company;
        user.EmailConfirmed = emailConfirmed;
        user.AcceptTermsOfService = acceptTermsOfService;
        user.Approved = approved;

        await _userStore.SetUserNameAsync(user, username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Couldn't create new user");
        }

        return user;
    }

    /// <summary>
    /// Returns the email store used by the usermanager.
    /// TODO: Check if with this we can use our own instead of the Identity one.
    /// </summary>
    public IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    public async Task<List<ApplicationUser>> GetAllUsersAsync(bool onlyEnabled = false)
    {
        return await GetAllUsersAsync([], onlyEnabled);
    }

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    public async Task<List<ApplicationUser>> GetAllUsersAsync(string? roleName, bool onlyEnabled = false)
    {
        return string.IsNullOrEmpty(roleName) ? await GetAllUsersAsync([], onlyEnabled) : await GetAllUsersAsync([roleName], onlyEnabled);
    }

    /// <summary>
    /// Obtain the list of users for the site ordered by the last accessed date.
    /// </summary>
    /// <returns>The list of users</returns>
    public async Task<List<ApplicationUser>> GetAllUsersAsync(List<string> roleNames, bool onlyEnabled = false)
    {
        var users = _context.ApplicationUser.Select(x => x);

        if (onlyEnabled)
        {
            users = users.Where(x => !x.LockoutEnabled || x.LockoutEnd == null || x.LockoutEnd < DateTime.UtcNow);
        }

        var result = await users
             .OrderByDescending(x => x.LastAccess)
             .ToListAsync();

        if (roleNames.Count != 0)
        {
            for (int i = result.Count - 1; i >= 0; i--)
            {
                var roles = await _userManager.GetRolesAsync(result[i]);

                if (!roleNames.Intersect(roles).Any())
                {
                    result.RemoveAt(i);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determine if the specified user is locked out by the system
    /// </summary>
    public async Task<bool> IsLockedOutAsync(ApplicationUser user)
    {
        var lockoutEnabled = await _userManager.GetLockoutEnabledAsync(user);
        var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
        return lockoutEnabled && lockoutEndDate > DateTime.UtcNow;
    }
}
