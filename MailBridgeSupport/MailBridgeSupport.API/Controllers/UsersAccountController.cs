using CSharpFunctionalExtensions;
using MailBridgeSupport.API.Contracts;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MailBridgeSupport.API.Controllers;

public class UsersAccountController : BaseController
{
    private readonly ILogger _logger;
    private readonly JWTSecretOptions _options;
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ISessionsRepository _sessionsRepository;

    public UsersAccountController(
        ILogger<UsersAccountController> logger,
        IOptions<JWTSecretOptions> options,
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ISessionsRepository sessionsRepository)
    {
        _logger = logger;
        _options = options.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _sessionsRepository = sessionsRepository;
    }

    /// <summary>
    /// System admin registration.
    /// </summary>
    /// <param name="request">Nickname, email and password.</param>
    /// <returns>Success.</returns>
    [AllowAnonymous]
    [HttpPost("systemadmin/registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationSystemAdmin(
        [FromBody] MailAdminRegistrationRequest request)
    {
        var courseAdmin = await _userManager.FindByEmailAsync(request.Email);
        if (courseAdmin is null)
        {
            var mailAdmin = new UserEntity
            {
                UserName = request.Nickname,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(
                mailAdmin,
                request.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("{errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.SystemAdmin));
            if (!roleExists)
            {
                var role = new IdentityRole<Guid>()
                {
                    Name = nameof(Roles.SystemAdmin)
                };

                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(mailAdmin, nameof(Roles.SystemAdmin));
        }
        else
        {
            var error = "User is existing.";
            _logger.LogError("{error}", error);
            return BadRequest(error);
        }

        return Ok(true);
    }

    /// <summary>
    /// User registration.
    /// </summary>
    /// <param name="request">Nickname, email and password.</param>
    /// <returns>Success.</returns>
    [AllowAnonymous]
    [HttpPost("user/registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrationUser(
        [FromBody] MailAdminRegistrationRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            var newUser = new UserEntity
            {
                UserName = request.Nickname,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(
                newUser,
                request.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("{errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.User));
            if (!roleExists)
            {
                var role = new IdentityRole<Guid>()
                {
                    Name = nameof(Roles.User)
                };

                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(newUser, nameof(Roles.User));
        }
        else
        {
            var error = "User is existing.";
            _logger.LogError("{error}", error);
            return BadRequest(error);
        }

        return Ok(true);
    }

    /// <summary>
    /// Users login.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <returns>Jwt token.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogIn([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest("Email not found.");
        }

        var isSuccess = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!isSuccess)
        {
            return BadRequest("Password is incorrect.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role is null)
        {
            return BadRequest("Role isn't exist.");
        }

        var userInformation = new UserInformation(user.UserName, user.Id, role);
        var accessToken = JwtHelper.CreateAccessToken(userInformation, _options);
        var refreshToken = JwtHelper.CreateRefreshToken(userInformation, _options);

        var session = Session.Create(user.Id, accessToken, refreshToken);
        if (session.IsFailure)
        {
            _logger.LogError("{error}", session.Error);
            return BadRequest(session.Error);
        }

        var result = await _sessionsRepository.Create(session.Value);

        if (result.IsFailure)
        {
            _logger.LogError("{error}", result.Error);
            return BadRequest(result.Error);
        }

        Response.Cookies.Append("refreshtoken", refreshToken, new CookieOptions()
        {
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax //SameSite = SameSiteMode.None
        });

        return Ok(new TokenResponse
        {
            Role = role,
            AccessToken = accessToken,
            Nickname = user.UserName
        });
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    /// <param name="request">Access token.</param>
    /// <returns>New access token and refresh token.</returns>
    [AllowAnonymous]
    [HttpPost("refreshaccesstoken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshAccessToken()
    {
        const bool jwtTokenV2 = false;
        Result<UserInformation> userInformation;
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest("Refresh Token can't be null");
        }

        if (jwtTokenV2)
        {
            userInformation = JwtHelper.GetPayloadFromJWTTokenV2(refreshToken, _options);
        }
        else
        {
            var payload = JwtHelper.GetPayloadFromJWTToken(refreshToken, _options);
            userInformation = JwtHelper.ParseUserInformation(payload);
        }

        if (userInformation.IsFailure)
        {
            _logger.LogError("{error}", userInformation.Error);
            return BadRequest(userInformation.Error);
        }

        var resultGet = await _sessionsRepository.GetById(userInformation.Value.UserId);
        if (resultGet.IsFailure)
        {
            _logger.LogError("{error}", resultGet.Error);
            return BadRequest(resultGet.Error);
        }

        if (resultGet.Value.RefreshToken != refreshToken)
        {
            _logger.LogError("error", "Refresh tokens not equals.");
            return BadRequest("Refresh tokens not equals.");
        }

        var accsessToken = JwtHelper.CreateAccessToken(userInformation.Value, _options);

        var session = Session.Create(userInformation.Value.UserId, accsessToken, resultGet.Value.RefreshToken);
        if (resultGet.IsFailure)
        {
            _logger.LogError("{error}", resultGet.Error);
            return BadRequest(resultGet.Error);
        }

        var result = await _sessionsRepository.Create(session.Value);
        if (result.IsFailure)
        {
            _logger.LogError("{error}", result.Error);
            return BadRequest(result.Error);
        }

        return Ok(new TokenResponse
        {
            Role = userInformation.Value.Role,
            AccessToken = accsessToken,
            Nickname = userInformation.Value.Nickname
        });
    }
}