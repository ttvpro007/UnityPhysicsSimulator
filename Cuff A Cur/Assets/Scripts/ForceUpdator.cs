public enum EvaluateMethod
{
    Parabola,
    Linear
}

public static class ForceEvaluator
{
    public static float Evaluate(float maxForce, float lerpValue, EvaluateMethod method)
    {
        float eval = 0;

        switch (method)
        {
            case EvaluateMethod.Parabola:
                // parabola curve
                // with y = 1 at x = 0.5 
                // and  y = 0 at x = 0   or   1
                eval = -4 * lerpValue * (lerpValue - 1);
                break;
            case EvaluateMethod.Linear:
                // linear
                // with y = 1 at x = 0.5 
                // and  y = 0 at x = 0   or   1
                if (lerpValue >= 0.5)
                    eval = 2 * (-lerpValue + 1);
                else
                    eval = 2 * lerpValue;
                break;
        }

        return eval * maxForce;
    }
}