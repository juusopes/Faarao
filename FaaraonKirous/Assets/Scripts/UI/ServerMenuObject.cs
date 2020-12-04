using System.Net;
using UnityEngine.UI;

public class ServerMenuObject : BaseMenuObject
{
    public Text endpoint;
    public Text hasPassword;
    private ServerUIObject serverObject;

    public override void UpdateObject(int index, object inObj)
    {
        ServerUIObject server = (ServerUIObject)inObj;
        if (server == null) return;

        serverObject = server;
        nameText.text = server.Name;
        endpoint.text = server.EndPoint;
        hasPassword.text = server.HasPassword ? "true" : "false";
    }

    public void Connect()
    {
        IPEndPoint endPoint = NetTools.CreateIPEndPoint(serverObject.EndPoint);
        NetworkManager._instance.AttemptHandshake(serverObject.Guid, endPoint, MainMenuUIManager.Instance._password.text);
    }
}