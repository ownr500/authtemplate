using API.Constants;
using API.Controllers;
using API.Controllers.Dtos;
using API.Core.Models;
using API.Core.Services;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class UserControllerUnitTests
{
    private readonly IUserService _userService;
    private readonly UserController _controller;
    private readonly CancellationToken _ct = CancellationToken.None;

    public UserControllerUnitTests()
    {
        _userService = Substitute.For<IUserService>();
        _controller = new UserController(_userService);
    }

    [Fact]
    public async Task ShouldDelete()
    {
        //Arrange
        var login = "login";
        var deleteResult = Result.Ok();
        _userService.DeleteAsync(Arg.Any<string>(), _ct)
            .Returns(deleteResult);
        
        //Act
        var result = await _controller.Delete(login, _ct);

        //Assert
        await _userService.Received(1)
            .DeleteAsync(Arg.Is<string>(x => x == login), _ct);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ShouldNotDeleteAsync()
    {
        //Arrange
        var login = "login";
        var ct = CancellationToken.None;
        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        _userService.DeleteAsync(Arg.Any<string>(), ct)
            .Returns(new Result().WithErrors(errors));

        //Act
        var actual = await _controller.Delete(login, ct);

        //Assert
        await _userService.Received(1)
            .DeleteAsync(Arg.Is<string>(x => x == login), ct);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actual);
        var businessError = Assert.IsType<BusinessErrorDto>(conflictResult.Value);
        Assert.Equivalent(errors, businessError.Messages);

    }
    
    [Fact]
    public async Task ShouldUpdateFirstLastNameAsync()
    {
        //Arrange
        var firstName = "John";
        var lastName = "Doe";
        var requestDto = new UpdateFirstLastNameRequestDto(firstName, lastName);
        var changeResult = Result.Ok();
        _userService.UpdateFirstLastNameAsync(Arg.Any<UpdateFirstLastNameModel>(), _ct)
            .Returns(changeResult);

        //Act
        var actual = await _controller.UpdateFirstLastName(requestDto, _ct);

        //Assert
        await _userService.Received(1)
            .UpdateFirstLastNameAsync(Arg.Is<UpdateFirstLastNameModel>(x => x.FirstName == firstName
                                                    && x.LastName == lastName), _ct);
        Assert.IsType<OkResult>(actual);
    }

    [Fact]
    public async Task ShouldNotUpdateFirstLastNameAsync()
    {
        //Arrange
        var firstName = "Michael";
        var lastName = "Dee";
        var request = new UpdateFirstLastNameRequestDto(firstName, lastName);
        var errorMessage = MessageConstants.UserNotFound;
        var errors = new List<string> { errorMessage };
        var result = new Result().WithErrors(errors);
        _userService.UpdateFirstLastNameAsync(Arg.Any<UpdateFirstLastNameModel>(), _ct)
            .Returns(result);

        //Act
        var actual = await _controller.UpdateFirstLastName(request, _ct);

        //Assert
        await _userService.Received(1)
            .UpdateFirstLastNameAsync(Arg.Is<UpdateFirstLastNameModel>(x => x.FirstName == firstName
                                                                            && x.LastName == lastName), _ct);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actual);
        var businessError = Assert.IsType<BusinessErrorDto>(conflictResult.Value);
        Assert.Equivalent(errors, businessError.Messages);
    }
}