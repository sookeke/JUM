using jumwebapi.Features.Users.Commands;
using jumwebapi.Features.Users.Models;
using jumwebapi.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace jumwebapi.Features.Users.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers()
    {
        var e =  await _mediator.Send(new AllUsersQuery());
        return new JsonResult(e);
    }

    [HttpGet("{username:alpha}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string username)
    {
        var user = await _mediator.Send(new GetUserQuery(username));
        return new JsonResult(user);
    }
    [HttpGet("{partId:long}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUser(long partId)
    {
        var user = await _mediator.Send(new GetUserByPartId(partId));
        return new JsonResult(user);
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUser([FromBody] UserModel user)
    {

        var entity = await _mediator.Send(new CreateUserCommand(user.UserId,
            user.UserName, user.ParticipantId, user.IsDisable, user.FirstName, user.LastName, user.MiddleName,user.PreferredName, user.PhoneNumber, user.Email, user.BirthDate,
            user.AgencyId, user.PartyTypeCode, user.Roles         
            ));
        return Ok(entity);
    }
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(long ParticipantId, [FromBody] UserModel user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var update = await _mediator.Send(new UpdateUserCommand(
                user.UserId, user.UserName, user.ParticipantId, user.IsDisable,
                user.FirstName, user.LastName, user.MiddleName, user.PreferredName,
                user.PhoneNumber, user.Email, user.BirthDate, user.AgencyId,
                user.PartyTypeCode, user.Roles
            ));

        return Ok(update);
    }


}
