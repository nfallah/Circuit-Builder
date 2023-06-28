using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitCaller : MonoBehaviour
{
    private static CircuitCaller instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            throw new Exception("CircuitCaller instance already established; terminating.");
        }

        instance = this;
    }

    public static void InitiateUpdateCalls(List<Circuit.UpdateCall> updateCalls)
    {
        instance.StartCoroutine(UpdateCalls(updateCalls));
    }

    private static IEnumerator UpdateCalls(List<Circuit.UpdateCall> updateCalls)
    {
        yield return new WaitForSeconds(Circuit.callTimer);

        foreach (Circuit.UpdateCall updateCall in updateCalls)
        {
            Circuit.UpdateCircuit(updateCall.Powered, updateCall.Input, updateCall.Output);
        }
    }
}
