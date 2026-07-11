using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkedTime : NetworkIdentity
{
    public SyncVar<float> TimeScale = new(1f);
    public float ClientTimeScale = 0f;
    public SyncVar<float> ServerSimulationTime = new(0f);
    public float ClientSimulationTime = 0f;
    private InputAction pause;
    private InputAction resume;

    void Update()
    {
        if (isServer)
        {
            ServerSimulationTime.value += Time.deltaTime * TimeScale.value; 
        }

        ClientSimulationTime += Time.deltaTime * ClientTimeScale;

        if (pause.WasPressedThisFrame())
        {
            ClientTimeScale = 0f;
            SetTimeScale(0f);
        }
        else if (resume.WasPressedThisFrame())
        {
            ClientTimeScale = 1f;
            SetTimeScale(1f);
        }
    }

    private void OnEnable()
    {
        ServerSimulationTime.onChanged += OnSimulationTimeUpdate;
        TimeScale.onChanged += OnTimeScaleUpdate;

        pause = InputSystem.actions.FindAction("Pause");
        resume = InputSystem.actions.FindAction("UnPause");
    }

    private void OnDisable()
    {
        ServerSimulationTime.onChanged -= OnSimulationTimeUpdate;
        TimeScale.onChanged -= OnTimeScaleUpdate;
    }

    private void OnSimulationTimeUpdate(float newValue)
    {
        ClientSimulationTime = newValue;
        //Debug.Log($"ClientSimulationTime updated to {newValue}");
    }

    private void OnTimeScaleUpdate(float newValue)
    {
        ClientTimeScale = newValue;
        //Debug.Log($"ClientTimeScale updated to {newValue}");
    }

    [ServerRpc]
    public void SetSimulationTime(float newTime = 0f)
    {
        ServerSimulationTime.value = newTime;
    }

    [ServerRpc]
    public void SetTimeScale(float newTimeScale)
    {
        TimeScale.value = newTimeScale;
        //Debug.Log($"TimeScale set to {newTimeScale}");
    }
}
