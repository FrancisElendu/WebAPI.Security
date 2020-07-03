using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Security.Data.Repository.IRepository;
using WebAPI.Security.Models.Dtos;

namespace WebAPI.Security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticateController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(UserForRegisterDto userForRegisterDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _unitOfWork.AuthServices.Authenticate(userForRegisterDto, userForRegisterDto.Password);
                if (result.IsSuccess)
                    return Ok(result);
                if (result == null)
                    return NotFound(new { message = "User not Found" });
                if (!result.IsSuccess)
                    return BadRequest(result);
            }
            return BadRequest("Model not valid");
        }
    }
}
