using AutoMapper;
using EventsLib;
using MassTransit;
using MatchService.DTOs;
using MatchService.Exceptions;
using MatchService.Features;
using MatchService.Interfaces;
using MatchService.Models;
using Microsoft.AspNetCore.SignalR;

namespace MatchService.Consumers
{
    public class TimedOutConsumer : IConsumer<TimedOut>
    {
        private readonly IMatchRepository _matchRepo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly IHubContext<MatchHub> _hubContext;

        public TimedOutConsumer(IMatchRepository matchRepo, IPublishEndpoint publishEndpoint, IMapper mapper, IHubContext<MatchHub> hubContext)
        {
            _matchRepo = matchRepo;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<TimedOut> context)
        {
            var match = await _matchRepo.GetMatchById(context.Message.MatchId);
            if (match == null)
                throw new MatchNotFoundException($"Match with id {context.Message.MatchId} wasn't found");

            if (match.Status != MatchStatus.InProgress)
                return;

            string? winner = null;

            if (context.Message.TimedOutSide == 0)
                winner = match.WhiteSidePlayer == match.Creator ? match.Acceptor : match.Creator;
            else if (context.Message.TimedOutSide == 1)
                winner = match.WhiteSidePlayer == match.Creator ? match.Creator : match.Acceptor;

            if (winner == null)
                return;

            match.EndedAtUtc = DateTime.UtcNow;
            match.Status = MatchStatus.Finished;
            match.ActingSide = null;
            match.DrawBy = null;
            match.WinBy = WinDescriptor.OnTime;
            match.Winner = winner;

            await _matchRepo.SaveChangesAsync();

            await _publishEndpoint.Publish(_mapper.Map<MatchFinished>(match));

            var matchFinishedDto = _mapper.Map<MatchFinishedDto>(match);

            await _hubContext.Clients.Group(context.Message.MatchId.ToString()).SendAsync("MatchFinished", matchFinishedDto);
        }
    }
}
