using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Multiplayer
{
    public class MultiplayerService
    {
        private readonly List<MultiplayerSession> _multiplayerSessions = new();
        public IReadOnlyList<MultiplayerSession> MultiplayerSessions { get { return _multiplayerSessions.AsReadOnly(); } }

        /// <summary>
        /// Opens a multiplayer session anew or returns an existing one.
        /// </summary>
        public MultiplayerSession OpenSession(string matchId, List<long> playerIds)
        {
            var session = GetSession(matchId);

            if (session == null)
            {
                session = new MultiplayerSession(matchId, playerIds);
                session.Open();
                _multiplayerSessions.Add(session);
            }

            return session;
        }

        public bool IsSessionOpen(string matchId)
        {
            return GetSession(matchId) != null;
        }

        /// <summary>
        /// Returns an existing open session. Can be null if it doesn't exist.
        /// </summary>
        public MultiplayerSession GetSession(string matchId)
        {
            return _multiplayerSessions.Find(session => string.Equals(session.MatchId, matchId));
        }

        public void CloseSession(string matchId)
        {
            var session = GetSession(matchId);

            if (session != null)
            {
                session.Close();
                _multiplayerSessions.Remove(session);
            }
            else
                Debug.LogWarning("Trying to close a session that doesn't exist.");
        }
    }
}