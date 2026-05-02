namespace GamaEdtech.Application.Interface
{
    using System.Diagnostics.CodeAnalysis;

    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.DataAccess.Specification;
    using GamaEdtech.Common.DataAnnotation;
    using GamaEdtech.Data.Dto.Message;
    using GamaEdtech.Domain.Entity;

    [Injectable]
    public interface IMessageService
    {
        Task<ResultData<IEnumerable<MessageConnectionDto>>> GetMessageConnectionsAsync([NotNull] MessageConnectionsRequestDto requestDto);
        Task<ResultData<ListDataSource<MessageDto>>> GetMessagesAsync([NotNull] MessagesRequestDto requestDto);
        Task<ResultData<bool>> ToggleMessageAsync([NotNull] ToggleMessageRequestDto requestDto);
        Task<ResultData<long>> ManageMessageAsync([NotNull] ManageMessageRequestDto requestDto);
        Task<ResultData<bool>> RemoveMessageAsync([NotNull] ISpecification<Message> specification);
    }
}
