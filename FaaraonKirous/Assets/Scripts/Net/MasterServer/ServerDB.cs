using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ServerDB
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EndPoint { get; set; }
    public List<Guid> Players { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime CreationDate { get; set; }
    public List<Guid> BannedPlayers { get; set; }
    public bool HasPassword { get; set; }

    public ServerDB(string name, string endPoint, bool hasPassword)
    {
        Id = Guid.NewGuid();
        Name = name;
        EndPoint = endPoint;
        Players = new List<Guid>();
        MaxPlayers = Constants.maxPlayers;
        CreationDate = DateTime.Now;
        BannedPlayers = new List<Guid>();
        HasPassword = hasPassword;
    }
}

