using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Save
{
    public List<SavedObject> Objects { get { return _objects; } set { _objects = value; } }
    public Dictionary<ObjectList, int> Counters { get { return _counters; } set { _counters = value; } }
    public int SceneIndex { get { return _sceneIndex; } set { _sceneIndex = value; } }

    private List<SavedObject> _objects = new List<SavedObject>();
    private Dictionary<ObjectList, int> _counters = new Dictionary<ObjectList, int>();
    private int _sceneIndex;

    [System.Serializable]
    public class SavedObject
    {
        
        public bool IsStatic { get { return _isStatic; } set { _isStatic = value; } }
        public ObjectList List { get { return _list; } set { _list = value; } }
        public int Id { get { return _id; } set { _id = value; } }
        public ObjectType Type { get { return _type; } set { _type = value; } }
        public List<byte> Data { get { return _data; } set { _data = value; } }

        private bool _isStatic;
        private ObjectList _list;
        private int _id;
        private ObjectType _type;
        private List<byte> _data = new List<byte>();

        public SavedObject(ObjectManager objectManager)
        {
            // Read basic properties
            IsStatic = objectManager.IsStatic;
            Id = objectManager.Id;
            List = objectManager.List;
            Type = objectManager.Type;

            // Write data
            using (Packet packet = new Packet())
            {
                objectManager.WriteState(packet);
                Data = packet.ToArray().ToList();
            }
        }
    }
}
