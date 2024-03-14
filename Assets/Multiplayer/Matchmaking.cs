using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Multiplayer
{
    public class MatchmakingEvent : UnityEvent<MatchmakingResult> { }

    /// <summary>
    /// Contains the in-progress matchmaking data.
    /// When the process is complete, this contains
    /// the players list and the MatchId
    /// </summary>
    [Serializable]
    public class MatchmakingResult
    {
        //  Properties  -------------------------------------
        public List<long> Players
        {
            get
            {
                if (_matchmakingHandle == null || _matchmakingHandle.Status == null ||
                    _matchmakingHandle.Status.Players == null)
                {
                    return new List<long>();
                }
                return _matchmakingHandle.Status.Players.Select(i => long.Parse(i)).ToList();
            }
        }

        public int PlayerCountMin
        {
            get
            {
                int playerCountMin = 0;
                foreach (TeamContent teamContent in _simGameType.teams)
                {
                    if (teamContent.minPlayers.HasValue)
                    {
                        playerCountMin += teamContent.minPlayers.Value;
                    }
                }
                return playerCountMin;
            }
        }

        public int PlayerCountMax
        {
            get
            {
                int playerCountMax = 0;
                foreach (TeamContent teamContent in _simGameType.teams)
                {
                    playerCountMax += teamContent.maxPlayers;
                }
                return playerCountMax;
            }
        }

        public string MatchId
        {
            get
            {
                return _matchmakingHandle?.Match?.matchId;
            }
        }

        public long LocalPlayer { get { return _localPlayer; } }
        public SimGameType SimGameType { get { return _simGameType; } }
        public MatchmakingHandle MatchmakingHandle { get { return _matchmakingHandle; } set { _matchmakingHandle = value; } }
        public bool IsInProgress { get { return _isInProgress; } set { _isInProgress = value; } }

        //  Fields  -----------------------------------------
        public string ErrorMessage = "";
        private long _localPlayer;
        private bool _isInProgress = false;
        private MatchmakingHandle _matchmakingHandle = null;
        private SimGameType _simGameType;

        //  Constructor  ------------------------------------
        public MatchmakingResult(SimGameType simGameType, long localPlayer)
        {
            _simGameType = simGameType;
            _localPlayer = localPlayer;
        }

        //  Other Methods  ----------------------------------
        public override string ToString()
        {
            return $"[MatchmakingResult (" +
               $"MatchId = {MatchId}, " +
               $"Teams = {_matchmakingHandle?.Match?.teams}, " +
               $"players.Count = {Players?.Count})]";
        }
    }

    /// <summary>
    /// This example is for reference only. Use as
    /// inspiration for usage in production.
    /// </summary>
    public class MatchmakingService
    {
        //  Events  -----------------------------------------
        public MatchmakingEvent OnProgress = new MatchmakingEvent();
        public MatchmakingEvent OnComplete = new MatchmakingEvent();
        public MatchmakingEvent OnError = new MatchmakingEvent();


        //  Properties  -------------------------------------
        public MatchmakingResult MyMatchmakingResult { get { return _myMatchmakingResult; } }


        //  Fields  -----------------------------------------
        private MatchmakingResult _myMatchmakingResult = null;
        private Beamable.Experimental.Api.Matchmaking.MatchmakingService _matchmakingService = null;
        public const string TimeoutErrorMessage = "Timeout";


        //  Constructor  ------------------------------------
        public MatchmakingService(Beamable.Experimental.Api.Matchmaking.MatchmakingService matchmakingService,
           SimGameType simGameType, long localPlayerDbid)
        {
            _matchmakingService = matchmakingService;
            _myMatchmakingResult = new MatchmakingResult(simGameType, localPlayerDbid);
        }


        //  Other Methods  ----------------------------------
        public async Task StartMatchmaking()
        {
            if (_myMatchmakingResult.IsInProgress)
            {
                Debug.LogError($"Matchmaking.StartMatchmaking() failed. " +
                               $"IsInProgress must not be {_myMatchmakingResult.IsInProgress}.\n\n");
                return;
            }

            _myMatchmakingResult.IsInProgress = true;

            _myMatchmakingResult.MatchmakingHandle = await _matchmakingService.StartMatchmaking(
               _myMatchmakingResult.SimGameType.Id,
               updateHandler: handle =>
               {
                   OnUpdateHandler(handle);
               },
               readyHandler: handle =>
               {
                   // Call both
                   OnUpdateHandler(handle);
                   OnReadyHandler(handle);
               },
               timeoutHandler: handle =>
               {
                   // Call both
                   OnUpdateHandler(handle);
                   OnTimeoutHandler(handle);
               });
        }


        public async Task CancelMatchmaking()
        {
            await _matchmakingService.CancelMatchmaking(_myMatchmakingResult.MatchmakingHandle.Tickets[0].ticketId);
        }


        //  Event Handlers  ----------------------------------
        private void OnUpdateHandler(MatchmakingHandle handle)
        {
            OnProgress.Invoke(_myMatchmakingResult);
        }

        private void OnReadyHandler(MatchmakingHandle handle)
        {
            Debug.Assert(handle.State == MatchmakingState.Ready);
            _myMatchmakingResult.IsInProgress = false;
            OnComplete.Invoke(_myMatchmakingResult);
        }

        private void OnTimeoutHandler(MatchmakingHandle handle)
        {
            _myMatchmakingResult.IsInProgress = false;
            _myMatchmakingResult.ErrorMessage = TimeoutErrorMessage;
            OnError?.Invoke(_myMatchmakingResult);
        }
    }
}