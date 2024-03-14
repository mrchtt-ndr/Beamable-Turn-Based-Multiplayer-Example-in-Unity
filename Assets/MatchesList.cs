using System.Collections.Generic;
using UnityEngine;
using Multiplayer;
using TMPro;
using UnityEngine.UI;


public class MatchesList : MonoBehaviour
{
    [SerializeField] private Transform _host;
    [SerializeField] private GameObject _itemTemplate;

    public void SetContent (IReadOnlyList<MultiplayerSession> sessions)
    {
        foreach (Transform child in _host)
            Destroy(child.gameObject);

        foreach (var session in sessions)
        {
            var item = Instantiate(_itemTemplate, _host);
            item.GetComponentInChildren<TextMeshProUGUI>().text = session.ChannelName;
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameManager.Instance.ChangeMatch(session.MatchId);
            });
            item.SetActive(true);
        }
    }
}
