﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ttn.Web.JwtToken;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiSample.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<ApiSampleUser> _userManager;
        private readonly SignInManager<ApiSampleUser> _signInManager;
        private readonly JwtTokenUtil _tokenUtil;

        public AccountController(UserManager<ApiSampleUser> userManager,
    SignInManager<ApiSampleUser> signInManager, JwtTokenUtil tokenUtil)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenUtil = tokenUtil;
        }

        /// <summary>
        /// show options
        /// </summary>
        /// <returns></returns>
        public ActionResult Info()
        {
            return Ok(_tokenUtil.GetOptions());
        }

        private Task<ApiSampleUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        // POST api/account/register

        [HttpPost]
        public async Task<ActionResult<object>> Register()
        {
            var passwordHasher = (IPasswordHasher<ApiSampleUser>)KContext.ServiceProvider.GetService(typeof(IPasswordHasher<ApiSampleUser>));

            var user = new ApiSampleUser();
            user.UserName = "admin";
            user.PhoneNumber = "1234567";
            user.PasswordHash = passwordHasher.HashPassword(user, "passpass1");
            await _userManager.CreateAsync(user);
            return new { status = "ok" };
        }

        // POST api/account/login

        [HttpPost]
        public async Task<ActionResult<object>> Login([FromForm]string userName, [FromForm]string password)
        {
            if (String.IsNullOrEmpty(userName))
            {
                return new { msg = "userName can't be null" };
            }

            if (String.IsNullOrEmpty(password))
            {
                return new { msg = "password can't be null" };
            }

            var isValid = userName == "admin" && password == "pass";
            if (!isValid)
            {
                return Ok(new { success = false, msg = "userName or password is not valide" });
            }

            TokenResponse res = await _tokenUtil.AuthorizeClientAsync(_userManager, _signInManager, userName, password);

            return Ok(new { success = true, token = res.AccessToken, refreshToken = res.RefreshToken, expiresIn = res.ExpiresIn });
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Json(new { status = "ok" });
        }


        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [Authorize(AuthenticationSchemes = Consts.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> GetToken([FromForm] string refreshToken)
        {
            TokenResponse token = await _tokenUtil.RefreshTokenAsync(_userManager, _signInManager, refreshToken);

            if (null == token)
            {
                return BadRequest("request failed.");
            }

            return Ok(token);
        }
    }
}
