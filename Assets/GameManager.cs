using UnityEngine;
using TMPro;
using Beamable;
using Beamable.Common.TicTacToe;
using Multiplayer;
using TicTacToe;
using System.Collections.Generic;

public enum GameState { Inactive, PlayerTurn, OpponentTurn, Lost, Draw, Won }

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private TextMeshProUGUI _gameStateText;
    [SerializeField] private Matchmaker _matchmaker;
    [SerializeField] private Board _board;
    [SerializeField] private MatchesList _matchesList;

    public long PlayerId { get { return BeamContext.Default.PlayerId; } }
    public Cell.Sign CurrentPlayerSign { get; private set; }

    private MultiplayerService _multiplayerService;
    private MultiplayerSession _currentMultiplayer;
    public MultiplayerSession CurrentMultiplayer => _currentMultiplayer;

    private GameState _gameState = GameState.Inactive;
    private void SetGameState (GameState state)
    {
        _gameState = state;
        _board.IsActive = _gameState == GameState.PlayerTurn;

        switch (_gameState)
        {
            case GameState.Inactive:
                _gameStateText.text = "Start a new match.";
                break;
            case GameState.PlayerTurn:
                _gameStateText.text = "It's your turn.";
                break;
            case GameState.OpponentTurn:
                _gameStateText.text = "Waiting for the opponent...";
                break;
            case GameState.Lost:
                _gameStateText.text = "You lost.";
                break;
            case GameState.Draw:
                _gameStateText.text = "It's a draw.";
                break;
            case GameState.Won:
                _gameStateText.text = "You won!";
                break;
        }

        if (state == GameState.Lost || state == GameState.Draw || state == GameState.Won)
        {
            _multiplayerService.CloseSession(_currentMultiplayer?.MatchId);
            _currentMultiplayer = null;
            _matchmaker.Reset();
            _matchesList.SetContent(_multiplayerService.MultiplayerSessions);
        }
    }


    protected override async void OnAwake()
    {
        await BeamContext.Default.OnReady;

        _multiplayerService = new();

        _matchmaker.OnMatchFound.AddListener(StartMultiplayer);
    }

    private void Start()
    {
        SetGameState(GameState.Inactive);
    }

    private void StartMultiplayer (MatchmakingResult matchmakingResult)
    {
        _currentMultiplayer = _multiplayerService.OpenSession(matchmakingResult.MatchId, matchmakingResult.Players);
        _currentMultiplayer.OnOpponentMoveReceived.AddListener(ReceiveOpponentMove);

        _board.Reset();
        AssignSigns(matchmakingResult.Players);

        _matchesList.SetContent(_multiplayerService.MultiplayerSessions);
    }

    public void ChangeMatch(string matchId)
    {
        _currentMultiplayer?.OnOpponentMoveReceived.RemoveListener(ReceiveOpponentMove);
        _currentMultiplayer = _multiplayerService.GetSession(matchId);

        if (_currentMultiplayer != null)
        {
            _currentMultiplayer.OnOpponentMoveReceived.AddListener(ReceiveOpponentMove);

            _board.Reset();
            AssignSigns(new List<long>(_currentMultiplayer.Players));

            GameState? gameState = null;
            foreach (var moveEvent in _currentMultiplayer.MovesHistory)
                gameState = _board.WriteCell(moveEvent.Position, moveEvent.PlayerId == GameManager.Instance.PlayerId);
            if (gameState.HasValue)
                SetGameState(gameState.Value);
        }
        else
            Debug.LogWarning("Trying to recover a match that doesn't exist.");
    }

    private void AssignSigns(List<long> players)
    {
        // Assign the same signs to both players, assuming there can't be more than 2 players per match.
        int localPlayerIndex = players.FindIndex(player => player == PlayerId);
        var signValue = localPlayerIndex % 2;
        CurrentPlayerSign = signValue == 0 ? Cell.Sign.O : CurrentPlayerSign = Cell.Sign.X;

        // Give to the X player the first move.
        if (CurrentPlayerSign == Cell.Sign.X)
            SetGameState(GameState.PlayerTurn);
        else
            SetGameState(GameState.OpponentTurn);
    }

    private void ReceiveOpponentMove (string matchId, MoveEvent moveEvent)
    {
        if (string.Equals(matchId, _currentMultiplayer.MatchId))
        {
            var gameState = _board.WriteCell(moveEvent.Position, false);
            SetGameState(gameState);
        }
    }

    public void SendPlayerMove (CellPosition position)
    {
        var moveEvent = new MoveEvent { PlayerId = PlayerId, Position = position };
        _currentMultiplayer.SendEvent(moveEvent);

        var gameState = _board.WriteCell(moveEvent.Position, true);
        SetGameState(gameState);
    }
}
