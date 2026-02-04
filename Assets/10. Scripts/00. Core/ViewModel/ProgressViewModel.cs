using System;
using jjudy;

public class ProgressViewModel<T> : IDisposable where T : struct
{
    public event Action<float> OnRatioChanged;

    private readonly ObservableValue<T> current;
    private readonly ObservableValue<T> max;
    private readonly Func<T, float> toFloat;

    public float Ratio
    {
        get
        {
            float fCur = toFloat(current.Value);
            float fMax = toFloat(this.max.Value);

            float ratio = (fMax <= 0f) ? 0f : fCur / fMax;
            return ratio;
        }
    }

    public ProgressViewModel(ObservableValue<T> current, ObservableValue<T> max, Func<T, float> toFloat)
    {
        this.current = current;
        this.max = max;
        this.toFloat = toFloat;

        current.OnValueChanged += OnCurrentChanged;
        max.OnValueChanged += OnMaxChanged;

        Publish();
    }
    
    public void Dispose()
    {
        current.OnValueChanged -= OnCurrentChanged;
        max.OnValueChanged -= OnMaxChanged;
    }

    private void OnCurrentChanged(T prev, T cur) => Publish();
    private void OnMaxChanged(T prev, T cur) => Publish();

    public void Publish()
    {
        float fCur = toFloat(current.Value);
        float fMax = toFloat(this.max.Value);

        float ratio = (fMax <= 0f) ? 00f : fCur / fMax;
        OnRatioChanged?.Invoke(ratio);
    }
}