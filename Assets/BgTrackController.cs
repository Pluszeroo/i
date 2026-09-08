using UnityEngine;

public class BgTrackController : MonoBehaviour
{
    [SerializeField] private Animator bgTrack01;
    [SerializeField] private Animator bgTrack02;
    public void UnmuteTrack01()
    {
        bgTrack01.SetTrigger("Unmute01");
    }
    public void UnmuteTrack02()
    {
        bgTrack02.SetTrigger("Unmute02");
    }
}
