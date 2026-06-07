namespace GamaEdtech.Presentation.Api.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.Specification.Impl;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Domain.Entity;
    using GamaEdtech.Domain.Specification.Message;
    using GamaEdtech.Presentation.ViewModel.Message;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Permission(policy: null)]
    public class MessagesController(Lazy<ILogger<MessagesController>> logger, Lazy<IMessageService> messageService)
        : ApiControllerBase<MessagesController>(logger)
    {
        [HttpGet("connections"), Produces(typeof(ApiResponse<IEnumerable<MessageConnectionResponseViewModel>>))]
        public async Task<IActionResult<IEnumerable<MessageConnectionResponseViewModel>>> GetConnections()
        {
            try
            {
                var result = await messageService.Value.GetMessageConnectionsAsync(new()
                {
                    UserId = User.UserId(),
                });

                return Ok<IEnumerable<MessageConnectionResponseViewModel>>(new(result.Errors)
                {
                    Data = result.Data?.Select(t => new MessageConnectionResponseViewModel
                    {
                        ConnectionId = t.ConnectionId,
                        ConnectionName = t.ConnectionName,
                        UnreadCount = t.UnreadCount,
                    }) ?? []
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<IEnumerable<MessageConnectionResponseViewModel>>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("connections/{connectionId:long}"), Produces<ApiResponse<ListDataSource<MessagesResponseViewModel>>>()]
        public async Task<IActionResult<ListDataSource<MessagesResponseViewModel>>> GetMessages([FromRoute] long connectionId, [NotNull, FromQuery] MessagesRequestViewModel request)
        {
            try
            {
                var result = await messageService.Value.GetMessagesAsync(new()
                {
                    PagingDto = request.PagingDto,
                    UserId = User.UserId(),
                    ConnectionId = connectionId,
                });
                return Ok<ListDataSource<MessagesResponseViewModel>>(new(result.Errors)
                {
                    Data = result.Data.List is null ? new() : new()
                    {
                        List = result.Data.List.Select(t => new MessagesResponseViewModel
                        {
                            Id = t.Id,
                            Body = t.Body,
                            CreationDate = t.CreationDate,
                            IsRead = t.IsRead,
                        }),
                        TotalRecordsCount = result.Data.TotalRecordsCount,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ListDataSource<MessagesResponseViewModel>>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost, Produces(typeof(ApiResponse<Void>))]
        [Display(Name = "Send Message")]
        public async Task<IActionResult> Create([NotNull] ManageMessageRequestViewModel request)
        {
            try
            {
                var result = await messageService.Value.ManageMessageAsync(new()
                {
                    UserId = User.UserId(),
                    Body = request.Body,
                    ConnectionId = request.ConnectionId.GetValueOrDefault(),
                });
                return Ok<Void>(new(result.Errors));
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new() { Errors = new[] { new Error { Message = exc.Message } } });
            }
        }

        [HttpPut("{id:long}"), Produces(typeof(ApiResponse<Void>))]
        [Display(Name = "Edit Message")]
        public async Task<IActionResult<Void>> Update([FromRoute] long id, [NotNull, FromBody] ManageMessageRequestViewModel request)
        {
            try
            {
                var result = await messageService.Value.ManageMessageAsync(new()
                {
                    Id = id,
                    UserId = User.UserId(),
                    Body = request.Body,
                    ConnectionId = request.ConnectionId.GetValueOrDefault(),
                });
                return Ok<Void>(new(result.Errors));
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new() { Errors = new[] { new Error { Message = exc.Message } } });
            }
        }

        [HttpPatch("{id:long}/toggle"), Produces(typeof(ApiResponse<Void>))]
        [Display(Name = "Toggle Message as readed/unreaded")]
        public async Task<IActionResult> Toggle([FromRoute] long id)
        {
            try
            {
                var result = await messageService.Value.ToggleMessageAsync(new()
                {
                    Id = id,
                    UserId = User.UserId(),
                });
                return Ok<Void>(new(result.Errors)
                {
                    Data = new()
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new() { Errors = new[] { new Error { Message = exc.Message } } });
            }
        }

        [HttpDelete("{id:long}"), Produces(typeof(ApiResponse<bool>))]
        [Display(Name = "Remove Message")]
        public async Task<IActionResult> Remove([FromRoute] long id)
        {
            try
            {
                var specification = new IdEqualsSpecification<Message, long>(id)
                    .And(new SenderIdEqualsSpecification(User.UserId()))
                    .And(new IsReadEqualsSpecification(false));
                var result = await messageService.Value.RemoveMessageAsync(specification);
                return Ok<bool>(new(result.Errors)
                {
                    Data = result.Data
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<bool>(new() { Errors = new[] { new Error { Message = exc.Message } } });
            }
        }
    }
}

