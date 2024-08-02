using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class NetworkController : NetworkBehaviour
{
    public static NetworkController instance;

    private string joinCode;

    [SerializeField] private GameObject game;
    [SerializeField] private GameObject networking;

    [SerializeField] private Button hostButton, clientButton;

    [SerializeField] private GameObject placeholderPanel;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject codeArea;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        joinCode = "";
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private async void Start()
    {
        game.SetActive(false);
        placeholderPanel.SetActive(false);

        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log(clientId + " joined");
            if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                StartGame();
            }
        };

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void StartGame()
    {
        SpawnBoardRpc();
        DisableNetworkUIRpc();
    }

    public async void StartHost()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            game.transform.Find("PlayerX").transform.GetComponentInChildren<Button>().interactable = true;
            game.transform.Find("PlayerO").transform.GetComponentInChildren<Button>().interactable = true;
            OnlineGameController.isHost = true;
            ActivatePlaceholderPanel(true);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void StartClient()
    {
        ActivatePlaceholderPanel(false);
    }

    public async void JoinGame()
    {
        try
        {
            Debug.Log(codeArea.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text);
            string code = codeArea.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text;
            code = code.Substring(0, 6);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            game.transform.Find("PlayerX").transform.GetComponentInChildren<Button>().interactable = false;
            game.transform.Find("PlayerO").transform.GetComponentInChildren<Button>().interactable = false;
            OnlineGameController.isHost = false;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
            NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager);
    }

    private void ActivatePlaceholderPanel(bool isHost)
    {
        placeholderPanel.SetActive(true);
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        if (isHost)
        {
            labelText.text = "Your Code:";
            codeArea.GetComponent<TMP_InputField>().interactable = false;
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = joinCode;
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            joinButton.interactable = false;
            joinButton.GetComponent<Image>().sprite = null;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "waiting for client...";
        }
        else if (!isHost)
        {
            labelText.text = "Enter Code:";
            codeArea.GetComponent<TMP_InputField>().interactable = true;
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(152, 152, 152);
            joinButton.interactable = true;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Join Game";
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 32;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DisableNetworkUIRpc()
    {
        networking.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnBoardRpc()
    {
        game.SetActive(true);
    }
}
