public struct TimeScaleChangedEvent : IEvent
{
    public float NewScale { get; }
    public TimeScaleChangedEvent(float newScale)
    {
        NewScale = newScale;
    }
}
