using UnityEngine;


public class DeathScript : MonoBehaviour
{
    [SerializeField]
    public float hp;
    public float damage;
    public float heal;
    public bool isDead;

    public SoundManager soundFX;

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
            Die();
        }
        //if (isDead)
        //{
        //    Debug.Log(this.gameObject + "Is Dead!");
        //}
    }

    public void Die(bool doMessage = true)
    {
        isDead = true;
        hp = 0;

        if(gameObject.CompareTag("Player"))
        {
            soundFX.PlayerDeathSound();
        }
        else
        {
            soundFX.DeathSound();
        }

        //if (doMessage && CharacterNetManager.List == ObjectList.player)
        //{
        //    MessageLog.Instance.AddMessage(
        //        $"{Tools.TypeToString(CharacterNetManager.Type)} died",
        //        Color.red);
        //}

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.CharacterDied(CharacterNetManager.List, CharacterNetManager.Id, doMessage);
        }
    }

    public void Revive(bool doMessage = true)
    {
        isDead = false;
        hp = 1;

        if (doMessage && CharacterNetManager.List == ObjectList.player)
        {
            MessageLog.Instance.AddMessage(
                $"{Tools.TypeToString(CharacterNetManager.Type)} was revived", 
                Color.green);
        }

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.CharacterRevived(CharacterNetManager.List, CharacterNetManager.Id, doMessage);
        }
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
        if (hp > 0 && isDead)
        {
            Revive();
        }
    }
}
