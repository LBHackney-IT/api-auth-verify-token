using ApiAuthTokenGenerator.V1.Boundary.Request;
using ApiAuthTokenGenerator.V1.Boundary.Response;
using ApiAuthTokenGenerator.V1.Domain;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;

namespace ApiAuthTokenGenerator.V1.Controllers
{
    [ApiController]
    [Route("api/v1/tokens")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ApiAuthTokenGeneratorController : BaseController
    {
        //Not removing GET usecases for now as will be used in future endpoints
        private readonly IGetAllUseCase _getAllUseCase;
        private readonly IGetByIdUseCase _getByIdUseCase;
        private readonly IPostTokenUseCase _postTokenUseCase;

        public ApiAuthTokenGeneratorController(IGetAllUseCase getAllUseCase, IGetByIdUseCase getByIdUseCase, IPostTokenUseCase postTokenUseCase)
        {
            _getAllUseCase = getAllUseCase;
            _getByIdUseCase = getByIdUseCase;
            _postTokenUseCase = postTokenUseCase;
        }

        //TODO: add xml comments containing information that will be included in the auto generated swagger docs (https://github.com/LBHackney-IT/lbh-base-api/wiki/Controllers-and-Response-Objects)
        /// <summary>
        /// ...
        /// </summary>
        /// <response code="200">...</response>
        /// <response code="400">Invalid Query Parameter.</response>
        [ProducesResponseType(typeof(ResponseObjectList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult ListContacts()
        {
            return Ok(_getAllUseCase.Execute());
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <response code="200">...</response>
        /// <response code="404">No ? found for the specified ID</response>
        [ProducesResponseType(typeof(ResponseObject), StatusCodes.Status200OK)]
        [HttpGet]
        [ActionName("GetToken")]
        //TODO: rename to match the identifier that will be used
        [Route("{id}")]
        public IActionResult ViewRecord(int id)
        {
            return Ok(_getByIdUseCase.Execute(id));
        }

        /// <summary>
        /// Generates a token to be used for auth purposes for Hackney APIs
        /// </summary>
        /// <response code="201">Token successfully generated</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem generating a token.</response>
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GenerateTokenResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult GenerateToken([FromBody] TokenRequestObject tokenRequest)
        {
            try
            {
                var response = _postTokenUseCase.Execute(tokenRequest);
                return CreatedAtAction("GetToken", new { id = response.Id }, response);
            }
            catch (TokenNotInsertedException)
            {
                return StatusCode(500, "There was a problem inserting the token data into the database.");
            }
            catch (JwtTokenNotGeneratedException)
            {
                return StatusCode(500, "There was a problem generating a JWT token");
            }
        }
    }
}
