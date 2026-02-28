using UnityEngine;

public class SeatedNPC : MonoBehaviour
{
    public AudioSource talkSound;
    public AudioSource clapSound;

    void Start()
    {
        InvokeRepeating(nameof(DoRandomAction), 3f, Random.Range(4f, 7f));
    }

    void DoRandomAction()
    {
        if (Random.value < 0.5f)
        {
            Debug.Log("NPC is talking");
            if (talkSound) talkSound.Play();
        }
        else
        {
            Debug.Log("NPC is clapping");
            if (clapSound) clapSound.Play();
        }
    }
}
