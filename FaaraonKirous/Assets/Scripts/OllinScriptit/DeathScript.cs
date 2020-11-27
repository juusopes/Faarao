using UnityEngine;


public class DeathScript : MonoBehaviour
{
    [SerializeField]
    public float hp;
    public float damage;
    public float heal;
    public bool isDead;

    private CharacterObjectManager CharacterNetManager { get; set; }

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if (NetworkManager._instance.IsHost)
        {
            DeathCheck();
            AliveCheck();
        }
    }

    private void Initialize() 
    {
        CharacterNetManager = GetComponent<CharacterObjectManager>();

        if (hp == 0)
        {
            hp = 1;
        }
        isDead = false;
    }

    private void DeathCheck()
    {
        if (damage > 0)
        {
            hp -= damage;
            if (hp < 0)
            {
                hp = 0;
            }
            damage = 0;
        }
        if (hp == 0 && !isDead)
        {
            isDead = true;

            if (NetworkManager._instance.ShouldSendToClient)
            {
                ServerSend.CharacterDied(CharacterNetManager.List, CharacterNetManager.Id);
            }
        }
        //if (isDead)
        //{
        //    Debug.Log(this.gameObject + "Is Dead!");
        //}
    }

    private void AliveCheck()
    {
        if (heal > 0)
        {
            hp += heal;
            if (hp > 1)
            {
                hp = 1;
            }
            heal = 0;
        }
        if (hp > 0)
        {
            isDead = false;

            if (NetworkManager._instance.ShouldSendToClient)
            {
                // TODO: Character revived
            }
        }
    }
}
