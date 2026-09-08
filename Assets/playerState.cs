using UnityEngine;

public class playerState : MonoBehaviour
{
    [SerializeField] BgTrackController bgTrackController;

    public int progression = 0;

    private void Update()
    {
        if (progression == 0)
        {
            return;
        }
        else if (progression == 1)
        {
            bgTrackController.UnmuteTrack01();
        }
        else if (progression == 2)
        {
            bgTrackController.UnmuteTrack02();
        }
    }
}
