using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Save
{
    public List<SavedObject> Objects { get; set; }
        = new List<SavedObject>();
    public Dictionary<ObjectList, int> Counters { get; set; }
        = new Dictionary<ObjectList, int>();

    public int SceneIndex { get; set; }

    [System.Serializable]
    public class SavedObject
    {
        public bool IsStatic { get; set; }
        public ObjectList List { get; set; }
        public int Id { get; set; }
        public ObjectType Type { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();

        public SavedObject(ObjectManager objectManager)
        {
            // Read basic properties
            IsStatic = objectManager.IsUnique;
            Id = objectManager.Id;
            List = objectManager.List;
            Type = objectManager.Type;
            Position = objectManager.Transform.position;
            Rotation = objectManager.Transform.rotation;

            // Read data
            using (Packet packet = new Packet())
            {
                objectManager.WriteState(packet);
                Data = packet.ToArray().ToList();
            }
        }
    }
}
