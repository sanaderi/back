namespace GamaEdtech.Application.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Application.Interface;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Core.Extensions.Linq;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAccess.UnitOfWork;
    using GamaEdtech.Common.Service;

    using GamaEdtech.Data.Dto.Message;
    using GamaEdtech.Domain.Entity;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    using static GamaEdtech.Common.Core.Constants;

    public class MessageService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider, Lazy<IHttpContextAccessor> httpContextAccessor, Lazy<IStringLocalizer<MessageService>> localizer
        , Lazy<ILogger<MessageService>> logger)
        : LocalizableServiceBase<MessageService>(unitOfWorkProvider, httpContextAccessor, localizer, logger), IMessageService
    {
        public async Task<ResultData<IEnumerable<MessageConnectionDto>>> GetMessageConnectionsAsync([NotNull] MessageConnectionsRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Message>();
                var lst = await repository.GetManyQueryable(t => t.SenderId == requestDto.UserId || t.ReceiverId == requestDto.UserId).Select(t => new
                {
                    ConnectionId = t.SenderId == requestDto.UserId ? t.ReceiverId : t.SenderId,
                    ConnectionName = t.SenderId == requestDto.UserId ? (t.Receiver.FirstName + " " + t.Receiver.LastName) : (t.Sender.FirstName + " " + t.Sender.LastName),
                }).Distinct().ToListAsync();

                var unreadData = await repository.GetManyQueryable(t => t.ReceiverId == requestDto.UserId && !t.IsRead).GroupBy(t => t.SenderId).Select(t => new
                {
                    ConnectionId = t.Key,
                    Count = t.Count(),
                }).ToListAsync();

                return new(OperationResult.Succeeded)
                {
                    Data = lst.Select(t => new MessageConnectionDto
                    {
                        ConnectionId = t.ConnectionId,
                        ConnectionName = t.ConnectionName,
                        UnreadCount = unreadData?.Find(r => r.ConnectionId == t.ConnectionId)?.Count ?? 0,
                    })
                };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<ListDataSource<MessageDto>>> GetMessagesAsync([NotNull] MessagesRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var lst = await uow.GetRepository<Message>().GetManyQueryable(t => (t.SenderId == requestDto.UserId && t.ReceiverId == requestDto.ConnectionId) || (t.SenderId == requestDto.ConnectionId && t.ReceiverId == requestDto.UserId)).FilterListAsync(requestDto.PagingDto);
                var result = await lst.List.Select(t => new MessageDto
                {
                    Id = t.Id,
                    Body = t.Body,
                    CreationDate = t.CreationDate,
                    IsRead = t.IsRead,
                }).ToListAsync();

                return new(OperationResult.Succeeded) { Data = new() { List = result, TotalRecordsCount = lst.TotalRecordsCount } };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<bool>> ToggleMessageAsync([NotNull] ToggleMessageRequestDto requestDto)
        {
            try
            {
                var affectedRows = await UnitOfWorkProvider.Value.CreateUnitOfWork().GetRepository<Message>().GetManyQueryable(t => t.Id == requestDto.Id && t.ReceiverId == requestDto.UserId)
                    .ExecuteUpdateAsync(t => t.SetProperty(p => p.IsRead, p => !p.IsRead));

                return new(OperationResult.Succeeded)
                {
                    Data = affectedRows > 0,
                };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message },] };
            }
        }

        public async Task<ResultData<long>> ManageMessageAsync([NotNull] ManageMessageRequestDto requestDto)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Message>();
                Message? message = null;

                if (requestDto.Id.HasValue)
                {
                    message = await repository.GetAsync(t => t.Id == requestDto.Id.Value && t.SenderId == requestDto.UserId && t.ReceiverId == requestDto.ConnectionId);
                    if (message is null)
                    {
                        return new(OperationResult.NotFound)
                        {
                            Errors = [new() { Message = Localizer.Value["MessageNotFound"] },],
                        };
                    }

                    message.Body = requestDto.Body;

                    _ = repository.Update(message);
                }
                else
                {
                    message = new Message
                    {
                        SenderId = requestDto.UserId,
                        ReceiverId = requestDto.ConnectionId,
                        Body = requestDto.Body,
                        CreationDate = DateTimeOffset.UtcNow,
                        IsRead = false,
                    };
                    repository.Add(message);
                }

                _ = await uow.SaveChangesAsync();

                return new(OperationResult.Succeeded) { Data = message.Id };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, }] };
            }
        }

        public async Task<ResultData<bool>> RemoveMessageAsync([NotNull] ISpecification<Message> specification)
        {
            try
            {
                var uow = UnitOfWorkProvider.Value.CreateUnitOfWork();
                var repository = uow.GetRepository<Message>();
                var message = await repository.GetAsync(specification);
                if (message is null)
                {
                    return new(OperationResult.NotFound)
                    {
                        Data = false,
                        Errors = [new() { Message = Localizer.Value["MessageNotFound"] },],
                    };
                }

                repository.Remove(message);
                _ = await uow.SaveChangesAsync();
                return new(OperationResult.Succeeded) { Data = true };
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);
                return new(OperationResult.Failed) { Errors = [new() { Message = exc.Message, },] };
            }
        }
    }
}
