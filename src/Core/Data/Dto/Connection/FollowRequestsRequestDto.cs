namespace GamaEdtech.Data.Dto.Connection
{
    using GamaEdtech.Common.Data;

    public sealed class FollowRequestsRequestDto
    {
        public required int UserId { get; set; }
        public PagingDto? PagingDto { get; set; }
    }
}
