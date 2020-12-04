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
        var mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
        var database = mongoClient.GetDatabase("masterserver");
        _serverCollection = database.GetCollection<ServerDB>("servers");

        _bsonDocumentCollection = database.GetCollection<BsonDocument>("servers");
    }

    public async Task<ServerDB[]> GetAllServers()
    {
        return (await (await _serverCollection.FindAsync(new BsonDocument())).ToListAsync()).ToArray();
    }

    public async Task<ServerDB> GetServer(Guid id)
    {
        var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, id);
        return await (await _serverCollection.FindAsync(filter)).FirstOrDefaultAsync();
    }

    public async Task<ServerDB> CreateServer(ServerDB server)
    {
        await _serverCollection.InsertOneAsync(server);
        return server;
    }

    public async Task<ServerDB> DeleteServer(Guid id)
    {
        var filter = Builders<ServerDB>.Filter.Eq(s => s.Id, id);
        return await _serverCollection.FindOneAndDeleteAsync(filter);
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

