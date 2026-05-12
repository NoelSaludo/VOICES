public class GameTimer
{
    private float elapsedTime;

    public float ElapsedTime => elapsedTime;

    public void Tick(float deltaTime)
    {
        elapsedTime += deltaTime;
    }

    public void Reset()
    {
        elapsedTime = 0f;
    }
}
