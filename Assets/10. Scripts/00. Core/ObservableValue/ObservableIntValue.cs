namespace jjudy
{
    public class ObservableIntValue : ObservableValue<int>
    {
        public ObservableIntValue() : base() { }
        public ObservableIntValue(int initValue) : base(initValue) { }

        public static int operator +(ObservableIntValue a, ObservableIntValue b) => a.Value + b.Value;
        public static int operator -(ObservableIntValue a, ObservableIntValue b) => a.Value - b.Value;
        
        public void Add(int delta)
        {
            Value = Value + delta;
        }
        
        public void Subtract(int delta)
        {
            Value = Value - delta;
        }
    }
}