using UnityEngine;

public static class StateValueLerping
{
    public static float StateLerping01(UpdatorState state, float currentValue, float secondToFilled)
    {
        if (secondToFilled == 0) return currentValue;
        float updateValue = 1 / secondToFilled * Time.fixedDeltaTime;

        if (updateValue == 0 || state == UpdatorState.Stop) return currentValue;

        state = GetLerpingState(state, currentValue);

        switch (state)
        {
            case UpdatorState.Up:
                currentValue += updateValue;
                break;
            case UpdatorState.Down:
                currentValue -= updateValue;
                break;
        }

        return currentValue;
    }

    public static UpdatorState GetLerpingState(UpdatorState currentState, float currentValue)
    {
        switch (currentState)
        {
            case UpdatorState.Up:
                currentValue = Mathf.Min(currentValue, 1); // clamp to max force value
                if (currentValue == 1)
                    return UpdatorState.Down; // RETURN down state to down if reached 1
                break;
            case UpdatorState.Down:
                currentValue = Mathf.Max(currentValue, 0); // clamp to min force value
                if (currentValue == 0)
                    return UpdatorState.Up; // RETURN up state to up if reached 0
                break;
        }

        return currentState; // RETURN current state if none of the conditions above is met
    }
}
