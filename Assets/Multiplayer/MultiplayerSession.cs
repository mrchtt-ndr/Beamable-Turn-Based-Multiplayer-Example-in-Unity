using UnityEngine.Events;
using System.Collections.Generic;
using Beamable;
using Beamable.Common.TicTacToe;
using Beamable.Server.Clients;
using System;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerEvent : UnityEvent<string, MoveEvent> { }

    public class MultiplayerSession
    {
        /// <summary>
        /// Invoked when someone else's move is received. Passed arguments are the MatchId and the MoveEvent.
        /// </summary>
        public MultiplayerEvent OnOpponentMoveReceived { get; }

        public string MatchId { get; }
        public string ChannelName { get { return string.Concat(NotificationChannels.Prefix.MultiplayerChannel, MatchId); } }

        private readonly List<long> _players = new();
        public IReadOnlyList<long> Players { get { return _players; } }

        private readonly List<MoveEvent> _movesHistory = new();
        public IReadOnlyList<MoveEvent> MovesHistory { get { return _movesHistory.AsReadOnly(); } }

        public MultiplayerSession (string matchId, List<long> players)
        {
            OnOpponentMoveReceived = new();
            MatchId = matchId;
            _players = players;
            _movesHistory = new();
        }

        public void Open()
        {
            BeamContext.Default.Api.NotificationService.Subscribe<MoveEvent>(ChannelName, EventHandler);
        }

        private void EventHandler(MoveEvent moveEvent)
        {
            _movesHistory.Add(moveEvent);

            if (moveEvent.PlayerId != BeamContext.Default.PlayerId)
            {
                OnOpponentMoveReceived.Invoke(MatchId, moveEvent);
            }
        }

        public async void SendEvent(MoveEvent moveEvent)
        {
            try
            {
                await BeamContext.Default.Microservices().MultiplayerMicroservice().BroadcastMove(ChannelName, _players, moveEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending move event: {ex.Message}");
            }
        }

        public void Close()
        {
            OnOpponentMoveReceived.RemoveAllListeners();
            BeamContext.Default.Api.NotificationService.Unsubscribe<MoveEvent>(ChannelName, EventHandler);
        }
    }
}