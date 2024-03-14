using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Beamable.Common.Content;
using Beamable;
using TMPro;

namespace Multiplayer
{
    public enum MatchmakerState { Ready, Started, Searching, Error, Completed }

    public class MatchmakerEvent : UnityEvent<MatchmakingResult> { }

    public class Matchmaker : MonoBehaviour
    {
        [SerializeField] private SimGameTypeRef _simGameTypeRef;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private TextMeshProUGUI _stateText;

        public MatchmakerEvent OnMatchFound = new();

        private MatchmakingService _matchmaking;
        private MatchmakingResult _matchmakingResult;

        private MatchmakerState _state = MatchmakerState.Ready;
        private void SetState(MatchmakerState state, string overrideMsg = null)
        {
            _state = state;
            _stateText.text = overrideMsg ?? _state.ToString();
            if (_state == MatchmakerState.Ready || _state == MatchmakerState.Error || _state == MatchmakerState.Completed)
                _buttonText.text = "Find Match";
            else if (_state == MatchmakerState.Started)
                _buttonText.text = "...";
            else if (_state == MatchmakerState.Searching)
                _buttonText.text = "Stop Match";
        }

        public bool IsAvailable { get { return _state == MatchmakerState.Ready || _state == MatchmakerState.Error || _state == MatchmakerState.Completed; } }


        public async void StartRandomMatch()
        {
            if (IsAvailable)
            {
                SetState(MatchmakerState.Started);

                SimGameType gameType = await _simGameTypeRef.Resolve();
                _matchmaking = new(
                        BeamContext.Default.Api.Experimental.MatchmakingService,
                        gameType,
                        BeamContext.Default.PlayerId);

                _matchmaking.OnProgress.AddListener(OnMatchMakingProgress);
                _matchmaking.OnComplete.AddListener(OnMatchMakingComplete);
                _matchmaking.OnError.AddListener(OnMatchMakingError);

                await _matchmaking.StartMatchmaking();
            }
            else if (_state != MatchmakerState.Started)  // if it has just started the matchmaking might be initializing. Can't be stopped now
            {
                Reset();
            }
        }

        private void OnMatchMakingProgress(MatchmakingResult match)
        {
            SetState(MatchmakerState.Searching);
            _matchmakingResult = match;
        }

        private void OnMatchMakingComplete(MatchmakingResult match)
        {
            SetState(MatchmakerState.Completed);
            _matchmakingResult = match;

            OnMatchFound.Invoke(match);
        }

        private void OnMatchMakingError(MatchmakingResult result)
        {
            SetState(MatchmakerState.Error, result.ErrorMessage);
            _matchmakingResult = result;

            Reset();
        }

        public async void Reset()
        {
            await _matchmaking?.CancelMatchmaking();
            SetState(MatchmakerState.Ready);
            _matchmakingResult = null;
        }
    }
}