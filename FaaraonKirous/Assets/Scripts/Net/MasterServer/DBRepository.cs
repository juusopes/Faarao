using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DBRepository
{
    private IMongoCollection<ServerDB> _serverCollection;
    private IMongoCollection<BsonDocument> _bsonDocumentCollection;

    public DBRepository()
    {
        var mongoClient = new MongoClient("mongodb://localhost:27017?connect=replicaSet");
        var database = mongoClient.GetDatabase("masterserver");
        _serverCollection = database.GetCollection<ServerDB>("servers");

        _bsonDocumentCollection = database.GetCollection<BsonDocument>("servers");

    }

    public ServerDB[] GetAllServers()
    {
        return _serverCollection.Find(new BsonDocument()).ToList().ToArray();
    }

    public ServerDB GetServer(Guid id)
    {
        var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, id);
        return _serverCollection.Find(filter).FirstOrDefault();
    }

    public ServerDB CreateServer(ServerDB server)
    {
        _serverCollection.InsertOne(server);
        return server;
    }

    public ServerDB DeleteServer(Guid id)
    {
        var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, id);
        return _serverCollection.FindOneAndDelete(filter);
    }

    //public async Task<ServerDB> PlayerConnected(Guid serverId, Guid playerId)
    //{
    //    var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, serverId);
    //    var update = Builders<ServerDB>.Update.Push(s => s.Players, playerId);
    //    var options = new FindOneAndUpdateOptions<ServerDB>()
    //    {
    //        ReturnDocument = ReturnDocument.After
    //    };
    //    return await _serverCollection.FindOneAndUpdateAsync(filter, update, options);
    //}

    //public async Task<ServerDB> PlayerDisconnected(Guid serverId, Guid playerId)
    //{
    //    var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, serverId);
    //    var update = Builders<ServerDB>.Update.Pull(s => s.Players, playerId);
    //    var options = new FindOneAndUpdateOptions<ServerDB>()
    //    {
    //        ReturnDocument = ReturnDocument.After
    //    };
    //    return await _serverCollection.FindOneAndUpdateAsync(filter, update, options);
    //}
}

