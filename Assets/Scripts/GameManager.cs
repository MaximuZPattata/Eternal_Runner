using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent playerConnected;

    private IEnumerator Start()
    {
        bool sessionConnected = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if(!response.success) 
            {
                Debug.Log("\nERROR : starting LootLocker session");
                
                return;
            }

            Debug.Log("SUCCESS : LootLocker session started");
            
            sessionConnected = true;
        });

        yield return new WaitUntil(() => sessionConnected);

        playerConnected.Invoke();
    }
}
